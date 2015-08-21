using campaigns.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace campaigns.Models
{
    public class CharacterSheetService : ICharacterSheetService
    {
        CharacterSheetDbContext _db;

        public CharacterSheetService(CharacterSheetDbContext db)
        {
            _db = db;
        }

        public void AddStandardAttributesTo(CharacterSheet characterSheet)
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
        }

        public CharacterSheet CreateCharacterSheet()
        {
            var characterSheet = new CharacterSheet();
            AddStandardAttributesTo(characterSheet);
            return characterSheet;
        }
    }

    public interface ICharacterSheetService
    {
        void AddStandardAttributesTo(CharacterSheet characterSheet);
        CharacterSheet CreateCharacterSheet();
    }
}
