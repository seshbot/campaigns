using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models.CharacterSheet
{
    public class CharacterViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Display(Name="Race")]
        public string RaceName { get; set; }
        [Display(Name="Class")]
        public string ClassName { get; set; }
        public int Level { get; set; }
        public string Description { get; set; }
        [Display(Name="Short Description")]
        public string ShortDescription
        {
            get
            {
                return (Description ?? "").Split(new[] { '.', '\n', '<' }).FirstOrDefault();
            }
        }

        public IEnumerable<AttributeValueViewModel> AttributeValues { get; set; }

        public AttributeValueViewModel GetAttributeValue(string name, string category)
        {
            return AttributeValues.FirstOrDefault(a =>
                0 == string.Compare(a.AttributeName, name, true) &&
                0 == string.Compare(a.AttributeCategory, category, true));
        }

        public AttributeValueViewModel GetCategorySingleAttributeValue(string category)
        {
            return AttributeValues.FirstOrDefault(a => 0 == string.Compare(a.AttributeCategory, category, true));
        }

        public IEnumerable<AttributeValueViewModel> GetCategoryAttributeValues(string category)
        {
            return AttributeValues
                .Where(a => 0 == string.Compare(a.AttributeCategory, category, true))
                .OrderBy(a => a.AttributeLongName)
                .OrderBy(a => a.AttributeSortOrder);
        }
    }

    public class AttributeValueViewModel
    {
        public int AttributeId { get; set; }
        public string AttributeCategory { get; set; }
        public string AttributeName { get; set; }
        public string AttributeLongName { get; set; }
        public int AttributeSortOrder { get; set; }
        public int Value { get; set; }
        public string ValueWithSign { get { return Value.ToString("+#;-#;0"); } }

        public override string ToString()
        {
            return string.Format("{0}: {1} ({2}) = {3}", AttributeId, AttributeName, AttributeCategory, Value);
        }
    }
}
