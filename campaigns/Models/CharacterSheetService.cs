using Campaigns.Core.Data;
using Campaigns.Models.DAL;
using Services.Calculation;
using Services.Rules;
using Services.Rules.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models
{
    public interface ICharacterSheetService
    {
        DAL.CharacterSheet CreateCharacterSheet();
        DAL.CharacterSheet EnsureValid(DAL.CharacterSheet characterSheet);
        DAL.CharacterSheet AddStandardAttributesTo(DAL.CharacterSheet characterSheet);
        DAL.CharacterSheet AddCalculatedStatisticsTo(DAL.CharacterSheet characterSheet);
    }

    public class CharacterSheetService : ICharacterSheetService
    {
        CharacterSheetDbContext _db;
        private RulesDbContext _rulesDb = new RulesDbContext();

        private EFEntityRepository<Services.Calculation.Attribute> _attributesDb;
        private EFEntityRepository<Services.Calculation.AttributeContribution> _contributionsDb;

        private RulesService _rules;

        //AttributeValue ToAttribValue(AbilityValueCalculation calc)
        //{
        //    return new AttributeValue
        //    {
        //        Attribute = _rulesDb.GetAttribute(calc.Allocation.Ability.ShortName, "abilities"),
        //        Contributions = new List<AttributeContribution>(),
        //        Value = calc.Value,
        //    };
        //}

        //AttributeValue ToAttribValue(SkillValueCalculation calc)
        //{
        //    return new AttributeValue
        //    {
        //        Attribute = _rulesDb.GetAttribute(calc.Allocation.Skill.Name, "skills"),
        //        Contributions = new List<AttributeContribution>(),
        //        Value = calc.Value,
        //    };
        //}

        //public DAL.Experimental.CharacterSheet ToExperimental(DAL.CharacterSheet characterSheet)
        //{
        //    var derivedAbilityAttribs = characterSheet.DerivedStatistics.Abilities
        //        .Select(ToAttribValue);
        //    var derivedSkillAttribs = characterSheet.DerivedStatistics.Skills
        //        .Select(ToAttribValue);

        //    return new Models.DAL.Experimental.CharacterSheet
        //    {
        //        Name = characterSheet.Description.Name,
        //        Description = characterSheet.Description.Text,
        //        AttributeValues = derivedAbilityAttribs.Concat(derivedSkillAttribs).ToList(),
        //    };
        //}

        public CharacterSheetService(CharacterSheetDbContext db)
        {
            _db = db;

            _attributesDb = new EFEntityRepository<Services.Calculation.Attribute>(_rulesDb, _rulesDb.Attributes);
            _contributionsDb = new EFEntityRepository<Services.Calculation.AttributeContribution>(_rulesDb, _rulesDb.AttributeContributions);

            _rules = new RulesService(_attributesDb, _contributionsDb);
    }

        public DAL.CharacterSheet CreateCharacterSheet()
        {
            var characterSheet = new DAL.CharacterSheet();
            AddStandardAttributesTo(characterSheet);
            return characterSheet;
        }

        public DAL.CharacterSheet EnsureValid(DAL.CharacterSheet characterSheet)
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

        public DAL.CharacterSheet AddStandardAttributesTo(DAL.CharacterSheet characterSheet)
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

        private Services.Calculation.Attribute GetAttribute(Ability ability)
        {
            return _attributesDb.EntityTable.First(a =>
                0 == string.Compare(a.Name, ability.ShortName, true) &&
                0 == string.Compare(a.Category, "abilities", true));
        }

        public DAL.CharacterSheet AddCalculatedStatisticsTo(DAL.CharacterSheet characterSheet)
        {
            var characterContributions =
                from alloc in characterSheet.AbilityAllocations
                let attribute = GetAttribute(alloc.Ability)
                select attribute.ConstantContributionFrom(null, alloc.Points);

            var characterSheetImpl = _rules.CreateCharacterSheet(new CharacterSpecification
            {
                Allocations = characterContributions,
            });

            //
            // calculated abilities
            //
            
            var abilityAttribValuesByName = characterSheetImpl.AttributeValues
                .Where(val => 0 == string.Compare(val.Attribute.Category, "abilities"))
                .ToDictionary(val => val.Attribute.Name.ToLower());
            
            var abilityModAttribValuesByName = characterSheetImpl.AttributeValues
                .Where(val => 0 == string.Compare(val.Attribute.Category, "ability-modifiers"))
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
            
            var skillAttribValuesByName = characterSheetImpl.AttributeValues
                .Where(val => 0 == string.Compare(val.Attribute.Category, "skills"))
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
