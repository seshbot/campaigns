using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace campaigns.Models
{
    class RaceInfo
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

    class LevelInfo
    {
        public int Level { get; private set; }
        public int XP { get; private set; }
        public int ProficiencyBonus { get; private set; }

        private static LevelInfo[] _levels = new LevelInfo[]
        {
            new LevelInfo { Level = 1, XP = 0, ProficiencyBonus = 2 },
            new LevelInfo { Level = 2, XP = 300, ProficiencyBonus = 2 },
            new LevelInfo { Level = 3, XP = 900, ProficiencyBonus = 2 },
            new LevelInfo { Level = 4, XP = 2700, ProficiencyBonus = 2 },
            new LevelInfo { Level = 5, XP = 6500, ProficiencyBonus = 3 },
            new LevelInfo { Level = 6, XP = 14000, ProficiencyBonus = 3 },
            new LevelInfo { Level = 7, XP = 23000, ProficiencyBonus = 3 },
            new LevelInfo { Level = 8, XP = 34000, ProficiencyBonus = 3 },
            new LevelInfo { Level = 9, XP = 48000, ProficiencyBonus = 4 },
            new LevelInfo { Level = 10, XP = 64000, ProficiencyBonus = 4 },
            new LevelInfo { Level = 11, XP = 85000, ProficiencyBonus = 4 },
            new LevelInfo { Level = 12, XP = 100000, ProficiencyBonus = 4 },
            new LevelInfo { Level = 13, XP = 120000, ProficiencyBonus = 5 },
            new LevelInfo { Level = 14, XP = 140000, ProficiencyBonus = 5 },
            new LevelInfo { Level = 15, XP = 165000, ProficiencyBonus = 5 },
            new LevelInfo { Level = 16, XP = 195000, ProficiencyBonus = 5 },
            new LevelInfo { Level = 17, XP = 225000, ProficiencyBonus = 6 },
            new LevelInfo { Level = 18, XP = 265000, ProficiencyBonus = 6 },
            new LevelInfo { Level = 19, XP = 305000, ProficiencyBonus = 6 },
            new LevelInfo { Level = 20, XP = 355000, ProficiencyBonus = 6 },
        };

        private LevelInfo CloneWithXp(int xp)
        {
            return new LevelInfo { Level = Level, ProficiencyBonus = ProficiencyBonus, XP = xp };
        }

        public static LevelInfo FindForLevel(int level)
        {
            if (level <= 0) return _levels.First();
            if (level >= _levels.Length) return _levels.Last();
            return _levels[level - 1];
        }

        public static LevelInfo FindForXp(int xp)
        {
            var result = _levels.LastOrDefault(i => i.XP <= xp);

            if (null == result)
                return _levels.Last();

            return result;
        }

        public static LevelInfo FindBestFit(int xp, int level)
        {
            var byXp = FindForXp(xp);
            var byLevel = FindForLevel(level);
            if (byLevel.Level > byXp.Level)
                return byLevel;

            return byXp.CloneWithXp(xp);
        }
    }

    public static class CharacterSheetCalculator
    {
        public static CharacterSheet AddDerivedStatisticsTo(CharacterSheet characterSheet)
        {
            var derivedStatistics = CalculateDerived(characterSheet);
            characterSheet.DerivedStatistics = derivedStatistics;
            return characterSheet;
        }

        public static CharacterDerivedStatistics CalculateDerived(CharacterSheet characterSheet)
        {
            //public IList<AbilityValueCalculation> Abilities { get; set; }
            //public IList<SkillValueCalculation> Skills { get; set; }
            //public int ProficiencyBonus { get; set; }
            //public IList<int> SpellSlots { get; set; }

            var levelInfo = LevelInfo.FindForXp(characterSheet.Experience);

            var abilityCalculations = null == characterSheet.AbilityAllocations
                ? new List<AbilityValueCalculation> { }
                : (
                    from a in characterSheet.AbilityAllocations
                    select CalculateAbilityValue(characterSheet, a)
                  ).ToList();

            var abilityCalculationTable = abilityCalculations.ToDictionary(a => a.Allocation.Ability.Id);

            var skillCalculations = null == characterSheet.SkillAllocations
                ? new List<SkillValueCalculation> { }
                : (
                   from s in characterSheet.SkillAllocations
                   select CalculateSkillValue(characterSheet, s, abilityCalculationTable)
                  ).ToList();

            return new CharacterDerivedStatistics
            {
                Abilities = abilityCalculations,
                ProficiencyBonus = levelInfo.ProficiencyBonus,
                Skills = skillCalculations,
                SpellSlots = new List<int> { },
                Level = levelInfo.Level,
            };
        }

        private static AbilityValueCalculation CalculateAbilityValue(CharacterSheet characterSheet, AbilityAllocation allocation)
        {
            var contributions = new List<Contribution>
            {
                new Contribution
                {
                    Type = ContributionType.Accumulate,
                    Source = ContributionSource.DirectlyAllocated,
                    SourceId = -1,
                    Value = allocation.Points,
                }
            };

            var raceInfo = RaceInfo.FindForCharacter(characterSheet);
            if (null != raceInfo)
            {
                contributions.AddRange(
                    from abilityContribution in raceInfo.AbilityContributions
                    where abilityContribution.Key == allocation.Ability.ShortName.ToLower()
                    select new Contribution
                    {
                        Type = ContributionType.Accumulate,
                        Source = ContributionSource.Race,
                        SourceId = characterSheet?.Race?.Id ?? -1,
                        Value = abilityContribution.Value,
                    });
            }

            var calculatedValue = contributions.Sum(c => c.Value);
            var modifier = (calculatedValue - 10) / 2;

            return new AbilityValueCalculation
            {
                Allocation = allocation,
                Contributions = contributions,
                Value = calculatedValue,
                Modifier = modifier,
            };
        }

        private static SkillValueCalculation CalculateSkillValue(CharacterSheet characterSheet, SkillAllocation allocation, IDictionary<int, AbilityValueCalculation> calculatedAbilities)
        {
            AbilityValueCalculation contributingAbilityCalculation;
            calculatedAbilities.TryGetValue(allocation.Skill.Ability.Id, out contributingAbilityCalculation);

            var contributions = new List<Contribution> {
                new Contribution
                {
                    Type = ContributionType.Accumulate,
                    Source = ContributionSource.DirectlyAllocated,
                    SourceId = -1,
                    Value = allocation.Points,
                },
            };

            if (null != contributingAbilityCalculation)
            {
                contributions.Add(new Contribution
                {
                    Type = ContributionType.Accumulate,
                    Source = ContributionSource.Ability,
                    SourceId = contributingAbilityCalculation.Allocation.Ability.Id,
                    Value = contributingAbilityCalculation.Modifier,
                });
            }

            var calculatedValue = contributions.Sum(c => c.Value);

            return new SkillValueCalculation
            {
                Allocation = allocation, 
                Contributions = contributions,
                Value = calculatedValue,
            };
        }
    }
}
