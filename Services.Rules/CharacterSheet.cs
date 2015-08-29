using Campaigns.Core;
using Services.Calculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Rules
{
    public class CharacterUpdate : BaseEntity
    {
        public IEnumerable<Calculation.Attribute> AddedAttributes { get; set; }
        public IEnumerable<Calculation.Attribute> RemovedAttributes { get; set; }
        public IEnumerable<Calculation.AttributeContribution> AddedAllocations { get; set; }
        public IEnumerable<Calculation.AttributeContribution> RemovedAllocations { get; set; }
    }

    public class CharacterSpecification : BaseEntity
    {
        public CharacterSpecification()
        {
            Attributes = new List<Calculation.Attribute>();
            Allocations = new List<Calculation.AttributeContribution>();
        }
        public IEnumerable<Calculation.Attribute> Attributes { get; set; }
        public IEnumerable<Calculation.AttributeContribution> Allocations { get; set; } // TODO: find a better name
    }

    // TODO: create annotations to allow validation/attribute relationship constraints
    public class CharacterSheet : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription
        {
            get
            {
                return (Description ?? "").Split(new[] { '.', '\n', '<' }).FirstOrDefault();
            }
        }
        
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
