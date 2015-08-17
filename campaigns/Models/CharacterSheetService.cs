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

        public CharacterSheet CreateCharacterSheet()
        {
            // need to jump through some hoops because LINQ-to-entities cannot construct our types directly
            var abilityAllocations =
                _db.Abilities
                .Where(a => a.IsStandard)
                .OrderBy(a => a.SortOrder)
                .Select(a => new { Entity = a }).AsEnumerable()
                .Select(a => new AbilityAllocation { Ability = a.Entity, Points = 8 }).ToList();

            var skillAllocations =
                _db.Skills
                .Where(s => s.IsStandard)
                .OrderBy(s => s.Name)
                .Select(s => new { Entity = s }).AsEnumerable()
                .Select(s => new SkillAllocation { Skill = s.Entity, Points = 0 }).ToList();

            return new CharacterSheet { AbilityAllocations = abilityAllocations, SkillAllocations = skillAllocations };
        }
    }

    public interface ICharacterSheetService
    {
        CharacterSheet CreateCharacterSheet();
    }
}
