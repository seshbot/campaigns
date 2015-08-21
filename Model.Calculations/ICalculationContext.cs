using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Calculations
{
    public interface ICalculationContext
    {
        bool IsAttributeContributing(Attribute source);
        IEnumerable<AttributeValue> ContributingAttributes { get; }
        IEnumerable<AttributeContribution> AllContributionsFor(Attribute target);
        IEnumerable<AttributeContribution> AllContributionsBy(Attribute source);
    }

    public static class CalculationContextExtensions
    {
        public static ICollection<AttributeContribution> ContributionsFor(this ICalculationContext ctx, Attribute target)
        {
            return ctx.AllContributionsFor(target)
                .Where(c => ctx.IsAttributeContributing(c.Source))
                .ToList();
        }

        public static ICollection<AttributeContribution> ContributionsBy(this ICalculationContext ctx, Attribute source)
        {
            return ctx.AllContributionsBy(source)
                .Where(c => ctx.IsAttributeContributing(c.Target))
                .ToList();
        }
    }

    public class InMemoryRules
    {
        #region Public Interface

        public ICollection<AttributeContribution> ContributionsBy(Attribute source)
        {
            if (null == source)
                throw new ArgumentNullException("source");
            return _contributionsByAttributeId[source.Id];
        }

        public ICollection<AttributeContribution> ContributionsFor(Attribute target)
        {
            if (null == target)
                throw new ArgumentNullException("target");
            return _contributionsForAttributeId[target.Id];
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

        public Attribute CreateAttribute(string name, string category, bool isStandard)
        {
            var attrib = new Attribute { Id = AllocId(), Name = name, Category = category, IsStandard = isStandard };
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

        int _nextId = 1;
        int AllocId() { return _nextId++; }

        IDictionary<string, Attribute> _attributes = new Dictionary<string, Attribute>();
        IDictionary<int, IList<AttributeContribution>> _contributionsByAttributeId = new Dictionary<int, IList<AttributeContribution>>();
        IDictionary<int, IList<AttributeContribution>> _contributionsForAttributeId = new Dictionary<int, IList<AttributeContribution>>();

        #endregion
    }
}
