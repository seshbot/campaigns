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
}
