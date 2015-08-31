using Campaigns.Model;

namespace Services.Rules
{
    public interface IRulesService
    {
        CharacterSheet CreateCharacterSheet();
        CharacterSheet CreateCharacterSheet(CharacterSpecification specification);
        CharacterSheet UpdateCharacterSheet(CharacterSheet orig, CharacterUpdate update);
        //CharacterUpdate ValidateCharacterSheet(CharacterSheet orig);
    }
}