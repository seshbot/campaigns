using Campaigns.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models
{
    public enum Mode
    {
        View,
        Create,
        Edit,
    }

    public class AttributeViewModel<AttributeType>
    {
        public int AttributeId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public bool IsSet { get; set; }
        public AttributeType Value { get; set; }
    }

    public class AttributeContribution<AttributeType>
    {
        public int ContributionId { get; set; }
        public int SourceAttributeId { get; set; }
        public string SourceAttributeName { get; set; }
        public AttributeType Value { get; set; }
    }

    public class DerivedAttributeViewModel<AttributeType> : AttributeViewModel<AttributeType>
    {
        public AttributeType BaseValue { get; set; }
        public IList<AttributeContribution<AttributeType>> Contributions { get; set; }
    }

    public class CharacterSheetViewModel
    {
        public Mode Mode { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }

        public int? Xp { get; set; }
        public int? Level { get; set; }

        public AttributeViewModel<string> Race { get; set; }
        public AttributeViewModel<string> Class { get; set; }

        public DerivedAttributeViewModel<int> ProficiencyBonus { get; set; }

        public IEnumerable<DerivedAttributeViewModel<int>> Abilities { get; set; }
        public IEnumerable<DerivedAttributeViewModel<int>> Skills { get; set; }
    }
}
