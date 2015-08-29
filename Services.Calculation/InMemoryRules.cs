using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Calculation
{
    public class InMemoryRules
    {
        #region Public Interface

        public ICollection<AttributeContribution> ContributionsFrom(Attribute source)
        {
            if (null == source)
                throw new ArgumentNullException("source");
            return _contributionsByAttributeId[source.Id];
        }

        public ICollection<AttributeContribution> ContributionsTo(Attribute target)
        {
            if (null == target)
                throw new ArgumentNullException("target");
            return _contributionsForAttributeId[target.Id];
        }

        public ICollection<AttributeContribution> ContributionsFrom(int sourceId)
        {
            return _contributionsByAttributeId[sourceId];
        }

        public ICollection<AttributeContribution> ContributionsTo(int targetId)
        {
            return _contributionsForAttributeId[targetId];
        }

        public Attribute GetAttribute(string name, string category)
        {
            return _attributes[AttributeKey(name, category)];
        }

        public IEnumerable<Attribute> Attributes { get { return _attributes.Values; } }

        public void AddContribution(AttributeContribution contrib)
        {
            if (null == contrib.Target)
                throw new Exception("attribute contribution target cannot be null");
            if (null != contrib.Source)
                GetContributionsByAttribute(contrib.Source).Add(contrib);
            GetContributionsForAttribute(contrib.Target).Add(contrib);
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
            if (null == attrib)
                throw new ArgumentNullException("attrib");
            IList<AttributeContribution> contribs;
            if (!_contributionsByAttributeId.TryGetValue(attrib.Id, out contribs))
            {
                contribs = new List<AttributeContribution>();
                _contributionsByAttributeId.Add(attrib.Id, contribs);
            }
            return contribs;
        }

        private IList<AttributeContribution> GetContributionsForAttribute(Attribute attrib)
        {
            if (null == attrib)
                throw new ArgumentNullException("attrib");
            IList<AttributeContribution> contribs;
            if (!_contributionsForAttributeId.TryGetValue(attrib.Id, out contribs))
            {
                contribs = new List<AttributeContribution>();
                _contributionsForAttributeId.Add(attrib.Id, contribs);
            }
            return contribs;
        }

        IDictionary<string, Attribute> _attributes = new Dictionary<string, Attribute>();
        IDictionary<int, IList<AttributeContribution>> _contributionsByAttributeId = new Dictionary<int, IList<AttributeContribution>>();
        IDictionary<int, IList<AttributeContribution>> _contributionsForAttributeId = new Dictionary<int, IList<AttributeContribution>>();

        #endregion
    }
}
