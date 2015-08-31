using Services.Calculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models.API
{
    public class AttributeContributionViewModel
    {
        public int SourceId { get; set; }
        public string SourceName { get; set; }
        public int TargetId { get; set; }
        public string TargetName { get; set; }
        public string FormulaString { get; set; }
    }

    public class AttributeViewModel
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string LongName { get; set; }
        public string Description { get; set; }

        public IEnumerable<AttributeContributionViewModel> ContributionsFrom { get; set; }
        public IEnumerable<AttributeContributionViewModel> ContributionsTo { get; set; }
    }

    public class AttributeCategoryViewModel
    {
        public string Name { get; set; }
        public int AttributeCount { get; set; }
    }
}
