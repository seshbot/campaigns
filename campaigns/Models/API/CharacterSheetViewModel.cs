using Campaigns.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Campaigns.Models.API
{
    public class AbilityAllocationViewModel
    {
        public int AbilityId { get; set; }
        public int Points { get; set; }
    }

    public class SkillAllocationViewModel
    {
        public int SkillId { get; set; }
        public int Points { get; set; }
    }

    public class CharacterSheetViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Html), AllowSafeHtml]
        public string Description { get; set; }
        public int? Level { get; set; }
        public int? Xp { get; set; }
        public int? RaceId { get; set; }
        public int? ClassId { get; set; }
        public List<AbilityAllocationViewModel> AbilityAllocations { get; set; }
        public List<SkillAllocationViewModel> SkillAllocations { get; set; }
    }
}
