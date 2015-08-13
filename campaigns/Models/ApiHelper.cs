using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace campaigns.Models
{
    public static class ApiHelper
    {
        private static AbilityAllocation FindOrCreateAbilityAllocation(CharacterSheet cs, int abilityId, CharacterSheetDbContext db)
        {
            var allocation = cs.AbilityAllocations?.FirstOrDefault(a => a.Ability.Id == abilityId);
            if (null == allocation)
            {
                allocation = new AbilityAllocation { Ability = db.Abilities.Find(abilityId) };
            }
            return allocation;
        }

        private static AbilityAllocation CloneAndUpdate(this CharacterSheetDbContext db, AbilityAllocation a, Api.AbilityAllocationDTO apiData)
        {
            var ability = a?.Ability ?? db.Abilities.Find(apiData.AbilityId);
            return new AbilityAllocation { Ability = ability, Points = apiData.Points };
        }
        
        private static SkillAllocation CloneAndUpdate(this CharacterSheetDbContext db, SkillAllocation a, Api.SkillAllocationDTO apiData)
        {
            var skill = a?.Skill ?? db.Skills.Find(apiData.SkillId);
            return new SkillAllocation { Skill = skill, Points = apiData.Points };
        }

        public static CharacterSheet CreateFromApiData(CharacterSheetDbContext db, Api.CharacterSheetDTO apiData)
        {
            return UpdateFromApiData(db, new CharacterSheet(), apiData);
        }

        public static CharacterSheet UpdateFromApiData(CharacterSheetDbContext db, CharacterSheet characterSheet, Api.CharacterSheetDTO apiData)
        {
            characterSheet.Name = apiData.Name;
            characterSheet.Description = apiData.Description;

            var experience = apiData.Experience.GetValueOrDefault(0);
            var level = apiData.Level.GetValueOrDefault(0);

            // TODO: this shouldnt reference the rules directly
            var levelInfo = LevelInfo.FindBestFit(experience, level);
            characterSheet.Experience = levelInfo.XP;

            // copy existing allocations, overwriting with DTO
            if (null != apiData.AbilityAllocations)
            {
                var abilityAllocations =
                    (from allocationDto in apiData.AbilityAllocations
                     let allocation = characterSheet.AbilityAllocations?.FirstOrDefault(a => a.Ability.Id == allocationDto.AbilityId)
                     select db.CloneAndUpdate(allocation, allocationDto)
                     ).ToList();

                characterSheet.AbilityAllocations = abilityAllocations;
            }

            if (null != apiData.SkillAllocations)
            {
                var skillAllocations =
                    (from allocationDto in apiData.SkillAllocations
                     let allocation = characterSheet.SkillAllocations?.FirstOrDefault(a => a.Skill.Id == allocationDto.SkillId)
                     select db.CloneAndUpdate(allocation, allocationDto)
                     ).ToList();

                characterSheet.SkillAllocations = skillAllocations;
            }

            if (apiData.RaceId.HasValue)
            {
                characterSheet.Race = db.Races.Find(apiData.RaceId);
            }

            if (apiData.ClassId.HasValue)
            {
                characterSheet.Class = db.Classes.Find(apiData.ClassId);
            }

            return characterSheet;
        }
    }
}
