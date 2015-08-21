using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace campaigns.Models.DAL
{
    public class Ability
    {
        public int Id { get; set; }
        public bool IsStandard { get; set; }
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
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

    public class RuleSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Race> Races { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<Ability> Abilities { get; set; }
        public virtual ICollection<Skill> Skills { get; set; }
    }
}
