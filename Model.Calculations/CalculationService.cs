using Serialize.Linq.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Model.Calculations
{
    public class AttributeValue
    {
        public Attribute Attribute { get; set; }
        public int Value { get; set; }
        public ICollection<AttributeContribution> Contributions { get; set; }
        public override string ToString()
        {
            return string.Format("{0}:{1} ({2})", Attribute, Value, Contributions.Count);
        }
    }
    
    public class CalculationResult
    {
        public ICollection<AttributeValue> AttributeValues { get; set; }
        public IEnumerable<AttributeValue> AttributeValuesForCategory(string category)
        {
            return AttributeValues.Where(val => 0 == string.Compare(val.Attribute.Category, category, true));
        }
        public AttributeValue AttributeValue(string name, string category)
        {
            return AttributeValues
                .FirstOrDefault(val => 
                    0 == string.Compare(val.Attribute.Category, category, true) &&
                    0 == string.Compare(val.Attribute.Name, name, true));
        }
    }

    public interface ICalculationService
    {
        CalculationResult Calculate(ICalculationContext context);
    }

    public class CalculationService : ICalculationService
    {
        public CalculationResult Calculate(ICalculationContext context)
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
        ICalculationContext _context;
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

        private void AddValueAsPending(AttributeValue value)
        {
            var target = value.Attribute;
            if (_completedValues.ContainsKey(target.Id))
            {
                throw new Exception("calculation engine assertion: adding pending calculation for completed attribute");
            }

            _pendingValues.Add(target.Id, value);

            var contributionsForTarget =
                (from contrib in _context.AllContributionsFor(target)
                 let dependency = contrib.Source
                 where _context.IsAttributeContributing(dependency) &&
                      !_completedValues.ContainsKey(dependency.Id)
                 select contrib
                ).ToList();

            var targetDependencyIds =
                from contrib in contributionsForTarget
                select contrib.Source.Id;
            
            _pendingAttributeDependencies.Add(target.Id, new HashSet<int>(targetDependencyIds));
        }

        private AttributeValue GetOrAddPendingValue(Attribute target)
        {
            AttributeValue result;
            if (!_pendingValues.TryGetValue(target.Id, out result))
            {
                result = new AttributeValue
                {
                    Attribute = target,
                    Value = 0,
                    Contributions = new List<AttributeContribution>()
                };
                AddValueAsPending(result);

            }
            return result;
        }

        private void AddContribution(AttributeContribution contribution)
        {
            var value = GetOrAddPendingValue(contribution.Target);
            value.Contributions.Add(contribution);
        }

        private IList<Attribute> GetPreparedAttributes()
        {
            return (
                from kvp in _pendingAttributeDependencies
                let attributeId = kvp.Key
                let dependencyIds = kvp.Value
                where dependencyIds.Count == 0
                select _pendingValues[attributeId].Attribute
            ).ToList();
        }

        private int CompletePendingCalculation(Attribute target)
        {
            var value = GetOrAddPendingValue(target);
            if (null != value.Contributions)
            {
                foreach (var contribution in value.Contributions)
                {
                    var source = contribution.Source;
                    if (target.Id != contribution.Target.Id)
                        throw new Exception("calculation engine assertion: unexpected calculation target");

                    var sourceValue = _completedValues[source.Id].Value;
                    value.Value += contribution.Formula(sourceValue);
                }
            }
            return SetCompleted(value);
        }

        private AttributeValue RecursivelyAddPending(Attribute attribute)
        {
            if (_pendingValues.ContainsKey(attribute.Id))
            {
                return null;
                throw new Exception("calculation engine assertion: dependency loop detected while adding attribute hierarchy");
            }

            var result = GetOrAddPendingValue(attribute);
            foreach (var source in _context.ContributionsFor(attribute).Select(c => c.Source))
            {
                RecursivelyAddPending(source);
            }

            return result;
        }

        public Calculation(ICalculationContext context)
        {
            _context = context;

            foreach (var val in _context.ContributingAttributes)
            {
                var attribute = val.Attribute;
                var value = GetOrAddPendingValue(attribute);
                if (val.Contributions != null && val.Contributions.Count != 0)
                {
                    throw new Exception("calculation engine assertion: initial values cannot have contributions (or should they?)");
                }
                value.Value = val.Value;
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
