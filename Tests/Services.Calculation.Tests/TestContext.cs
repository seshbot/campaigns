using Campaigns.Core.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Rules.Data;
using Campaigns.Model;

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
        public void SetInitialValue(Campaigns.Model.Attribute attribute, int value)
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

        public IEnumerable<AttributeContribution> AllContributionsFrom(Campaigns.Model.Attribute source)
        {
            return _contributionsDb.ContributionsFrom(source);
        }

        public IEnumerable<AttributeContribution> AllContributionsTo(Campaigns.Model.Attribute target)
        {
            return _contributionsDb.ContributionsTo(target);
        }

        private Campaigns.Model.Attribute CreateAttribute(string name, string category, bool isStandard)
        {
            var result = new Campaigns.Model.Attribute { Name = name, Category = category, IsStandard = isStandard };
            _attributesDb.Add(result);
            return result;
        }

        public TestRules()
        {
            _attributesDb = new InMemoryEntityRepository<Campaigns.Model.Attribute>();
            _contributionsDb = new InMemoryEntityRepository<AttributeContribution>();
            _contributionsDb.AddForeignStore(_attributesDb);

            _contributingAttributes = new HashSet<Campaigns.Model.Attribute>();

            //
            // create attributes
            //

            _races = new Dictionary<string, Campaigns.Model.Attribute>
            {
                { "human", CreateAttribute("human", "races", isStandard: false) },
                { "gnome", CreateAttribute("gnome", "races", isStandard: false) }
            };

            _abilities = new Dictionary<string, Campaigns.Model.Attribute>
            {
                { "str", CreateAttribute("str", "abilities", isStandard: true) },
                { "int", CreateAttribute("int", "abilities", isStandard: true) }
            };

            _abilityMods =
                _abilities.Values
                .Select(attrib => CreateAttribute(attrib.Name, "ability-modifiers", isStandard: true))
                .ToDictionary(m => m.Name);

            _skills = new Dictionary<string, Campaigns.Model.Attribute>
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

        private InMemoryEntityRepository<Campaigns.Model.Attribute> _attributesDb;
        private InMemoryEntityRepository<AttributeContribution> _contributionsDb;

        private IDictionary<string, Campaigns.Model.Attribute> _races;
        private IDictionary<string, Campaigns.Model.Attribute> _abilities;
        private IDictionary<string, Campaigns.Model.Attribute> _abilityMods;
        private IDictionary<string, Campaigns.Model.Attribute> _skills;

        public IEnumerable<Campaigns.Model.Attribute> Races { get { return _races.Values; } }
        public IEnumerable<Campaigns.Model.Attribute> Abilities { get { return _abilities.Values; } }
        public IEnumerable<Campaigns.Model.Attribute> AbilityMods { get { return _abilityMods.Values; } }
        public IEnumerable<Campaigns.Model.Attribute> Skills { get { return _skills.Values; } }

        private IEnumerable<Campaigns.Model.Attribute> AllAttributes { get { return Abilities.Concat(AbilityMods).Concat(Races).Concat(Skills); } }

        public Campaigns.Model.Attribute Race(string name) { return _races[name]; }
        public Campaigns.Model.Attribute Ability(string name) { return _abilities[name]; }
        public Campaigns.Model.Attribute AbilityMod(string name) { return _abilityMods[name]; }
        public Campaigns.Model.Attribute Skill(string name) { return _skills[name]; }

        private ISet<Campaigns.Model.Attribute> _contributingAttributes;
        
        public bool IsAttributeContributing(Campaigns.Model.Attribute source)
        {
            return _contributingAttributes.Contains(source);
        }

        public IEnumerable<Campaigns.Model.Attribute> ContributingAttributes { get { return _contributingAttributes; } }
    }
}
