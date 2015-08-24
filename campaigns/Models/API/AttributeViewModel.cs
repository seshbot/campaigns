using Model.Calculations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace campaigns.Models.API
{
    public class AttributeViewModel
    {
        public Model.Calculations.Attribute Attribute { get; set; }
        public IEnumerable<AttributeContribution> ContributesTo { get; set; }
        public IEnumerable<AttributeContribution> ContributedToBy { get; set; }
    }

    public class AttributeCategoryViewModel
    {
        public string Name { get; set; }
        public int AttributeCount { get; set; }
    }
}
