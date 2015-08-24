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
        public DbSet<Model.Calculations.AttributeContribution> AttributeContributions { get; set; }

    }

    public static class RulesDbContextExtensions
    {
        public static Model.Calculations.Attribute GetAttribute(this RulesDbContext db, string name, string category)
        {
            return db.Attributes
                .FirstOrDefault(a => 0 == string.Compare(a.Name, name, true) && 0 == string.Compare(a.Category, category, true));
        }

        public static IEnumerable<Model.Calculations.Attribute> GetAttributesInCategory(this RulesDbContext db, string category)
        {
            return db.Attributes.Where(a => 0 == string.Compare(a.Category, category, true));
        }

        public static IEnumerable<Model.Calculations.Attribute> GetStandardAttributes(this RulesDbContext db)
        {
            return db.Attributes.Where(a => a.IsStandard);
        }

        public static IEnumerable<AttributeValue> GetIntrinsicAttributes(this RulesDbContext db, CharacterSheet characterSheet)
        {
            if (null != characterSheet.Race)
            {
                yield return new AttributeValue { Attribute = db.GetAttribute(characterSheet.Race.Name, "races"), Value = 0 };
            }
            if (null != characterSheet.Class)
            {
                yield return new AttributeValue { Attribute = db.GetAttribute(characterSheet.Class.Name, "classes"), Value = 0 };
            }
        }

        public static IEnumerable<Model.Calculations.AttributeContribution> GetContributionsToAttribute(this RulesDbContext db, int id)
        {
            return db.AttributeContributions.Where(c => c.SourceId == id);
        }

        public static IEnumerable<Model.Calculations.AttributeContribution> GetContributionsByAttribute(this RulesDbContext db, int id)
        {
            return db.AttributeContributions.Where(c => c.TargetId == id);
        }

        public static IEnumerable<AttributeContribution> CreateContributions(this RulesDbContext db)
        {
            var rules = new DnD5.RulesDescription();
            foreach (var ability in rules.Abilities)
            {
                var source = db.GetAttribute(ability.ShortName, "abilities");
                var target = db.GetAttribute(ability.ShortName, "ability-modifiers");
                yield return source.ContributionTo(target, val => val / 2 - 5);
            }
            foreach (var skill in rules.Skills)
            {
                var source = db.GetAttribute(skill.Ability.ShortName, "ability-modifiers");
                var target = db.GetAttribute(skill.Name, "skills");
                yield return source.CopyContributionTo(target);
            }
            foreach (var bonus in rules.RacialBonuses)
            {
                var source = db.GetAttribute(bonus.Race.Name, "races");
                foreach (var kvp in bonus.Bonuses)
                {
                    var target = db.GetAttribute(kvp.Key.ShortName, "abilities");
                    yield return source.ConstantContributionTo(target, kvp.Value);
                }
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

            foreach (var contrib in db.CreateContributions())
            {
                cache.AddContribution(contrib);
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
                _db.GetStandardAttributes()
                .Select(a => new AttributeValue { Attribute = a, Value = 0 });

            _attributeValuesByAttributeId =
                _db.GetIntrinsicAttributes(characterSheet)
                .Concat(standardAbilityValues)
                .ToDictionary(a => a.Attribute.Id);

            foreach (var ability in characterSheet.AbilityAllocations)
            {
                var attrib = _db.GetAttribute(ability.Ability.ShortName, "abilities");
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
                .Select(r => new Model.Calculations.Attribute { Category = "races", Name = r.Name, IsStandard = false, Description = r.Description })
                .ToDictionary(r => r.Name.ToLower());
            
            var abilities = dnd.Abilities
                .Select(a => new Model.Calculations.Attribute { Category = "abilities", Name = a.ShortName, IsStandard = true, Description = a.Description, SortOrder = a.SortOrder })
                .ToDictionary(a => a.Name.ToLower());
            
            var abilityMods = dnd.Abilities
                .Select(a => new Model.Calculations.Attribute { Category = "ability-modifiers", Name = a.ShortName, IsStandard = true, Description = a.Description, SortOrder = a.SortOrder })
                .ToDictionary(a => a.Name.ToLower());
            
            var classes = dnd.Classes
                .Select(c => new Model.Calculations.Attribute { Category = "classes", Name = c.Name, IsStandard = false, Description = c.Description })
                .ToDictionary(a => a.Name.ToLower());
            
            var skills = dnd.Skills
                .Select(s => new Model.Calculations.Attribute { Category = "skills", Name = s.Name, IsStandard = true, Description = s.Description })
                .ToDictionary(a => a.Name.ToLower());

            context.Attributes.AddRange(races.Values);
            context.Attributes.AddRange(abilities.Values);
            context.Attributes.AddRange(abilityMods.Values);
            context.Attributes.AddRange(classes.Values);
            context.Attributes.AddRange(skills.Values);
            
            context.SaveChanges();

            var contributions = context.CreateContributions();
            context.AttributeContributions.AddRange(contributions);

            context.SaveChanges();
        }
    }
}
