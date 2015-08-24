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

namespace Model.Calculations
{
    public class Attribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public string Category { get; set; }
        /// indicates that this attribute should be included in all calculations
        public bool IsStandard { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} ({2})", Id, Name, Category);
        }
    }

    public class AttributeContribution
    {
        public int Id { get; set; }

        [ForeignKey("Source")]
        public int? SourceId { get; set; }

        [ForeignKey("Target")]
        public int? TargetId { get; set; }

        public virtual Attribute Source { get; set; }

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
}
