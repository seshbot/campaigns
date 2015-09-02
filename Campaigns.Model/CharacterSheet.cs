using Campaigns.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Model
{
    public class CharacterUpdate : BaseEntity
    {
        public IEnumerable<Model.Attribute> AddedAttributes { get; set; }
        public IEnumerable<Model.Attribute> RemovedAttributes { get; set; }
        public IEnumerable<Model.AttributeContribution> AddedAllocations { get; set; }
        public IEnumerable<Model.AttributeContribution> RemovedAllocations { get; set; }
    }

    public class CharacterSpecification : BaseEntity
    {
        public CharacterSpecification()
        {
            Attributes = new List<Model.Attribute>();
            Allocations = new List<Model.AttributeContribution>();
        }
        public IEnumerable<Model.Attribute> Attributes { get; set; }
        public IEnumerable<Model.AttributeContribution> Allocations { get; set; } // TODO: find a better name
    }

    // TODO: create annotations to allow validation/attribute relationship constraints
    public class CharacterSheet : BaseEntity
    {
        public CharacterSpecification Specification { get; set; }
        public ICollection<AttributeValue> AttributeValues { get; set; }

        public static CharacterUpdate Diff(CharacterSheet first, CharacterSheet second)
        {
            var firstAttribs = first.Specification.Attributes;
            var secondAttribs = second.Specification.Attributes;

            var removedAttribs = firstAttribs.Except(secondAttribs);
            var addedAttribs = secondAttribs.Except(firstAttribs);

            var firstContribs = first.Specification.Allocations;
            var secondContribs = second.Specification.Allocations;

            // counts as removed if attribute is no longer targeted at all
            var contribsRemoved = firstContribs.Where(c1 => !secondContribs.Any(c2 => c1.Target == c2.Target));

            // counts as added if it is new or modified
            var contribsAdded = secondContribs.Where(c2 => !firstContribs.Any(c1 => c1 == c2));
   
            return new CharacterUpdate
            {
                AddedAttributes = addedAttribs,
                RemovedAttributes = removedAttribs,
                AddedAllocations = contribsAdded,
                RemovedAllocations = contribsRemoved,
            };
        }
    }
}
