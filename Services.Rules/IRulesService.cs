using Campaigns.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Rules
{
    public interface IRulesService
    {
        void DeleteCharacterById(int id);
        Character CreateCharacter(string name, string description);
        Character CreateCharacter(string name, string description, IEnumerable<AttributeAllocation> allocations);
        Character GetCharacter(int id);
        Task<Character> GetCharacterAsync(int id);
        IQueryable<Character> GetCharacters();
        Character UpdateCharacter(Character character, CharacterUpdate update);

        IQueryable<Campaigns.Model.Attribute> GetAllAttributes();
        IQueryable<Campaigns.Model.Attribute> GetAttributesByCategory(string category);
        Campaigns.Model.Attribute GetAttributeById(int id);
    }
}