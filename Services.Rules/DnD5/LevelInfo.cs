using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Rules.DnD5
{
    public class LevelInfo
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

        public static LevelInfo FindForLevel(int? level)
        {
            if (!level.HasValue || level <= 0) return _levels.First();
            if (level >= _levels.Length) return _levels.Last();
            return _levels[level.Value - 1];
        }

        public static LevelInfo FindForXp(int? xp)
        {
            if (!xp.HasValue) return _levels.First();
            var result = _levels.LastOrDefault(i => i.XP <= xp);

            if (null == result)
                return _levels.Last();

            return result;
        }

        public static LevelInfo FindBestFit(int? xp, int? level)
        {
            var byXp = FindForXp(xp);
            var byLevel = FindForLevel(level);
            if (byLevel.Level > byXp.Level)
                return byLevel;

            return byXp.CloneWithXp(xp.HasValue ? xp.Value : 0);
        }
    }
}
