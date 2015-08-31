using Campaigns.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Model
{
    public class AttributeCategory : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AllowMultiple { get; set; }
    }
}
