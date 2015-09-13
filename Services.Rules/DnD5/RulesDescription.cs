using System.Collections.Generic;
using System.Linq;

namespace Services.Rules.DnD5
{
    public class RulesDescription
    {
        private IDictionary<string, Class> _classes;
        private IDictionary<string, Ability> _abilities;
        private IDictionary<string, Skill> _skills;
        
        public ICollection<Class> Classes { get { return _classes.Values; } }
        public ICollection<Ability> Abilities { get { return _abilities.Values; } }
        public ICollection<Skill> Skills { get { return _skills.Values; } }

        public RulesDescription()
        {
            // http://engl393-dnd5th.wikia.com/wiki/Classes
            _classes = new[]
            {
                new Class { Name = "Barbarian", HitDie = 12, Description = "A fierce warrior of primitive background who can enter a battle rage." },
                new Class { Name = "Bard", HitDie = 8, Description = "An inspiring magician whose power echoes the music of creation." },
                new Class { Name = "Cleric", HitDie = 8, Description = "A priestly champion who wields divine magic in service of a higher power." },
                new Class { Name = "Druid", HitDie = 8, Description = "A priest of the Old Faith, wielding the powers of nature – moonlight and plant growth, fire and lightning – and adopting animal forms." },
                new Class { Name = "Fighter", HitDie = 10, Description = "A master of martial combat, skilled with a variety of weapons and armor." },
                new Class { Name = "Monk", HitDie = 8, Description = "A master of martial arts, harnessing the power of the body in pursuit of physical and spiritual perfection." },
                new Class { Name = "Paladin", HitDie = 10, Description = "A holy warrior bound to a sacred oath." },
                new Class { Name = "Ranger", HitDie = 10, Description = "A warrior who uses martial prowess and nature magic to combat threats on the edges of civilization." },
                new Class { Name = "Rogue", HitDie = 8, Description = "A scoundrel who uses stealth and trickery to overcome obstacles and enemies." },
                new Class { Name = "Sorcerer", HitDie = 6, Description = "A spellcaster who draws on inherent magic from a gift or bloodline." },
                new Class { Name = "Warlock", HitDie = 8, Description = "A wielder of magic that is derived from a bargain with an extraplanar entity." },
                new Class { Name = "Wizard", HitDie = 6, Description = "A scholarly magic-user capable of manipulating the structures of reality." },
            }.ToDictionary(v => v.Name.ToLower());
           
            _abilities = new[]
            {
                new Ability { SortOrder = 1, IsStandard = true, Name = "Strength", ShortName = "Str", Description = "Natural athleticism, bodily power" },
                new Ability { SortOrder = 2, IsStandard = true, Name = "Dexterity", ShortName = "Dex", Description = "Physical agility, reflexes, balance, poise" },
                new Ability { SortOrder = 3, IsStandard = true, Name = "Constitution", ShortName = "Con", Description = "Health, stamina, vital force" },
                new Ability { SortOrder = 4, IsStandard = true, Name = "Intelligence", ShortName = "Int", Description = "Mental acuity, information recall, analytical skill" },
                new Ability { SortOrder = 5, IsStandard = true, Name = "Wisdom", ShortName = "Wis", Description = "Awareness, intuition, insight" },
                new Ability { SortOrder = 6, IsStandard = true, Name = "Charisma", ShortName = "Cha", Description = "Confidence, eloquence, leadership" },
            }.ToDictionary(v => v.ShortName.ToLower());

            _skills = new[]
            {
                new Skill { IsStandard = true, Name = "Acrobatics", Description = "Balance, Escape, stunts", Ability = _abilities["dex"] },
                new Skill { IsStandard = true, Name = "Animal Handling", Description = "Calm a domestic animal, figure an animals intentions, control mount during a maneuver", Ability = _abilities["wis"] },
                new Skill { IsStandard = true, Name = "Arcana", Description = "Knowledge about magical creatures, spells, rituals, planes", Ability = _abilities["int"] },
                new Skill { IsStandard = true, Name = "Athletics", Description = "Running jumping, climbing swimming", Ability = _abilities["str"] },
                new Skill { IsStandard = true, Name = "Deception", Description = "Lying outright, disguising yourself", Ability = _abilities["cha"] },
                new Skill { IsStandard = true, Name = "History", Description = "Knowledge of the history of objects, places, people, and events, the significance of certain groups, etc", Ability = _abilities["int"] },
                new Skill { IsStandard = true, Name = "Insight", Description = "Determine the true intentions of a creature, such as when searching out a lie or predicting someone’s next move", Ability = _abilities["wis"] },
                new Skill { IsStandard = true, Name = "Intimidation", Description = "Getting your way by instilling fear", Ability = _abilities["cha"] },
                new Skill { IsStandard = true, Name = "Investigation", Description = "Close inspection of surroundings, draw conclusions from clues, find secret doors, find a weak point in something, research obscure info", Ability = _abilities["int"] },
                new Skill { IsStandard = true, Name = "Medicine", Description = "Stabilize a dying ally, diagnose illness, treat field wounds", Ability = _abilities["wis"] },
                new Skill { IsStandard = true, Name = "Nature", Description = "Knowledge of terrain, plants and animals, the weather, and natural cycles", Ability = _abilities["int"] },
                new Skill { IsStandard = true, Name = "Perception", Description = "General sense of the surroundings, sights, sounds and smells", Ability = _abilities["wis"] },
                new Skill { IsStandard = true, Name = "Performance", Description = "Perform for a crowd", Ability = _abilities["cha"] },
                new Skill { IsStandard = true, Name = "Persuasion", Description = "Non-violently influence a person's attitude", Ability = _abilities["cha"] },
                new Skill { IsStandard = true, Name = "Religion", Description = "knowledge of deities, rites and prayers, religious hierarchies, holy symbols, and the practices of secret cults", Ability = _abilities["int"] },
                new Skill { IsStandard = true, Name = "Sleight of Hand", Description = "Pickpocket, hiding something on you, planting something on someone", Ability = _abilities["dex"] },
                new Skill { IsStandard = true, Name = "Stealth", Description = "conceal yourself from enemies, slink past guards, slip away without being noticed, or sneak up on someone without being seen or heard", Ability = _abilities["dex"] },
                new Skill { IsStandard = true, Name = "Survival", Description = "follow tracks, hunt wild game, guide your group through wilds, identify signs of what live near, predict the weather, or avoid other natural hazards", Ability = _abilities["wis"] },
            }.ToDictionary(v => v.Name.ToLower());
        }
    }
}
