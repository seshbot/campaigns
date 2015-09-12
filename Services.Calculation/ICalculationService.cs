using Campaigns.Model;
using Serialize.Linq.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Services.Calculation
{
    public interface ICalculationService
    {
        CalculationResult Calculate(ICalculationRules context);
    }

    public class CalculationService : ICalculationService
    {
        public CalculationResult Calculate(ICalculationRules context)
        {
            var calculation = new Calculation(context);
            return calculation.Result;
        }
    }

    //
    // implementation details
    //

    class Calculation
    {
        ICalculationRules _context;
        IDictionary<int, ISet<int>> _pendingAttributeDependencies = new Dictionary<int, ISet<int>>();
        IDictionary<int, AttributeValue> _completedValues = new Dictionary<int, AttributeValue>();
        IDictionary<int, AttributeValue> _pendingValues = new Dictionary<int, AttributeValue>();

        public CalculationResult Result { get; private set; }

        // returns number of dependencies this might free up
        private int SetCompleted(AttributeValue value)
        {
            _completedValues.Add(value.Attribute.Id, value);
            _pendingValues.Remove(value.Attribute.Id);
            _pendingAttributeDependencies.Remove(value.Attribute.Id);
            int dependencies = 0;
            foreach (var kvp in _pendingAttributeDependencies)
            {
                if (kvp.Value.Remove(value.Attribute.Id))
                {
                    dependencies++;
                }
            }

            return dependencies;
        }

        private IEnumerable<AttributeContribution> GetPendingContributionsTo(Campaigns.Model.Attribute target)
        {
            return from contrib in _context.AllContributionsTo(target)
                   let dependency = contrib.Source
                   where null != dependency &&
                         _context.IsAttributeContributing(dependency) &&
                         !_completedValues.ContainsKey(dependency.Id)
                   select contrib;
        }

        private IEnumerable<AttributeContribution> GetRelevantContributionsTo(Campaigns.Model.Attribute target)
        {
            // direct contributions and contributions that are relevant to this calculation
            return _context.AllContributionsTo(target)
                .Where(contrib => null == contrib.Source || _context.IsAttributeContributing(contrib.Source));
        }

        private void AddValueAsPending(AttributeValue value)
        {
            var target = value.Attribute;
            if (_completedValues.ContainsKey(target.Id))
            {
                throw new Exception("calculation engine assertion: adding pending calculation for completed attribute");
            }

            _pendingValues.Add(target.Id, value);

            var targetDependencyIds = GetPendingContributionsTo(target).ToList()
                .Select(c => c.Source.Id);
            
            _pendingAttributeDependencies.Add(target.Id, new HashSet<int>(targetDependencyIds));
        }

        private AttributeValue GetOrAddPendingValue(Campaigns.Model.Attribute target)
        {
            AttributeValue result;
            if (!_pendingValues.TryGetValue(target.Id, out result))
            {
                result = new AttributeValue
                {
                    Attribute = target,
                    Value = 0,
                };
                AddValueAsPending(result);

            }
            return result;
        }

        private void AddContribution(AttributeContribution contribution)
        {
            var value = GetOrAddPendingValue(contribution.Target);
        }

        private IList<Campaigns.Model.Attribute> GetPreparedAttributes()
        {
            return (
                from kvp in _pendingAttributeDependencies
                let attributeId = kvp.Key
                let dependencyIds = kvp.Value
                where dependencyIds.Count == 0
                select _pendingValues[attributeId].Attribute
            ).ToList();
        }

        private int CompletePendingCalculation(Campaigns.Model.Attribute target)
        {
            var value = GetOrAddPendingValue(target);
            var contribsToTarget = GetRelevantContributionsTo(target).ToList();
            foreach (var contribution in contribsToTarget)
            {
                var source = contribution.Source;
                var sourceValue = null == source ? 0 : _completedValues[source.Id].Value;
                value.Value += contribution.Formula(sourceValue);
            }
            return SetCompleted(value);
        }

        public Calculation(ICalculationRules context)
        {
            _context = context;

            foreach (var attribute in _context.ContributingAttributes)
            {
                var value = GetOrAddPendingValue(attribute);
            }

            var prepared = GetPreparedAttributes();
            while (prepared.Count > 0)
            {
                foreach (var attribute in prepared)
                {
                    // this will remove the attribute from the 'pending' queue
                    var removed = CompletePendingCalculation(attribute);

                    foreach (var contribution in _context.ContributionsBy(attribute))
                    {
                        AddContribution(contribution);
                    }
                }
                prepared = GetPreparedAttributes();
            }

            if (_pendingValues.Count > 0)
            {
                throw new Exception("not enough info to calculate all attributes");
            }

            Result = new CalculationResult { AttributeValues = _completedValues.Values };
        }
    }
}
