using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Calculations
{
    public interface ICalculationContext
    {
        ICollection<AttributeValue> InitialValues { get; }
        ICollection<AttributeContribution> ContributionsFor(Attribute target);
        ICollection<AttributeContribution> ContributionsBy(Attribute source);
    }

    public class InMemoryRules
    {
        #region Public Interface

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

        public void AddContribution(AttributeContribution contrib)
        {
            GetContributionsByAttribute(contrib.Source).Add(contrib);
            GetContributionsForAttribute(contrib.Target).Add(contrib);
        }

        public Attribute CreateAttribute(string name, string category)
        {
            var attrib = new Attribute { Id = AllocId(), Name = name, Category = category };
            AddAttribute(attrib);
            return attrib;
        }

        public void AddAttribute(Attribute attrib)
        {
            if (_attributes.Values.Any(a => a.Id == attrib.Id))
            {
                throw new Exception(
                    String.Format("Attribute calculation assertion failure: attribute already exists with ID {0} ({1})", attrib.Id, attrib.Name));
            }
            // var attrib = new Attribute { Id = AllocId(), Name = name, Category = category };
            _attributes.Add(AttributeKey(attrib.Name, attrib.Category), attrib);
            GetContributionsByAttribute(attrib);
            GetContributionsForAttribute(attrib);
        }

        #endregion

        #region Private Implementation

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

        int _nextId = 1;
        int AllocId() { return _nextId++; }

        IDictionary<string, Attribute> _attributes = new Dictionary<string, Attribute>();
        IDictionary<Attribute, IList<AttributeContribution>> _contributionsByAttribute = new Dictionary<Attribute, IList<AttributeContribution>>();
        IDictionary<Attribute, IList<AttributeContribution>> _contributionsForAttribute = new Dictionary<Attribute, IList<AttributeContribution>>();

        #endregion
    }
}
