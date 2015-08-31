using Campaigns.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Model
{
    public class Attribute : BaseEntity
    {
        public string Name { get; set; }
        public string LongName { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public string Category { get; set; }

        /// indicates that this attribute should be included in all calculations
        public bool IsStandard { get; set; }

        // TODO
        // Type : Number, Text, Flag, Enumeration

        public override string ToString()
        {
            return string.Format("{0}: {1} ({2})", Id, Name, Category);
        }
    }
}
