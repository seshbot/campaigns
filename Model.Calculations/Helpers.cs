using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Model.Calculations
{
    public static class AttributeContributions
    {
        private static Expression<Func<int, int>> Constant(int val) { return v => val; }
        private static Expression<Func<int, int>> Copy() { return v => v; }

        public static AttributeContribution ContributionFrom(this Attribute target, Attribute source, Expression<Func<int, int>> formula)
        {
            return ContributionTo(source, target, formula);
        }

        public static AttributeContribution ContributionTo(this Attribute source, Attribute target, Expression<Func<int, int>> formula)
        {
            return new AttributeContribution { Target = target, Source = source, Formula = formula };
        }

        public static AttributeContribution ConstantContributionFrom(this Attribute target, Attribute source, int val)
        {
            return ConstantContributionTo(source, target, val);
        }

        public static AttributeContribution ConstantContributionTo(this Attribute source, Attribute target, int val)
        {
            return ContributionTo(source, target, Constant(val));
        }

        public static AttributeContribution CopyContributionFrom(this Attribute target, Attribute source)
        {
            return CopyContributionTo(source, target);
        }

        public static AttributeContribution CopyContributionTo(this Attribute source, Attribute target)
        {
            return ContributionTo(source, target, Copy());
        }
    }
}
