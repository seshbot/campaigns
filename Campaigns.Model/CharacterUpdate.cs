using Campaigns.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Model
{
    public class CharacterUpdate : BaseEntity
    {
        public virtual ICollection<Model.AttributeAllocation> RemovedAllocations { get; set; }
        public virtual ICollection<Model.AttributeAllocation> AddedOrUpdatedAllocations { get; set; }
    }
}
