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
            return _contributionsByAttribute[source];
        }

        public ICollection<AttributeContribution> ContributionsFor(Attribute target)
        {
            return _contributionsForAttribute[target];
        }

        public Attribute GetAttribute(string name, string category)
        {
            return _attributes[AttributeKey(name, category)];
        }

        public TestContext()
        {
            InitialValues = new List<AttributeValue>();

            //
            // create attributes
            //

            _races = new Dictionary<string, Attribute>
            {
                { "human", CreateAttribute("human", "race") },
                { "gnome", CreateAttribute("gnome", "race") }
            };

            _abilities = new Dictionary<string, Attribute>
            {
                { "str", CreateAttribute("str", "ability") },
                { "int", CreateAttribute("int", "ability") }
            };

            _abilityMods =
                _abilities.Values
                .Select(attrib => CreateAttribute(attrib.Name, "ability-modifier"))
                .ToDictionary(m => m.Name);

            _skills = new Dictionary<string, Attribute>
            {
                { "athletics", CreateAttribute("athletics", "skill") },
                { "arcana", CreateAttribute("arcana", "skill") }
            };

            //
            // create attribute contribution links
            //

            foreach (var mod in _abilities.Keys)
            {
                AddContribution(_abilities[mod].ContributionTo(_abilityMods[mod], srcVal => (srcVal - 10) / 2));
            }

            // TODO:
            //  - remove pending attributes once completed
            //  - do not assume context attributes are complete (race -> ability)
            //  - only one contributing link between src and target allowed (overwrite)

            AddContribution(_races["gnome"].ConstantContributionTo(_abilities["int"], 2));

            AddContribution(_skills["athletics"].CopyContributionFrom(_abilityMods["str"]));
            AddContribution(_skills["arcana"].CopyContributionFrom(_abilityMods["int"]));
        }

        IDictionary<string, Attribute> _attributes = new Dictionary<string, Attribute>();
        IDictionary<Attribute, IList<AttributeContribution>> _contributionsByAttribute = new Dictionary<Attribute, IList<AttributeContribution>>();
        IDictionary<Attribute, IList<AttributeContribution>> _contributionsForAttribute = new Dictionary<Attribute, IList<AttributeContribution>>();

        private string AttributeKey(string name, string category)
        {
            return name + "##" + category;
        }

        private IList<AttributeContribution> GetContributionsByAttribute(Attribute attrib)
        {
            IList<AttributeContribution> contribs;
            if (!_contributionsByAttribute.TryGetValue(attrib, out contribs))
            {
                contribs = new List<AttributeContribution>();
                _contributionsByAttribute.Add(attrib, contribs);
            }
            return contribs;
        }

        private IList<AttributeContribution> GetContributionsForAttribute(Attribute attrib)
        {
            IList<AttributeContribution> contribs;
            if (!_contributionsForAttribute.TryGetValue(attrib, out contribs))
            {
                contribs = new List<AttributeContribution>();
                _contributionsForAttribute.Add(attrib, contribs);
            }
            return contribs;
        }

        private void AddContribution(AttributeContribution contrib)
        {
            GetContributionsByAttribute(contrib.Source).Add(contrib);
            GetContributionsForAttribute(contrib.Target).Add(contrib);
        }

        int _nextId = 1;
        int AllocId() { return _nextId++; }

        private Attribute CreateAttribute(string name, string category)
        {
            var attrib = new Attribute { Id = AllocId(), Name = name, Category = category };
            _attributes.Add(AttributeKey(name, category), attrib);
            GetContributionsByAttribute(attrib);
            GetContributionsForAttribute(attrib);
            return attrib;
        }

        public IDictionary<string, Attribute> _races;
        public IDictionary<string, Attribute> _abilities;
        public IDictionary<string, Attribute> _abilityMods;
        public IDictionary<string, Attribute> _skills;

        public ICollection<Attribute> Races { get { return _races.Values; } }
        public ICollection<Attribute> Abilities { get { return _abilities.Values; } }
        public ICollection<Attribute> AbilityMods { get { return _abilityMods.Values; } }
        public ICollection<Attribute> Skills { get { return _skills.Values; } }

        public ICollection<AttributeValue> InitialValues { get; set; }
    }
}
