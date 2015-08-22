using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Calculations.Tests
{
    /// <summary>
    /// The text calculation context has a minimal set of attributes and contributions:
    ///  - abilitities and modifiers: int, str
    ///  - skills: arcana (int) and athletics (str)
    ///  - races: human and gnome (gnome -> +2 int)
    /// Ensure you 'SetIntialValue()' on any attributes you want included in your calculation!
    /// </summary>
    class TestContext : ICalculationContext
    {
        public void SetInitialValue(Attribute attribute, int value)
        {
            AttributeValue attribValue;
            if (!_contributingAttributes.TryGetValue(attribute, out attribValue))
            {
                attribValue = new AttributeValue { Attribute = attribute };
                _contributingAttributes.Add(attribute, attribValue);
            }
            attribValue.Value = value;
        }

        public IEnumerable<AttributeContribution> AllContributionsBy(Attribute source)
        {
            return _rules.ContributionsBy(source);
        }

        public IEnumerable<AttributeContribution> AllContributionsFor(Attribute target)
        {
            return _rules.ContributionsFor(target);
        }

        public IEnumerable<AttributeContribution> AllContributionsFor(int targetId)
        {
            return _rules.ContributionsFor(targetId);
        }

        public IEnumerable<AttributeContribution> AllContributionsBy(int sourceId)
        {
            return _rules.ContributionsBy(sourceId);
        }

        public TestContext()
        {
            _rules = new InMemoryRules();
            _contributingAttributes = new Dictionary<Attribute, AttributeValue>();

            //
            // create attributes
            //

            _races = new Dictionary<string, Attribute>
            {
                { "human", _rules.CreateAttribute("human", "race", isStandard: false) },
                { "gnome", _rules.CreateAttribute("gnome", "race", isStandard: false) }
            };

            _abilities = new Dictionary<string, Attribute>
            {
                { "str", _rules.CreateAttribute("str", "ability", isStandard: true) },
                { "int", _rules.CreateAttribute("int", "ability", isStandard: true) }
            };

            _abilityMods =
                _abilities.Values
                .Select(attrib => _rules.CreateAttribute(attrib.Name, "ability-modifier", isStandard: true))
                .ToDictionary(m => m.Name);

            _skills = new Dictionary<string, Attribute>
            {
                { "athletics", _rules.CreateAttribute("athletics", "skill", isStandard: true) },
                { "arcana", _rules.CreateAttribute("arcana", "skill", isStandard: true) }
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
                _rules.AddContribution(_abilities[mod].ContributionTo(_abilityMods[mod], srcVal => (srcVal / 2) - 5));
            }

            // TODO:
            //  - only one contributing link between src and target allowed (overwrite)

            _rules.AddContribution(_races["gnome"].ConstantContributionTo(_abilities["int"], 2));

            _rules.AddContribution(_skills["athletics"].CopyContributionFrom(_abilityMods["str"]));
            _rules.AddContribution(_skills["arcana"].CopyContributionFrom(_abilityMods["int"]));
        }

        private InMemoryRules _rules;

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

        private IDictionary<Attribute, AttributeValue> _contributingAttributes;
        
        public bool IsAttributeContributing(Attribute source)
        {
            return _contributingAttributes.ContainsKey(source);
        }

        public IEnumerable<AttributeValue> ContributingAttributes { get { return _contributingAttributes.Values; } }
    }
}
