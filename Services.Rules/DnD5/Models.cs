namespace Services.Rules.DnD5
{
    public class Ability
    {
        public int Id { get; set; }
        public bool IsStandard { get; set; }
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
    }

    public class Skill
    {
        public int Id { get; set; }
        public bool IsStandard { get; set; }
        public virtual Ability Ability { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int HitDie { get; set; }
        //public List<Ability> SavingThrowProficiencies { get; set; }
        //public List<string> ArmorAndWeaponProficiencies { get; set; }
    }
}
