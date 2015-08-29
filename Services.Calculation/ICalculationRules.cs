using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Calculation
{
    public interface ICalculationRules
    {
        bool IsAttributeContributing(Attribute source);
        IEnumerable<Attribute> ContributingAttributes { get; }
        IEnumerable<AttributeContribution> AllContributionsTo(Attribute target);
        IEnumerable<AttributeContribution> AllContributionsFrom(Attribute source);
        IEnumerable<AttributeContribution> AllContributionsTo(int targetId);
        IEnumerable<AttributeContribution> AllContributionsFrom(int sourceId);
    }

    public static class CalculationRulesExtensions
    {
        public static IEnumerable<AttributeContribution> ContributionsTo(this ICalculationRules ctx, Attribute target)
        {
            return ctx.AllContributionsTo(target)
                .Where(c => null == c.Source || ctx.IsAttributeContributing(c.Source));
        }

        public static IEnumerable<AttributeContribution> ContributionsBy(this ICalculationRules ctx, Attribute source)
        {
            return ctx.AllContributionsFrom(source)
                .Where(c => ctx.IsAttributeContributing(c.Target));
        }

        public static IEnumerable<AttributeContribution> ContributionsTo(this ICalculationRules ctx, int targetId)
        {
            return ctx.AllContributionsTo(targetId)
                .Where(c => null == c.Source || ctx.IsAttributeContributing(c.Source));
        }

        public static IEnumerable<AttributeContribution> ContributionsBy(this ICalculationRules ctx, int sourceId)
        {
            return ctx.AllContributionsFrom(sourceId)
                .Where(c => ctx.IsAttributeContributing(c.Target));
        }
    }
}
