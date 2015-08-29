using Campaigns.Core.Data;
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
        IEntityStore<Calculation.Attribute> _attributesDb;
        IEntityStore<AttributeContribution> _contributionsDb;

        CharacterSpecification _calculationParams;

        InMemoryRules _memDb = new InMemoryRules();

        Expression<Func<Calculation.Attribute, bool>> IsStandard = a => a.IsStandard;

        Calculation.Attribute GetAttributeById(int id)
        {
            return _attributesDb.GetById(id)
                ?? _calculationParams.Attributes.FirstOrDefault(a => a.Id == id);
        }

        public CalculationRules(
            IEntityStore<Calculation.Attribute> attributesDb,
            IEntityStore<AttributeContribution> contributionsDb)
            : this(attributesDb, contributionsDb, null)
        {
        }


        public CalculationRules(
            IEntityStore<Calculation.Attribute> attributesDb,
            IEntityStore<AttributeContribution> contributionsDb,
            CharacterSpecification calculationParams)
        {
            _attributesDb = attributesDb;
            _contributionsDb = contributionsDb;
            _calculationParams = calculationParams;

            foreach (var attrib in _attributesDb.EntityTable.Where(IsStandard))
            {
                _memDb.AddAttribute(attrib);
            }

            if (null != calculationParams)
            {
                foreach (var attrib in _calculationParams.Attributes)
                {
                    _memDb.AddAttribute(attrib);
                }

                foreach (var contrib in _calculationParams.Allocations)
                {
                    AddContribution(contrib);
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

        public IEnumerable<Calculation.Attribute> ContributingAttributes { get { return _memDb.Attributes; } }

        public IEnumerable<AttributeContribution> AllContributionsFrom(int sourceId)
        {
            var attrib = GetAttributeById(sourceId);
            return AllContributionsFrom(attrib);
        }

        public IEnumerable<AttributeContribution> AllContributionsFrom(Calculation.Attribute source)
        {
            return _memDb.ContributionsFrom(source).Concat(_contributionsDb.ContributionsFrom(source));
        }

        public IEnumerable<AttributeContribution> AllContributionsTo(int targetId)
        {
            var attrib = GetAttributeById(targetId);
            return AllContributionsTo(attrib);
        }

        public IEnumerable<AttributeContribution> AllContributionsTo(Calculation.Attribute target)
        {
            return _memDb.ContributionsTo(target).Concat(_contributionsDb.ContributionsTo(target));
        }

        public bool IsAttributeContributing(Calculation.Attribute source)
        {
            return _memDb.Attributes.Any(a => a.Id == source.Id);
        }
    }
}
