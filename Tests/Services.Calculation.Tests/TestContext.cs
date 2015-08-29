using Campaigns.Core.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Rules.Data;

namespace Services.Calculation.Tests
{
    /// <summary>
    /// The text calculation context has a minimal set of attributes and contributions:
    ///  - abilitities and modifiers: int, str
    ///  - skills: arcana (int) and athletics (str)
    ///  - races: human and gnome (gnome -> +2 int)
    /// Ensure you 'SetIntialValue()' on any attributes you want included in your calculation!
    /// </summary>
    class TestRules : ICalculationRules
    {
        public void SetInitialValue(Attribute attribute, int value)
        {
            _contributionsDb.Add(attribute.ConstantContributionFrom(null, value));
            _contributingAttributes.Add(attribute);
        }
        
        public IEnumerable<AttributeContribution> AllContributionsTo(int targetId)
        {
            return AllContributionsTo(_attributesDb.GetById(targetId));
        }

        public IEnumerable<AttributeContribution> AllContributionsFrom(int sourceId)
        {
            return AllContributionsFrom(_attributesDb.GetById(sourceId));
        }

        public IEnumerable<AttributeContribution> AllContributionsFrom(Attribute source)
        {
            return _contributionsDb.ContributionsFrom(source);
        }

        public IEnumerable<AttributeContribution> AllContributionsTo(Attribute target)
        {
            return _contributionsDb.ContributionsTo(target);
        }

        private Attribute CreateAttribute(string name, string category, bool isStandard)
        {
            var result = new Attribute { Name = name, Category = category, IsStandard = isStandard };
            _attributesDb.Add(result);
            return result;
        }

        public TestRules()
        {
            _attributesDb = new InMemoryEntityRepository<Attribute>();
            _contributionsDb = new InMemoryEntityRepository<AttributeContribution>();
            _contributionsDb.AddForeignStore(_attributesDb);

            _contributingAttributes = new HashSet<Attribute>();

            //
            // create attributes
            //

            _races = new Dictionary<string, Attribute>
            {
                { "human", CreateAttribute("human", "races", isStandard: false) },
                { "gnome", CreateAttribute("gnome", "races", isStandard: false) }
            };

            _abilities = new Dictionary<string, Attribute>
            {
                { "str", CreateAttribute("str", "abilities", isStandard: true) },
                { "int", CreateAttribute("int", "abilities", isStandard: true) }
            };

            _abilityMods =
                _abilities.Values
                .Select(attrib => CreateAttribute(attrib.Name, "ability-modifiers", isStandard: true))
                .ToDictionary(m => m.Name);

            _skills = new Dictionary<string, Attribute>
            {
                { "athletics", CreateAttribute("athletics", "skills", isStandard: true) },
                { "arcana", CreateAttribute("arcana", "skills", isStandard: true) }
            };

            // TODO: this should be standard behaviour
            // ensure all standard attributes are contributing
            foreach (var attrib in AllAttributes.Where(a => a.IsStandard))
            {
                SetInitialValue(attrib, 0);
            }

            //
            // create attribute contribution links
            //

            foreach (var mod in _abilities.Keys)
            {
                var contribution = _abilities[mod].ContributionTo(_abilityMods[mod], srcVal => (srcVal / 2) - 5);
                _contributionsDb.Add(contribution);
            }

            // TODO:
            //  - only one contributing link between src and target allowed (overwrite)

            _contributionsDb.Add(_races["gnome"].ConstantContributionTo(_abilities["int"], 2));

            _contributionsDb.Add(_skills["athletics"].CopyContributionFrom(_abilityMods["str"]));
            _contributionsDb.Add(_skills["arcana"].CopyContributionFrom(_abilityMods["int"]));
        }

        private InMemoryEntityRepository<Attribute> _attributesDb;
        private InMemoryEntityRepository<AttributeContribution> _contributionsDb;

        private IDictionary<string, Attribute> _races;
        private IDictionary<string, Attribute> _abilities;
        private IDictionary<string, Attribute> _abilityMods;
        private IDictionary<string, Attribute> _skills;

        public IEnumerable<Attribute> Races { get { return _races.Values; } }
        public IEnumerable<Attribute> Abilities { get { return _abilities.Values; } }
        public IEnumerable<Attribute> AbilityMods { get { return _abilityMods.Values; } }
        public IEnumerable<Attribute> Skills { get { return _skills.Values; } }

        private IEnumerable<Attribute> AllAttributes { get { return Abilities.Concat(AbilityMods).Concat(Races).Concat(Skills); } }

        public Attribute Race(string name) { return _races[name]; }
        public Attribute Ability(string name) { return _abilities[name]; }
        public Attribute AbilityMod(string name) { return _abilityMods[name]; }
        public Attribute Skill(string name) { return _skills[name]; }

        private ISet<Attribute> _contributingAttributes;
        
        public bool IsAttributeContributing(Attribute source)
        {
            return _contributingAttributes.Contains(source);
        }

        public IEnumerable<Attribute> ContributingAttributes { get { return _contributingAttributes; } }
    }
}
