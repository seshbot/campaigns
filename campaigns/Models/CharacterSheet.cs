using campaigns.Helpers;
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

namespace campaigns.Models
{
    // TODO: LevelDescriptor (spell slots etc) | <- Class and | <- Race?
    // TODO: derived/composite properties use [DatabaseGenerated(DatabaseGeneratedOption.None)]
    // TODO: separate DAO layer into MVC, API and DB

    public enum Dice
    {
        D4,
        D6,
        D8,
        D10,
        D12,
        D20,
    }

    public class Ability
    {
        public int Id { get; set; }
        public bool IsStandard { get; set; }
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public string ShortName{ get; set; }
        [DataType(DataType.Html)]
        public string Description { get; set; }
    }

    public class Skill
    {
        public int Id { get; set; }
        public bool IsStandard { get; set; }
        public virtual Ability Ability { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Html)]
        public string Description { get; set; }
    }

    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Html)]
        public string Description { get; set; }
        //public int HitDie { get; set; }
        //public List<Ability> SavingThrowProficiencies { get; set; }
        //public List<string> ArmorAndWeaponProficiencies { get; set; }
    }

    public class Race
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Html)]
        public string Description { get; set; }
        //public List<AbilityScore> AbilityIncrease { get; set; }
    }

    public class Rules
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Race> Races { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<Ability> Abilities { get; set; }
        public virtual ICollection<Skill> Skills { get; set; }
    }

    //// ComplexType
    //// NotMapped
    //// TimeStamp
    //// represent commands

    public class AbilityAllocation
    {
        public int Id { get; set; }
        public virtual Ability Ability { get; set; }
        public int Points { get; set; }
    }

    public class SkillAllocation
    {
        public int Id { get; set; }
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

    [NotMapped]
    public class CharacterDerivedStatistics
    {
        public int Level { get; set; }
        public IList<AbilityValueCalculation> Abilities { get; set; }
        public IList<SkillValueCalculation> Skills { get; set; }
        public int ProficiencyBonus { get; set; }
        public IList<int> SpellSlots { get; set; }
    }

    public class CharacterSheet
    {
        public int Id { get; set; }

        // description
        public string Name { get; set; }
        [DataType(DataType.Html), AllowSafeHtml]
        public string Description { get; set; }
        public string ShortDescription {
            get {
                return (Description ?? "").Split(new[] { '.', '\n', '<' }).FirstOrDefault();
            }
        }
        public int RaceId { get; set; }
        public virtual Race Race { get; set; }
        public int ClassId { get; set; }
        public virtual Class Class { get; set; }

        // statistics
        public int Experience { get; set; }
        public virtual IList<AbilityAllocation> AbilityAllocations { get; set; }
        public virtual IList<SkillAllocation> SkillAllocations { get; set; }

        public CharacterDerivedStatistics DerivedStatistics { get; set; }
    }
}