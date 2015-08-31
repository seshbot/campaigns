using Campaigns.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Calculation
{
    public class CalculationResult
    {
        public ICollection<AttributeValue> AttributeValues { get; set; }
        public IEnumerable<AttributeValue> AttributeValuesForCategory(string category)
        {
            return AttributeValues.Where(val => 0 == string.Compare(val.Attribute.Category, category, true));
        }
        public AttributeValue AttributeValue(string name, string category)
        {
            return AttributeValues
                .FirstOrDefault(val =>
                    0 == string.Compare(val.Attribute.Category, category, true) &&
                    0 == string.Compare(val.Attribute.Name, name, true));
        }
    }
}
