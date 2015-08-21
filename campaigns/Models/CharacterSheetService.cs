using campaigns.Models.DAL;
using Model.Calculations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace campaigns.Models
{
    public interface ICharacterSheetService
    {
        CharacterSheet CreateCharacterSheet();
        CharacterSheet EnsureValid(CharacterSheet characterSheet);
        CharacterSheet AddStandardAttributesTo(CharacterSheet characterSheet);
        CharacterSheet AddCalculatedStatisticsTo(CharacterSheet characterSheet);
    }

    public class CharacterSheetService : ICharacterSheetService
    {
        CharacterSheetDbContext _db;
        private RulesDbContext _rulesDb = new RulesDbContext();
        private ICalculationService _calcService = new CalculationService();

        public CharacterSheetService(CharacterSheetDbContext db)
        {
            _db = db;
        }

        public CharacterSheet CreateCharacterSheet()
        {
            var characterSheet = new CharacterSheet();
            AddStandardAttributesTo(characterSheet);
            return characterSheet;
        }

        public CharacterSheet EnsureValid(CharacterSheet characterSheet)
        {
            characterSheet.Description = characterSheet.Description ?? new CharacterDescription
            {
                Name = "",
                Text = ""
            };

            // TODO: this shouldnt reference the rules directly
            var levelInfo = DnD5.LevelInfo.FindBestFit(characterSheet.Experience, characterSheet.Level);
            characterSheet.Experience = levelInfo.XP;
            characterSheet.Level = levelInfo.Level;

            AddStandardAttributesTo(characterSheet);

            return characterSheet;
        }

        public CharacterSheet AddStandardAttributesTo(CharacterSheet characterSheet)
        {
            var existingAbilityIds = new HashSet<int>(characterSheet.AbilityAllocations?.Select(a => a.AbilityId) ?? new int[] { });
            var existingSkillIds = new HashSet<int>(characterSheet.SkillAllocations?.Select(a => a.SkillId) ?? new int[] { });

            // need to jump through some hoops because LINQ-to-entities cannot construct our types directly
            var abilityPoints =
               (from ability in _db.Abilities
                where ability.IsStandard
                where !existingAbilityIds.Contains(ability.Id)
                orderby ability.SortOrder
                select new { Ability = ability, Points = 8 })
                .ToList();

            var abilityAllocations = abilityPoints
                .Select(a => new AbilityAllocation { Ability = a.Ability, Points = a.Points });

            if (null != characterSheet.AbilityAllocations)
            {
                abilityAllocations = abilityAllocations.Concat(characterSheet.AbilityAllocations);
            }

            var skillPoints =
               (from skill in _db.Skills
                where skill.IsStandard
                where !existingSkillIds.Contains(skill.Id)
                orderby skill.Name
                select new { Skill = skill, Points = 0 })
                .ToList();

            var skillAllocations = skillPoints
                .Select(s => new SkillAllocation { Skill = s.Skill, Points = s.Points });

            if (null != characterSheet.SkillAllocations)
            {
                skillAllocations = skillAllocations.Concat(characterSheet.SkillAllocations);
            }

            characterSheet.AbilityAllocations = abilityAllocations.ToList();
            characterSheet.SkillAllocations = skillAllocations.ToList();

            return characterSheet;
        }

        public CharacterSheet AddCalculatedStatisticsTo(CharacterSheet characterSheet)
        {
            var calculationContext = new RulesCalculationContext(_rulesDb, characterSheet);
            var calculatedResults = _calcService.Calculate(calculationContext);

            //
            // calculated abilities
            //

            var abilityAttribValuesByName = calculatedResults.AttributeValuesForCategory("ability")
                .ToDictionary(val => val.Attribute.Name.ToLower());

            var abilityModAttribValuesByName = calculatedResults.AttributeValuesForCategory("ability-modifier")
                .ToDictionary(val => val.Attribute.Name.ToLower());

            var abilityCalculations =
                (from abilityAlloc in characterSheet.AbilityAllocations
                 let attribName = abilityAlloc.Ability.ShortName.ToLower()
                 select new AbilityValueCalculation
                 {
                     Allocation = abilityAlloc,
                     Value = abilityAttribValuesByName[attribName].Value,
                     Modifier = abilityModAttribValuesByName[attribName].Value,
                 });

            //
            // calculated skills
            //

            var skillAttribValuesByName = calculatedResults.AttributeValuesForCategory("skill")
                .ToDictionary(val => val.Attribute.Name.ToLower());

            var skillCalculations =
                (from skillAlloc in characterSheet.SkillAllocations
                 let attribName = skillAlloc.Skill.Name.ToLower()
                 select new SkillValueCalculation
                 {
                     Allocation = skillAlloc,
                     Value = skillAttribValuesByName[attribName].Value
                 });

            var derivedStatistics = new CharacterDerivedStatistics
            {
                Abilities = abilityCalculations.ToList(),
                ProficiencyBonus = 0,
                Skills = skillCalculations.ToList(),
                SpellSlots = new List<int> { },
            };

            characterSheet.DerivedStatistics = derivedStatistics;
            return characterSheet;
        }
    }
}
