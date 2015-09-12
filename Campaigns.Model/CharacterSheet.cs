using Campaigns.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Model
{
    // TODO: create annotations to allow validation/attribute relationship constraints
    public class CharacterSheet : BaseEntity
    {
        public int CharacterId { get; set; }
        public virtual ICollection<AttributeAllocation> AttributeAllocations { get; set; }
        public virtual ICollection<AttributeValue> AttributeValues { get; set; }

        public static CharacterUpdate Diff(CharacterSheet first, CharacterSheet second)
        {
            var firstAttribs = first.AttributeAllocations.Select(a => a.Attribute).Distinct();
            var secondAttribs = second.AttributeAllocations.Select(a => a.Attribute).Distinct();

            var removedAttribs = firstAttribs.Except(secondAttribs);
            var addedAttribs = secondAttribs.Except(firstAttribs);

            var removedAllocations = removedAttribs.Select(attrib => first.AttributeAllocations.First(a => a.Attribute == attrib));
            var addedAllocations = addedAttribs.Select(attrib => second.AttributeAllocations.First(a => a.Attribute == attrib));

            // updated if the allocation is unique to second but the attribute is present in the first
            var updatedAllocations = second.AttributeAllocations.Except(first.AttributeAllocations)
                .Where(a => firstAttribs.Contains(a.Attribute));

            return new CharacterUpdate
            {
                AddedOrUpdatedAllocations = addedAllocations.Concat(updatedAllocations).ToList(),
                RemovedAllocations = removedAllocations.ToList(),
            };
        }
    }
}
