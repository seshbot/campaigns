using Campaigns.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Rules
{
    public class CharacterArchetype : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public CharacterSpecification InitialAllocations { get; set; }
    }
}
