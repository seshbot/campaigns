﻿using Campaigns.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Campaigns.Models.DTO
{
    public class AbilityAllocationDTO
    {
        public int AbilityId { get; set; }
        public int Points { get; set; }
    }

    public class SkillAllocationDTO
    {
        public int SkillId { get; set; }
        public int Points { get; set; }
    }

    public class CharacterSheetDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Html), AllowSafeHtml]
        public string Description { get; set; }
        public int? Level { get; set; }
        public int? Experience { get; set; }
        public int? RaceId { get; set; }
        public int? ClassId { get; set; }
        public List<AbilityAllocationDTO> AbilityAllocations { get; set; }
        public List<SkillAllocationDTO> SkillAllocations { get; set; }
    }
}
