using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Calculations
{
    public class AttributeValue
    {
        public Attribute Attribute { get; set; }
        public int Value { get; set; }
        public ICollection<AttributeContribution> Contributions { get; set; }
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
        IDictionary<Attribute, AttributeValue> _completedAttributes = new Dictionary<Attribute, AttributeValue>();
        IDictionary<Attribute, AttributeValue> _pendingAttributes = new Dictionary<Attribute, AttributeValue>();
        IDictionary<Attribute, ISet<Attribute>> _pendingAttributeDependencies = new Dictionary<Attribute, ISet<Attribute>>();

        public CalculationResult Result { get; private set; }

        // returns number of dependencies this might free up
        private int SetCompleted(AttributeValue value)
        {
            _completedAttributes.Add(value.Attribute, value);
            _pendingAttributes.Remove(value.Attribute);
            _pendingAttributeDependencies.Remove(value.Attribute);
            int dependencies = 0;
            foreach (var kvp in _pendingAttributeDependencies)
            {
                if (kvp.Value.Remove(value.Attribute))
                {
                    dependencies++;
                }
            }

            return dependencies;
        }

        private void AddValueAsPending(AttributeValue value)
        {
            var target = value.Attribute;
            if (_completedAttributes.ContainsKey(target))
            {
                throw new Exception("calculation engine assertion: adding pending calculation for completed attribute");
            }

            _pendingAttributes.Add(target, value);

            var targetDependencies =
                from contrib in _context.ContributionsFor(target)
                let dependency = contrib.Source
                where !_completedAttributes.ContainsKey(dependency)
                select dependency;

            _pendingAttributeDependencies.Add(target, new HashSet<Attribute>(targetDependencies));
        }

        private AttributeValue GetOrAddPendingValue(Attribute target)
        {
            AttributeValue result;
            if (!_pendingAttributes.TryGetValue(target, out result))
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

        private IList<Attribute> GetPrepared()
        {
            return (
                from kvp in _pendingAttributeDependencies
                let attribute = kvp.Key
                let dependencies = kvp.Value
                where dependencies.Count == 0
                select attribute
            ).ToList();
        }

        private void CompletePendingCalculation(Attribute target)
        {
            var value = GetOrAddPendingValue(target);
            if (null != value.Contributions)
            {
                foreach (var contribution in value.Contributions)
                {
                    var source = contribution.Source;
                    if (target != contribution.Target)
                        throw new Exception("calculation engine assertion: unexpected calculation target");

                    var func = contribution.Formula.Compile();

                    var sourceValue = _completedAttributes[source].Value;
                    value.Value += func(sourceValue);
                }
            }
            SetCompleted(value);
        }

        private AttributeValue RecursivelyAddPending(Attribute attribute, ISet<Attribute> added = null)
        {
            if (null == added)
            {
                added = new HashSet<Attribute>();
            }
            else
            {
                if (added.Contains(attribute))
                {
                    throw new Exception("calculation engine assertion: dependency loop detected while adding attribute hierarchy");
                }
            }

            var result = GetOrAddPendingValue(attribute);
            added.Add(attribute);
            foreach (var source in _context.ContributionsFor(attribute).Select(c => c.Source))
            {
                RecursivelyAddPending(source, added);
            }

            return result;
        }

        public Calculation(ICalculationContext context)
        {
            _context = context;
            foreach (var val in _context.ContributingAttributes)
            {
                var value = RecursivelyAddPending(val.Attribute);
                if (val.Contributions != null && val.Contributions.Count != 0)
                {
                    throw new Exception("calculation engine assertion: initial values cannot have contributions (or should they?)");
                }
                value.Value = val.Value;
            }

            var prepared = GetPrepared();
            while (prepared.Count > 0)
            {
                foreach (var attribute in prepared)
                {
                    // this will remove the attribute from the 'pending' queue
                    CompletePendingCalculation(attribute);

                    foreach (var contribution in _context.ContributionsBy(attribute))
                    {
                        AddContribution(contribution);
                    }
                }
                prepared = GetPrepared();
            }

            if (_pendingAttributes.Count > 0)
            {
                throw new Exception("not enough info to calculate all attributes");
            }

            Result = new CalculationResult { AttributeValues = _completedAttributes.Values };
        }
    }
}
