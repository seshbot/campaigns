using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Campaigns.Models.Sessions
{
    public class DiceGroup
    {
        public int DiceSides { get; set; }
        public int DiceCount { get; set; }
        public string DiceColour { get; set; }

        public override string ToString()
        {
            if (DiceCount > 1)
                return string.Format("{0}d{1}", DiceCount, DiceSides);

            return string.Format("d{0}", DiceSides);
        }
    }

    public class RollSpec
    {
        public string Formula { get; set; }
        public IEnumerable<DiceGroup> DiceGroups { get; set; }
    }

    public class DiceGroupRoll
    {
        public DiceGroup DiceGroup { get; set; }
        public IEnumerable<int> Results { get; set; }
        public int Total { get; set; }
    }

    public class Roll
    {
        public string Formula { get; set; }
        public IEnumerable<DiceGroupRoll> DiceGroupRolls { get; set; }
        public int Total { get; set; }        
    }

    public class DiceService
    {
//        private static Regex FORMULA_REGEX = new Regex(@"^(\s*\d*d\d+\s*[\s\+])*(\s*\d*d\d+)\s*$", RegexOptions.IgnoreCase);
        private static Regex DICESPEC_SEPARATOR_REGEX = new Regex(@"[\s\+]+", RegexOptions.IgnoreCase);
        private static Regex DICESPEC_REGEX = new Regex(@"^\s*(\d*)d(\d+)\s*\+?\s*$", RegexOptions.IgnoreCase);

        private static Random _rnd = new Random();

        public static RollSpec ParseFormula(string formula)
        {
            var diceGroupSpecs = DICESPEC_SEPARATOR_REGEX.Split(formula);

            var diceGroups =
                from spec in diceGroupSpecs
                let match = DICESPEC_REGEX.Match(spec)
                let diceCount = tryIntParseOrDefault(match.Groups[1].Value, 0)
                let diceSides = int.Parse(match.Groups[2].Value)
                select new DiceGroup { DiceCount = diceCount, DiceSides = diceSides };

            return new RollSpec { Formula = formula, DiceGroups = diceGroups };
        }

        public static Roll RollDice(RollSpec spec)
        {
            var groupRolls =
               (from diceGroup in spec.DiceGroups
                let results = rollDiceGroup(diceGroup)
                let total = results.Sum()
                select new DiceGroupRoll { DiceGroup = diceGroup, Results = results, Total = total })
               .ToList();

            var grandTotal = groupRolls.Sum(g => g.Total);

            return new Roll { Formula = spec.Formula, DiceGroupRolls = groupRolls, Total = grandTotal };
        }

        private static IEnumerable<int> rollDiceGroup(DiceGroup diceGroup)
        {
            return Enumerable.Range(0, diceGroup.DiceCount)
                .Select(idx => _rnd.Next(1, diceGroup.DiceSides))
                .ToList();
        }

        private static int tryIntParseOrDefault(string value, int defaultValue)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }

            return defaultValue;
        }
    }
}
