using Campaigns.Core;
using Newtonsoft.Json;
using Serialize.Linq.Serializers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Services.Calculation
{
    public class AttributeContribution : BaseEntity
    {
        [ForeignKey("Source")]
        public int? SourceId { get; set; }

        public virtual Attribute Source { get; set; }

        [ForeignKey("Target")]
        public int? TargetId { get; set; }

        public virtual Attribute Target { get; set; }

        [JsonIgnore]
        public string FormulaJson { get; set; }

        [JsonIgnore]
        [NotMapped]
        public Expression<Func<int, int>> FormulaExpression
        {
            set
            {
                if (null == value)
                    throw new ArgumentNullException("formula");

                var serializer = new ExpressionSerializer(new Serialize.Linq.Serializers.JsonSerializer());
                FormulaJson = serializer.SerializeText(value);
            }
        }

        private Func<int, int> _formula;
        [JsonIgnore]
        [NotMapped]
        public Func<int, int> Formula
        {
            get
            {
                if (false || null == _formula)
                {
                    var serializer = new ExpressionSerializer(new Serialize.Linq.Serializers.JsonSerializer());
                    var expression = (Expression<Func<int, int>>)serializer.DeserializeText(FormulaJson);
                    _formula = expression.Compile();
                }
                return _formula;
            }
        }
    }

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
            return new AttributeContribution { Target = target, Source = source, FormulaExpression = formula };
        }

        public static AttributeContribution ConstantContributionFrom(this Attribute target, Attribute source, int val)
        {
            return ContributionTo(source, target, Constant(val));
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

        public static AttributeContribution DirectContributionTo(this Attribute target, int val)
        {
            return new AttributeContribution { Target = target, FormulaExpression = Constant(val) };
        }
    }
}
