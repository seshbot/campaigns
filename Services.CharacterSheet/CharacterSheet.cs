using Campaigns.Core;
using Services.Calculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.CharacterSheets
{
    public class CharacterSpec : BaseEntity
    {
        public static CharacterSpec EMPTY = new CharacterSpec { Attributes = new List<Calculation.Attribute>(), Allocations = new List<Calculation.AttributeContribution>() };

        public IEnumerable<Calculation.Attribute> Attributes { get; set; }
        public IEnumerable<Calculation.AttributeContribution> Allocations { get; set; }
    }

    // TODO: create annotations to allow validation/attribute relationship constraints
    public class CharacterSheet : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription
        {
            get
            {
                return (Description ?? "").Split(new[] { '.', '\n', '<' }).FirstOrDefault();
            }
        }
        
        public CharacterSpec Spec { get; set; }
        public ICollection<AttributeValue> AttributeValues { get; set; }
    }
}
