using Campaigns.Core.Data;
using Campaigns.Model;
using Services.Calculation;
using Services.Rules.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Services.Rules
{
    // TODO: get rid of this class, nothing a simple repository cant handle
    public class CalculationRules : ICalculationRules
    {
        IEntityStore<Campaigns.Model.Attribute> _attributesDb;
        IEntityStore<AttributeContribution> _contributionsDb;

        IEnumerable<AttributeAllocation> _allocations;

        InMemoryRules _memDb = new InMemoryRules();

        Expression<Func<Campaigns.Model.Attribute, bool>> IsStandard = a => a.IsStandard;

        Campaigns.Model.Attribute GetAttributeById(int id)
        {
            return _attributesDb.GetById(id)
                ?? _allocations.FirstOrDefault(a => a.Attribute.Id == id).Attribute;
        }

        public CalculationRules(
            IEntityStore<Campaigns.Model.Attribute> attributesDb,
            IEntityStore<AttributeContribution> contributionsDb)
            : this(attributesDb, contributionsDb, new List<AttributeAllocation>())
        {
        }


        public CalculationRules(
            IEntityStore<Campaigns.Model.Attribute> attributesDb,
            IEntityStore<AttributeContribution> contributionsDb,
            IEnumerable<AttributeAllocation> allocations)
        {
            if (null == attributesDb) throw new ArgumentNullException("attributesDb");
            if (null == contributionsDb) throw new ArgumentNullException("contributionsDb");
            if (null == allocations) throw new ArgumentNullException("allocations");

            _attributesDb = attributesDb;
            _contributionsDb = contributionsDb;
            _allocations = allocations;

            foreach (var attrib in _attributesDb.AsQueryable.Where(IsStandard))
            {
                _memDb.AddAttribute(attrib);
            }
            
            foreach (var alloc in _allocations)
            {
                if (!_memDb.Attributes.Contains(alloc.Attribute))
                {
                    _memDb.AddAttribute(alloc.Attribute);
                }

                if (alloc.Value.HasValue)
                {
                    var contribution =
                        AttributeContributions.ConstantContributionTo(
                            null, alloc.Attribute, alloc.Value.Value);

                    _memDb.AddContribution(contribution);
                }
            }
        }

        public void AddContribution(AttributeContribution contribution)
        {
            if (null != contribution.Source && !_memDb.Attributes.Any(a => a.Id == contribution.Source.Id))
            {
                _memDb.AddAttribute(contribution.Source);
            }

            if (!_memDb.Attributes.Any(a => a.Id == contribution.Target.Id))
            {
                _memDb.AddAttribute(contribution.Target);
            }

            _memDb.AddContribution(contribution);
        }

        public IEnumerable<Campaigns.Model.Attribute> ContributingAttributes { get { return _memDb.Attributes; } }

        public IEnumerable<AttributeContribution> AllContributionsFrom(int sourceId)
        {
            var attrib = GetAttributeById(sourceId);
            return AllContributionsFrom(attrib);
        }

        public IEnumerable<AttributeContribution> AllContributionsFrom(Campaigns.Model.Attribute source)
        {
            return _memDb.ContributionsFrom(source).Concat(_contributionsDb.ContributionsFrom(source));
        }

        public IEnumerable<AttributeContribution> AllContributionsTo(int targetId)
        {
            var attrib = GetAttributeById(targetId);
            return AllContributionsTo(attrib);
        }

        public IEnumerable<AttributeContribution> AllContributionsTo(Campaigns.Model.Attribute target)
        {
            return _memDb.ContributionsTo(target).Concat(_contributionsDb.ContributionsTo(target));
        }

        public bool IsAttributeContributing(Campaigns.Model.Attribute source)
        {
            return _memDb.Attributes.Any(a => a.Id == source.Id);
        }
    }
}
