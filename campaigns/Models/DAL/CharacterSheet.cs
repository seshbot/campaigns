using Campaigns.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Campaigns.Models.DAL
{
    // TODO: LevelDescriptor (spell slots etc) | <- Class and | <- Race?
    // TODO: derived/composite properties use [DatabaseGenerated(DatabaseGeneratedOption.None)]
    // TODO: separate DAO layer into MVC, API and DB


    //// ComplexType
    //// NotMapped
    //// TimeStamp
    //// represent commands

    public class AbilityAllocation
    {
        public int Id { get; set; }
        public int AbilityId { get; set; }
        public virtual Ability Ability { get; set; }
        public int Points { get; set; }
    }

    public class SkillAllocation
    {
        public int Id { get; set; }
        public int SkillId { get; set; }
        public virtual Skill Skill { get; set; }
        public int Points { get; set; }
    }

    public enum ContributionSource { DirectlyAllocated, Ability, Skill, Race, Class }
    public enum ContributionType { Accumulate, Override }
    public class Contribution
    {
        public ContributionType Type { get; set; }
        public ContributionSource Source { get; set; }
        public int SourceId { get; set; }
        public int Value { get; set; }
    }
    
    public class SkillValueCalculation
    {
        public SkillAllocation Allocation { get; set; }
        public ICollection<Contribution> Contributions { get; set; }
        public int Value;
    }

    public class AbilityValueCalculation
    {
        public AbilityAllocation Allocation { get; set; }
        public ICollection<Contribution> Contributions { get; set; }
        public int Value { get; set; }
        public int Modifier { get; set; }
    }

    public class CharacterDerivedStatistics
    {
        public IList<AbilityValueCalculation> Abilities { get; set; }
        public IList<SkillValueCalculation> Skills { get; set; }
        public int ProficiencyBonus { get; set; }
        public IList<int> SpellSlots { get; set; }
    }

    [ComplexType]
    public class CharacterDescription
    {
        public string Name { get; set; }
        [DataType(DataType.Html), AllowSafeHtml]
        public string Text { get; set; }
        public string ShortText
        {
            get
            {
                return (Text ?? "").Split(new[] { '.', '\n', '<' }).FirstOrDefault();
            }
        }
    }
    public class CharacterSheet
    {
        public int Id { get; set; }

        public CharacterDescription Description { get; set; }

        // relationships
        public int? RaceId { get; set; }
        public virtual Race Race { get; set; }
        public int? ClassId { get; set; }
        public virtual Class Class { get; set; }
        
        // statistics
        public int? Xp { get; set; }
        public int? Level { get; set; }
        public virtual IList<AbilityAllocation> AbilityAllocations { get; set; }
        public virtual IList<SkillAllocation> SkillAllocations { get; set; }

        [NotMapped]
        public CharacterDerivedStatistics DerivedStatistics { get; set; }
    }
}
