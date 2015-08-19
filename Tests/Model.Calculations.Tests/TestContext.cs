using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Calculations.Tests
{
    class TestContext : ICalculationContext
    {
        public void SetInitialValue(Attribute attribute, int value)
        {
            InitialValues.Add(new AttributeValue { Attribute = attribute, Value = value });
        }

        public ICollection<AttributeContribution> ContributionsBy(Attribute source)
        {
            return _rules.ContributionsBy(source);
        }

        public ICollection<AttributeContribution> ContributionsFor(Attribute target)
        {
            return _rules.ContributionsFor(target);
        }

        public TestContext()
        {
            _rules = new InMemoryRules();
            InitialValues = new List<AttributeValue>();

            //
            // create attributes
            //

            _races = new Dictionary<string, Attribute>
            {
                { "human", _rules.CreateAttribute("human", "race") },
                { "gnome", _rules.CreateAttribute("gnome", "race") }
            };

            _abilities = new Dictionary<string, Attribute>
            {
                { "str", _rules.CreateAttribute("str", "ability") },
                { "int", _rules.CreateAttribute("int", "ability") }
            };

            _abilityMods =
                _abilities.Values
                .Select(attrib => _rules.CreateAttribute(attrib.Name, "ability-modifier"))
                .ToDictionary(m => m.Name);

            _skills = new Dictionary<string, Attribute>
            {
                { "athletics", _rules.CreateAttribute("athletics", "skill") },
                { "arcana", _rules.CreateAttribute("arcana", "skill") }
            };

            //
            // create attribute contribution links
            //

            foreach (var mod in _abilities.Keys)
            {
                _rules.AddContribution(_abilities[mod].ContributionTo(_abilityMods[mod], srcVal => (srcVal - 10) / 2));
            }

            // TODO:
            //  - remove pending attributes once completed
            //  - do not assume context attributes are complete (race -> ability)
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

        public ICollection<Attribute> Races { get { return _races.Values; } }
        public ICollection<Attribute> Abilities { get { return _abilities.Values; } }
        public ICollection<Attribute> AbilityMods { get { return _abilityMods.Values; } }
        public ICollection<Attribute> Skills { get { return _skills.Values; } }

        public Attribute Race(string name) { return _races[name]; }
        public Attribute Ability(string name) { return _abilities[name]; }
        public Attribute AbilityMod(string name) { return _abilityMods[name]; }
        public Attribute Skill(string name) { return _skills[name]; }

        public ICollection<AttributeValue> InitialValues { get; set; }
    }
}
