﻿using System.Collections.Generic;
using System.Linq;

namespace Services.Rules.DnD5
{
    public class RacialBonus
    {
        public Race Race { get; set; }
        public IDictionary<Ability, int> Bonuses { get; set; }
    }

    public class RulesDescription
    {
        private IDictionary<string, Race> _races;
        private IDictionary<string, Class> _classes;
        private IDictionary<string, Ability> _abilities;
        private IDictionary<string, Skill> _skills;

        private IDictionary<Race, RacialBonus> _racialBonuses;

        public ICollection<Race> Races { get { return _races.Values; } }
        public ICollection<Class> Classes { get { return _classes.Values; } }
        public ICollection<Ability> Abilities { get { return _abilities.Values; } }
        public ICollection<Skill> Skills { get { return _skills.Values; } }

        public ICollection<RacialBonus> RacialBonuses { get { return _racialBonuses.Values; } }

        public RulesDescription()
        {
            _races = new[]
            {
                new Race { Name = "Dwarf", Description = "United by rich kingdoms of ancient grandeur, halls carved into roots of mountains with the echoing of picks and happers in deep mines and blazing forges, with a commitment to clan and tradition and a burning hatred of goblins and orcs.<p />Hardy, skilled warriors, miners, and workers of stone and metal. Standing well under 5 feet tall yet so broad as to weigh as much as a human. Known for their courage and endurance." },
                new Race { Name = "Elf", Description = "Magical people of otherworldly grace, living in the world but not entirely part of it. Elves live in places of ethereal beauty, in the midst of ancient forests or in silvery spires glittering with faerie light.<p />Elves love nature and magic, and with unearthly grace range from well under 5 feet to just over 6 feet, living for well over 700 years." },
                new Race { Name = "Halfling", Description = "With a love of comforts of home, peace and quiet, with blazing fires and generous meals, halflings survive in a world filled with larger creatures by avoiding notice. At about 3 feet tall, they have managed to survive for centuries in the shadow of empires and on the edges of wars and political strife." },
                new Race { Name = "Human", Description = "The youngest of the common races, although short-lived they strive to achieve as much as they can in the years they are given.<p />With their penchant for migration and conquest, humans are more physically diverse than other common races." },
                new Race { Name = "Dragonborn", Description = "Born of dragons they walk proudly through a world that greets them with fearful incomprehension. Shaped by draconic gods or the dragons themselves, dragonborn originally hatched from dragon eggs as a unique race, combining the best of dragons and humanoids and can be allied with either.<p />Their small, fine scales are usually brass or bronze in colour, and they are tall and strongly built, standing around 6 1/2 feet tall, with strong, talonlike claws with three fingers and a thumb on each hand." },
                new Race { Name = "Gnome", Description = "Forming close-knit communities in warren-like neighbourhoods, they share a love of loud sounds such as explosions and grinding gears. Gnomes take delight in life, enjoying every moment of invention, exploration, investigation, creation and play.<p />Gnomes average a little over 3 feet tall, their tan-brown faces usually adorned with broad smiles beneath their prodigious noses." },
                new Race { Name = "Half-Elf", Description = "Of two worlds yet truly belonging to neither, half-elves combine the best qualities of their elf and human parents: human curiosity, inventiveness and ambition tempered by the refined sences, love of nature and artistic tastes of the elves." },
                new Race { Name = "Half-Orc", Description = "With grayish pigmentation, sloping foreheads, jutting jaws with prominient teeth and towering builds (between 6-7 feet tall), the half-orc's orcish heritage is plain for all to see. They can be found united under the leadership of mighty warlocks or joined with larger hordes of humans or orc tribes.<p />Any half-orc that has lived near orcs bears scars, whether marks of humiliation or pride." },
                new Race { Name = "Tiefling", Description = "Tieflings are met with stares and whispers and suffer violence, insult, mistrust and fear at the hands of other civilised folk. Formed of an ancient pact that infused them with the essence of Asmodeus - the overlord of the Nine Hells - their appearance and nature are the result of some ancient sin for which they will always be held accountable.<p />Tieflings have large horns that may be curling or straight, and a large tail which may lash around their legs while they are nervous. Their canine teeth are sharply pointed and their eyes are solid colours - black, red, white, silver or gold." },
            }.ToDictionary(v => v.Name.ToLower());

            // http://engl393-dnd5th.wikia.com/wiki/Classes
            _classes = new[]
            {
                new Class { Name = "Barbarian", Description = "A fierce warrior of primitive background who can enter a battle rage." },
                new Class { Name = "Bard", Description = "An inspiring magician whose power echoes the music of creation." },
                new Class { Name = "Cleric", Description = "A priestly champion who wields divine magic in service of a higher power." },
                new Class { Name = "Druid", Description = "A priest of the Old Faith, wielding the powers of nature – moonlight and plant growth, fire and lightning – and adopting animal forms." },
                new Class { Name = "Fighter", Description = "A master of martial combat, skilled with a variety of weapons and armor." },
                new Class { Name = "Monk", Description = "A master of martial arts, harnessing the power of the body in pursuit of physical and spiritual perfection." },
                new Class { Name = "Paladin", Description = "A holy warrior bound to a sacred oath." },
                new Class { Name = "Ranger", Description = "A warrior who uses martial prowess and nature magic to combat threats on the edges of civilization." },
                new Class { Name = "Rogue", Description = "A scoundrel who uses stealth and trickery to overcome obstacles and enemies." },
                new Class { Name = "Sorcerer", Description = "A spellcaster who draws on inherent magic from a gift or bloodline." },
                new Class { Name = "Warlock", Description = "A wielder of magic that is derived from a bargain with an extraplanar entity." },
                new Class { Name = "Wizard", Description = "A scholarly magic-user capable of manipulating the structures of reality." },
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

            _racialBonuses = new[]
            {
                // new RacialBonus { Race = _races["hill dwarf"], Bonuses = new Dictionary<Ability, int> { { _abilities["con"], 2 }, { _abilities["wis"], 1 }, } },
                // new RacialBonus { Race = _races["mountain dwarf"], Bonuses = new Dictionary<Ability, int> { { _abilities["str"], 2 }, { _abilities["con"], 2 }, } },
                // new RacialBonus { Race = _races["high elf"], Bonuses = new Dictionary<Ability, int> { { _abilities["dex"], 2 }, { _abilities["int"], 1 }, } },
                // new RacialBonus { Race = _races["wood elf"], Bonuses = new Dictionary<Ability, int> { { _abilities["dex"], 2 }, { _abilities["wis"], 1 }, } },
                // new RacialBonus { Race = _races["drow elf"], Bonuses = new Dictionary<Ability, int> { { _abilities["dex"], 2 }, { _abilities["cha"], 1 }, } },
                // new RacialBonus { Race = _races["lightfoot halfling"], Bonuses = new Dictionary<Ability, int> { { _abilities["dex"], 2 }, { _abilities["cha"], 1 }, } },
                // new RacialBonus { Race = _races["stout halfling"], Bonuses = new Dictionary<Ability, int> { { _abilities["dex"], 2 }, { _abilities["con"], 1 }, } },
                // new RacialBonus { Race = _races["forest gnome"], Bonuses = new Dictionary<Ability, int> { { _abilities["dex"], 2 }, { _abilities["int"], 1 }, } },
                // new RacialBonus { Race = _races["rock gnome"], Bonuses = new Dictionary<Ability, int> { { _abilities["con"], 1 }, { _abilities["int"], 2 }, } },
                // TODO: special rules for the following
                // new RacialBonus { Race = _races["human"], Bonuses = new Dictionary<Ability, int> { { _abilities["str"], 1 }, { _abilities["dex"], 1 }, { _abilities["con"], 1 }, } },
                // new RacialBonus { Race = _races["half-elf"], Bonuses = new Dictionary<Ability, int> { { _abilities["dex"], 2 }, { _abilities["cha"], 1 }, } },
                new RacialBonus { Race = _races["dwarf"], Bonuses = new Dictionary<Ability, int> { { _abilities["con"], 2 }, { _abilities["wis"], 1 }, } },
                new RacialBonus { Race = _races["elf"], Bonuses = new Dictionary<Ability, int> { { _abilities["dex"], 2 }, { _abilities["int"], 1 }, } },
                new RacialBonus { Race = _races["halfling"], Bonuses = new Dictionary<Ability, int> { { _abilities["dex"], 2 }, { _abilities["cha"], 1 }, } },
                new RacialBonus { Race = _races["dragonborn"], Bonuses = new Dictionary<Ability, int> { { _abilities["str"], 2 }, { _abilities["cha"], 1 }, } },
                new RacialBonus { Race = _races["gnome"], Bonuses = new Dictionary<Ability, int> { { _abilities["dex"], 2 }, { _abilities["int"], 1 }, } },
                new RacialBonus { Race = _races["half-orc"], Bonuses = new Dictionary<Ability, int> { { _abilities["str"], 2 }, { _abilities["con"], 1 }, } },
                new RacialBonus { Race = _races["tiefling"], Bonuses = new Dictionary<Ability, int> { { _abilities["int"], 1 }, { _abilities["cha"], 2 }, } },
            }.ToDictionary(v => v.Race);
        }
    }
}
