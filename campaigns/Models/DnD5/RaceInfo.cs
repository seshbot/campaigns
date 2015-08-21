using campaigns.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace campaigns.Models.DnD5
{
    public class RaceInfo
    {
        public string Name { get; private set; }
        public IDictionary<string, int> AbilityContributions { get; private set; }

        private static RaceInfo[] _races = new RaceInfo[]
        {
            new RaceInfo
            {
                Name = "hill dwarf",
                AbilityContributions = new Dictionary<string, int> { { "con", 2 }, { "wis", 1 }, },
            },
            new RaceInfo
            {
                Name = "mountain dwarf",
                AbilityContributions = new Dictionary<string, int> { { "str", 2 }, { "con", 2 }, },
            },
            new RaceInfo
            {
                Name = "high elf",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "int", 1 }, },
            },
            new RaceInfo
            {
                Name = "wood elf",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "wis", 1 }, },
            },
            new RaceInfo
            {
                Name = "drow elf",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "cha", 1 }, },
            },
            new RaceInfo
            {
                Name = "lightfoot halfling",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "cha", 1 }, },
            },
            new RaceInfo
            {
                Name = "stout halfling",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "con", 1 }, },
            },
            new RaceInfo
            {
                Name = "human",
                AbilityContributions = new Dictionary<string, int> { { "str", 1 }, { "dex", 1 }, { "con", 1 }, { "int", 1 }, { "wis", 1 }, { "cha", 1 }, },
            },
            new RaceInfo
            {
                Name = "dragonborn",
                AbilityContributions = new Dictionary<string, int> { { "str", 2 }, { "cha", 1 }, },
            },
            new RaceInfo
            {
                Name = "forest gnome",
                AbilityContributions = new Dictionary<string, int> { { "dex", 2 }, { "int", 1 }, },
            },
            new RaceInfo
            {
                Name = "rock gnome",
                AbilityContributions = new Dictionary<string, int> { { "con", 1 }, { "int", 2 }, },
            },
            new RaceInfo
            {
                Name = "half-elf",
                AbilityContributions = new Dictionary<string, int> { { "*", 2 }, },
            },
            new RaceInfo
            {
                Name = "half-orc",
                AbilityContributions = new Dictionary<string, int> { { "str", 2 }, { "con", 1 }, },
            },
            new RaceInfo
            {
                Name = "tiefling",
                AbilityContributions = new Dictionary<string, int> { { "int", 1 }, { "cha", 2 }, },
            },
        };

        public static RaceInfo FindForCharacter(CharacterSheet characterSheet)
        {
            if (null == characterSheet?.Race?.Name)
                return null;
            return _races.FirstOrDefault(r => r.Name.Contains(characterSheet.Race.Name.ToLower()));
        }
    }
}
