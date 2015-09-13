using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Rules.DnD5
{
    public class RaceInfo
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public IDictionary<string, int> AbilityContributions { get; private set; }
        public int MaxSpeed { get; private set; }

        public static IEnumerable<RaceInfo> Races { get { return _races; } }
        private static RaceInfo[] _races = new RaceInfo[]
        {
            new RaceInfo
            {
                Name = "Dwarf",
                Description = "United by rich kingdoms of ancient grandeur, halls carved into roots of mountains with the echoing of picks and happers in deep mines and blazing forges, with a commitment to clan and tradition and a burning hatred of goblins and orcs.<p />Hardy, skilled warriors, miners, and workers of stone and metal. Standing well under 5 feet tall yet so broad as to weigh as much as a human. Known for their courage and endurance.",
                AbilityContributions = new Dictionary<string, int> { { "con", 2 }, },
                MaxSpeed = 25,
            },
            new RaceInfo
            {
                Name = "Hill Dwarf",
                Description = "United by rich kingdoms of ancient grandeur, halls carved into roots of mountains with the echoing of picks and happers in deep mines and blazing forges, with a commitment to clan and tradition and a burning hatred of goblins and orcs.<p />Hardy, skilled warriors, miners, and workers of stone and metal. Standing well under 5 feet tall yet so broad as to weigh as much as a human. Known for their courage and endurance.",
                AbilityContributions = new Dictionary<string, int> { { "con", 2 }, { "wis", 1 }, },
                MaxSpeed = 25,
            },
            new RaceInfo
            {
                Name = "Mountain Dwarf",
                Description = "United by rich kingdoms of ancient grandeur, halls carved into roots of mountains with the echoing of picks and happers in deep mines and blazing forges, with a commitment to clan and tradition and a burning hatred of goblins and orcs.<p />Hardy, skilled warriors, miners, and workers of stone and metal. Standing well under 5 feet tall yet so broad as to weigh as much as a human. Known for their courage and endurance.",
                AbilityContributions = new Dictionary<string, int> { { "str", 2 }, { "con", 2 }, },
                MaxSpeed = 25,
            },
            new RaceInfo
            {
                Name = "Elf",
                Description = "Magical people of otherworldly grace, living in the world but not entirely part of it. Elves live in places of ethereal beauty, in the midst of ancient forests or in silvery spires glittering with faerie light.<p />Elves love nature and magic, and with unearthly grace range from well under 5 feet to just over 6 feet, living for well over 700 years.",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, },
                MaxSpeed = 30,
            },
            new RaceInfo
            {
                Name = "High Elf",
                Description = "Magical people of otherworldly grace, living in the world but not entirely part of it. Elves live in places of ethereal beauty, in the midst of ancient forests or in silvery spires glittering with faerie light.<p />Elves love nature and magic, and with unearthly grace range from well under 5 feet to just over 6 feet, living for well over 700 years.",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "int", 1 }, },
                MaxSpeed = 30,
            },
            new RaceInfo
            {
                Name = "Wood Elf",
                Description = "Magical people of otherworldly grace, living in the world but not entirely part of it. Elves live in places of ethereal beauty, in the midst of ancient forests or in silvery spires glittering with faerie light.<p />Elves love nature and magic, and with unearthly grace range from well under 5 feet to just over 6 feet, living for well over 700 years.",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "wis", 1 }, },
                MaxSpeed = 30,
            },
            new RaceInfo
            {
                Name = "Drow Elf",
                Description = "Magical people of otherworldly grace, living in the world but not entirely part of it. Elves live in places of ethereal beauty, in the midst of ancient forests or in silvery spires glittering with faerie light.<p />Elves love nature and magic, and with unearthly grace range from well under 5 feet to just over 6 feet, living for well over 700 years.",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "cha", 1 }, },
                MaxSpeed = 30,
            },
            new RaceInfo
            {
                Name = "Halfling",
                Description = "With a love of comforts of home, peace and quiet, with blazing fires and generous meals, halflings survive in a world filled with larger creatures by avoiding notice. At about 3 feet tall, they have managed to survive for centuries in the shadow of empires and on the edges of wars and political strife.",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, },
                MaxSpeed = 25,
            },
            new RaceInfo
            {
                Name = "Lightfoot Halfling",
                Description = "With a love of comforts of home, peace and quiet, with blazing fires and generous meals, halflings survive in a world filled with larger creatures by avoiding notice. At about 3 feet tall, they have managed to survive for centuries in the shadow of empires and on the edges of wars and political strife.",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "cha", 1 }, },
                MaxSpeed = 25,
            },
            new RaceInfo
            {
                Name = "Stout Halfling",
                Description = "With a love of comforts of home, peace and quiet, with blazing fires and generous meals, halflings survive in a world filled with larger creatures by avoiding notice. At about 3 feet tall, they have managed to survive for centuries in the shadow of empires and on the edges of wars and political strife.",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "con", 1 }, },
                MaxSpeed = 25,
            },
            new RaceInfo
            {
                Name = "Human",
                Description = "The youngest of the common races, although short-lived they strive to achieve as much as they can in the years they are given.<p />With their penchant for migration and conquest, humans are more physically diverse than other common races.",
                AbilityContributions = new Dictionary<string, int> { { "str", 1 }, { "dex", 1 }, { "con", 1 }, { "int", 1 }, { "wis", 1 }, { "cha", 1 }, },
                MaxSpeed = 30,
            },
            new RaceInfo
            {
                Name = "Dragonborn",
                Description = "Born of dragons they walk proudly through a world that greets them with fearful incomprehension. Shaped by draconic gods or the dragons themselves, dragonborn originally hatched from dragon eggs as a unique race, combining the best of dragons and humanoids and can be allied with either.<p />Their small, fine scales are usually brass or bronze in colour, and they are tall and strongly built, standing around 6 1/2 feet tall, with strong, talonlike claws with three fingers and a thumb on each hand.",
                AbilityContributions = new Dictionary<string, int> { { "str", 2 }, { "cha", 1 }, },
                MaxSpeed = 30,
            },
            new RaceInfo
            {
                Name = "Gnome",
                Description = "Forming close-knit communities in warren-like neighbourhoods, they share a love of loud sounds such as explosions and grinding gears. Gnomes take delight in life, enjoying every moment of invention, exploration, investigation, creation and play.<p />Gnomes average a little over 3 feet tall, their tan-brown faces usually adorned with broad smiles beneath their prodigious noses.",
                AbilityContributions = new Dictionary<string, int> { { "int", 2 }, },
                MaxSpeed = 25,
            },
            new RaceInfo
            {
                Name = "Forest Gnome",
                Description = "Forming close-knit communities in warren-like neighbourhoods, they share a love of loud sounds such as explosions and grinding gears. Gnomes take delight in life, enjoying every moment of invention, exploration, investigation, creation and play.<p />Gnomes average a little over 3 feet tall, their tan-brown faces usually adorned with broad smiles beneath their prodigious noses.",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "int", 1 }, },
                MaxSpeed = 25,
            },
            new RaceInfo
            {
                Name = "Rock Gnome",
                Description = "Forming close-knit communities in warren-like neighbourhoods, they share a love of loud sounds such as explosions and grinding gears. Gnomes take delight in life, enjoying every moment of invention, exploration, investigation, creation and play.<p />Gnomes average a little over 3 feet tall, their tan-brown faces usually adorned with broad smiles beneath their prodigious noses.",
                AbilityContributions = new Dictionary<string, int> { { "con", 1 }, { "int", 2 }, },
                MaxSpeed = 25,
            },
            new RaceInfo
            {
                Name = "Half-Elf",
                Description = "Of two worlds yet truly belonging to neither, half-elves combine the best qualities of their elf and human parents: human curiosity, inventiveness and ambition tempered by the refined sences, love of nature and artistic tastes of the elves.",
                AbilityContributions = new Dictionary<string, int> { { "*", 2 }, },
                MaxSpeed = 30,
            },
            new RaceInfo
            {
                Name = "Half-Orc",
                Description = "With grayish pigmentation, sloping foreheads, jutting jaws with prominient teeth and towering builds (between 6-7 feet tall), the half-orc's orcish heritage is plain for all to see. They can be found united under the leadership of mighty warlocks or joined with larger hordes of humans or orc tribes.<p />Any half-orc that has lived near orcs bears scars, whether marks of humiliation or pride.",
                AbilityContributions = new Dictionary<string, int> { { "str", 2 }, { "con", 1 }, },
                MaxSpeed = 30,
            },
            new RaceInfo
            {
                Name = "Tiefling",
                Description = "Tieflings are met with stares and whispers and suffer violence, insult, mistrust and fear at the hands of other civilised folk. Formed of an ancient pact that infused them with the essence of Asmodeus - the overlord of the Nine Hells - their appearance and nature are the result of some ancient sin for which they will always be held accountable.<p />Tieflings have large horns that may be curling or straight, and a large tail which may lash around their legs while they are nervous. Their canine teeth are sharply pointed and their eyes are solid colours - black, red, white, silver or gold.",
                AbilityContributions = new Dictionary<string, int> { { "int", 1 }, { "cha", 2 }, },
                MaxSpeed = 30,
            },
        };
    }
}
