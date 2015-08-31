using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Calculation
{
    public interface ICalculationRules
    {
        bool IsAttributeContributing(Campaigns.Model.Attribute source);
        IEnumerable<Campaigns.Model.Attribute> ContributingAttributes { get; }
        IEnumerable<Campaigns.Model.AttributeContribution> AllContributionsTo(Campaigns.Model.Attribute target);
        IEnumerable<Campaigns.Model.AttributeContribution> AllContributionsFrom(Campaigns.Model.Attribute source);
        IEnumerable<Campaigns.Model.AttributeContribution> AllContributionsTo(int targetId);
        IEnumerable<Campaigns.Model.AttributeContribution> AllContributionsFrom(int sourceId);
    }

    public static class CalculationRulesExtensions
    {
        public static IEnumerable<Campaigns.Model.AttributeContribution> ContributionsTo(this ICalculationRules ctx, Campaigns.Model.Attribute target)
        {
            return ctx.AllContributionsTo(target)
                .Where(c => null == c.Source || ctx.IsAttributeContributing(c.Source));
        }

        public static IEnumerable<Campaigns.Model.AttributeContribution> ContributionsBy(this ICalculationRules ctx, Campaigns.Model.Attribute source)
        {
            return ctx.AllContributionsFrom(source)
                .Where(c => ctx.IsAttributeContributing(c.Target));
        }

        public static IEnumerable<Campaigns.Model.AttributeContribution> ContributionsTo(this ICalculationRules ctx, int targetId)
        {
            return ctx.AllContributionsTo(targetId)
                .Where(c => null == c.Source || ctx.IsAttributeContributing(c.Source));
        }

        public static IEnumerable<Campaigns.Model.AttributeContribution> ContributionsBy(this ICalculationRules ctx, int sourceId)
        {
            return ctx.AllContributionsFrom(sourceId)
                .Where(c => ctx.IsAttributeContributing(c.Target));
        }
    }
}
