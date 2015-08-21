using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Model.Calculations
{
    public class Attribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        /// indicates that this attribute should be included in all calculations
        public bool IsStandard { get; set; }
    }

    public class AttributeContribution
    {
        [Required]
        public virtual Attribute Source { get; set; }

        [Required]
        public virtual Attribute Target { get; set; }

        [Required]
        public Expression<Func<int, int>> Formula { get; set; }
    }
}
