using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Calculations;
using campaigns.Models.DAL;

namespace campaigns.Models
{
    public class RulesDbContext : DbContext
    {
        public RulesDbContext()
        {
            Database.SetInitializer(new RulesInitializer());
        }

        public DbSet<Model.Calculations.Attribute> Attributes { get; set; }
        //public DbSet<Model.Calculations.AttributeContribution> AttributeContributions { get; set; }

    }

    public static class RulesDbContextExtensions
    {
        public static Model.Calculations.Attribute Attrib(this RulesDbContext db, string name, string category)
        {
            return db.Attributes
                .FirstOrDefault(a => 0 == String.Compare(a.Name, name, true) && 0 == String.Compare(a.Category, category, true));
        }

        public static IEnumerable<Model.Calculations.Attribute> AttribsForCategory(this RulesDbContext db, string category)
        {
            return db.Attributes.Where(a => 0 == String.Compare(a.Category, category, true));
        }

        public static IEnumerable<Model.Calculations.Attribute> StandardAttribs(this RulesDbContext db)
        {
            return db.Attributes.Where(a => a.IsStandard);
        }

        public static IEnumerable<AttributeValue> CharacterIntrinsicAttribs(this RulesDbContext db, CharacterSheet characterSheet)
        {
            if (null != characterSheet.Race)
            {
                yield return new AttributeValue { Attribute = db.Attrib(characterSheet.Race.Name, "race"), Value = 0 };
            }
            if (null != characterSheet.Class)
            {
                yield return new AttributeValue { Attribute = db.Attrib(characterSheet.Class.Name, "class"), Value = 0 };
            }
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
                cache.AddContribution(source.ContributionTo(target, val => val / 2 - 5));
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

        public RulesCalculationContext(RulesDbContext db, CharacterSheet characterSheet)
        {
            _db = db;
            _rules = Rules(_db);

            var standardAbilityValues =
                _db.StandardAttribs()
                .Select(a => new AttributeValue { Attribute = a, Value = 0 });

            _attributeValuesByAttributeId =
                _db.CharacterIntrinsicAttribs(characterSheet)
                .Concat(standardAbilityValues)
                .ToDictionary(a => a.Attribute.Id);

            foreach (var ability in characterSheet.AbilityAllocations)
            {
                var attrib = _db.Attrib(ability.Ability.ShortName, "ability");
                _attributeValuesByAttributeId[attrib.Id].Value = ability.Points;
            }
        }

        private IDictionary<int, AttributeValue> _attributeValuesByAttributeId;
        public IEnumerable<AttributeValue> ContributingAttributes { get { return _attributeValuesByAttributeId.Values; } }

        public bool IsAttributeContributing(Model.Calculations.Attribute source)
        {
            return _attributeValuesByAttributeId.ContainsKey(source.Id);
        }

        public IEnumerable<AttributeContribution> AllContributionsBy(Model.Calculations.Attribute source)
        {
            return _rules.ContributionsBy(source);
        }

        public IEnumerable<AttributeContribution> AllContributionsFor(Model.Calculations.Attribute target)
        {
            return _rules.ContributionsFor(target);
        }

        public IEnumerable<AttributeContribution> AllContributionsBy(int sourceId)
        {
            return _rules.ContributionsBy(sourceId);
        }

        public IEnumerable<AttributeContribution> AllContributionsFor(int targetId)
        {
            return _rules.ContributionsFor(targetId);
        }
    }

    public class RulesInitializer : DropCreateDatabaseIfModelChanges<RulesDbContext>
    {
        protected override void Seed(RulesDbContext context)
        {
            var dnd = new DnD5.RulesDescription();

            var races = dnd.Races
                .Select(r => new Model.Calculations.Attribute { Category = "race", Name = r.Name, IsStandard = false })
                .ToDictionary(r => r.Name.ToLower());
            
            var abilities = dnd.Abilities
                .Select(a => new Model.Calculations.Attribute { Category = "ability", Name = a.ShortName, IsStandard = true })
                .ToDictionary(a => a.Name.ToLower());
            
            var abilityMods = dnd.Abilities
                .Select(a => new Model.Calculations.Attribute { Category = "ability-modifier", Name = a.ShortName, IsStandard = true })
                .ToDictionary(a => a.Name.ToLower());
            
            var classes = dnd.Classes
                .Select(c => new Model.Calculations.Attribute { Category = "class", Name = c.Name, IsStandard = false })
                .ToDictionary(a => a.Name.ToLower());
            
            var skills = dnd.Skills
                .Select(c => new Model.Calculations.Attribute { Category = "skill", Name = c.Name, IsStandard = true })
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
