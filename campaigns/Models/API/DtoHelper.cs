using Campaigns.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models.DTO
{
#if false
    public static class DtoHelper
    {
        private static AbilityAllocation CloneAndUpdate(this CharacterSheetDbContext db, AbilityAllocation a, DTO.AbilityAllocationDTO apiData)
        {
            var ability = a?.Ability ?? db.Abilities.Find(apiData.AbilityId);
            return new AbilityAllocation { Ability = ability, Points = apiData.Points };
        }
        
        private static SkillAllocation CloneAndUpdate(this CharacterSheetDbContext db, SkillAllocation a, DTO.SkillAllocationDTO apiData)
        {
            var skill = a?.Skill ?? db.Skills.Find(apiData.SkillId);
            return new SkillAllocation { Skill = skill, Points = apiData.Points };
        }

        public static CharacterSheet CreateFromDTO(CharacterSheetDbContext db, DTO.CharacterSheetDTO dto)
        {
            return UpdateFromDTO(db, new CharacterSheet(), dto);
        }

        public static CharacterSheet UpdateFromDTO(CharacterSheetDbContext db, CharacterSheet characterSheet, DTO.CharacterSheetDTO dto)
        {
            characterSheet.Description = characterSheet.Description ?? new CharacterDescription();
            characterSheet.Description.Name = dto.Name;
            characterSheet.Description.Text = dto.Description;

            var experience = dto.Experience.GetValueOrDefault(0);
            var level = dto.Level.GetValueOrDefault(0);

            // TODO: this shouldnt reference the rules directly
            var levelInfo = LevelInfo.FindBestFit(experience, level);
            characterSheet.Experience = levelInfo.XP;
            characterSheet.Level = levelInfo.Level;

            // copy existing allocations, overwriting with DTO
            if (null != dto.AbilityAllocations)
            {
                var abilityAllocations =
                    (from allocationDto in dto.AbilityAllocations
                     let allocation = characterSheet.AbilityAllocations?.FirstOrDefault(a => a.Ability.Id == allocationDto.AbilityId)
                     select db.CloneAndUpdate(allocation, allocationDto)
                     ).ToList();

                characterSheet.AbilityAllocations = abilityAllocations;
            }

            if (null != dto.SkillAllocations)
            {
                var skillAllocations =
                    (from allocationDto in dto.SkillAllocations
                     let allocation = characterSheet.SkillAllocations?.FirstOrDefault(a => a.Skill.Id == allocationDto.SkillId)
                     select db.CloneAndUpdate(allocation, allocationDto)
                     ).ToList();

                characterSheet.SkillAllocations = skillAllocations;
            }

            if (dto.RaceId.HasValue)
            {
                characterSheet.Race = db.Races.Find(dto.RaceId);
            }

            if (dto.ClassId.HasValue)
            {
                characterSheet.Class = db.Classes.Find(dto.ClassId);
            }

            return characterSheet;
        }
    }
#endif
}
