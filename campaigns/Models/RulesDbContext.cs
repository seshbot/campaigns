using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Calculations;

namespace campaigns.Models
{
    public class RulesDbContext : DbContext
    {
        public RulesDbContext()
        {
            Database.SetInitializer(new RulesInitializer());
        }

        public DbSet<Model.Calculations.Attribute> Attributes { get; set; }

        public Model.Calculations.Attribute Attrib(string name, string category)
        {
            return Attributes
                .FirstOrDefault(a => 0 == String.Compare(a.Name, name) && 0 == String.Compare(a.Category, category));
        }
    }

    public class RulesCalculationContext : ICalculationContext
    {
        private static InMemoryRules _rulesSingleton;
        private static InMemoryRules Rules(RulesDbContext db)
        {
            if (null == _rulesSingleton)
            {
                _rulesSingleton = CreateRules(db);
            }

            return _rulesSingleton;
        }

        private static InMemoryRules CreateRules(RulesDbContext db)
        {
            var rules = new DnD5.RulesDescription();
            var cache = new InMemoryRules();
            foreach (var attrib in db.Attributes)
            {
                cache.AddAttribute(attrib);
            }
            foreach (var ability in rules.Abilities)
            {
                var source = db.Attrib(ability.ShortName, "ability");
                var target = db.Attrib(ability.ShortName, "ability-modifier");
                cache.AddContribution(source.ContributionTo(target, val => (val - 10) / 2));
            }
            foreach (var skill in rules.Skills)
            {
                var source = db.Attrib(skill.Ability.ShortName, "ability-modifier");
                var target = db.Attrib(skill.Name, "skill");
                cache.AddContribution(source.CopyContributionTo(target));
            }
            foreach (var bonus in rules.RacialBonuses)
            {
                var source = db.Attrib(bonus.Race.Name, "race");
                foreach (var kvp in bonus.Bonuses)
                {
                    var target = db.Attrib(kvp.Key.ShortName, "ability");
                    cache.AddContribution(source.ConstantContributionTo(target, kvp.Value));
                }
            }
            return cache;
        }

        private RulesDbContext _db;
        private InMemoryRules _rules;

        public RulesCalculationContext(RulesDbContext db)
        {
            _db = db;
            _rules = Rules(_db);
        }

        public ICollection<AttributeValue> InitialValues { get; set; }

        public ICollection<AttributeContribution> ContributionsBy(Model.Calculations.Attribute source)
        {
            return _rules.ContributionsBy(source);
        }

        public ICollection<AttributeContribution> ContributionsFor(Model.Calculations.Attribute target)
        {
            return _rules.ContributionsFor(target);
        }
    }

    public class RulesInitializer : DropCreateDatabaseIfModelChanges<RulesDbContext>
    {
        protected override void Seed(RulesDbContext context)
        {
            var dnd = new DnD5.RulesDescription();

            var races = dnd.Races
                .Select(r => new Model.Calculations.Attribute { Category = "race", Name = r.Name })
                .ToDictionary(r => r.Name.ToLower());
            
            var abilities = dnd.Abilities
                .Select(a => new Model.Calculations.Attribute { Category = "ability", Name = a.ShortName })
                .ToDictionary(a => a.Name.ToLower());
            
            var abilityMods = dnd.Abilities
                .Select(a => new Model.Calculations.Attribute { Category = "ability-modifier", Name = a.ShortName })
                .ToDictionary(a => a.Name.ToLower());
            
            var classes = dnd.Classes
                .Select(c => new Model.Calculations.Attribute { Category = "class", Name = c.Name })
                .ToDictionary(a => a.Name.ToLower());
            
            var skills = dnd.Skills
                .Select(c => new Model.Calculations.Attribute { Category = "skill", Name = c.Name })
                .ToDictionary(a => a.Name.ToLower());

            context.Attributes.AddRange(races.Values);
            context.Attributes.AddRange(abilities.Values);
            context.Attributes.AddRange(abilityMods.Values);
            context.Attributes.AddRange(classes.Values);
            context.Attributes.AddRange(skills.Values);

            context.SaveChanges();
        }
    }
}
