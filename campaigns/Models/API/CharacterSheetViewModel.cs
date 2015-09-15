using Campaigns.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Campaigns.Models.API
{
    public class AttributeValueViewModel
    {
        public int AttributeId { get; set; }
        public int Value { get; set; }
    }

    public class CharacterViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Html), AllowSafeHtml]
        public string Description { get; set; }

        public int LatestSheetId { get; set; }

        public IEnumerable<AttributeValueViewModel> AttributeValues { get; set; }
    }
    
    public class AttributeAllocationViewModel
    {
        public int AttributeId { get; set; }
        public int Value { get; set; }
    }

    public class CharacterAttributeAllocationsViewModel
    {
        public int CharacterId { get; set; }
        public int CharacterSheetId { get; set; }

        public IEnumerable<AttributeAllocationViewModel> AttributeAllocations { get; set; }
    }
}
