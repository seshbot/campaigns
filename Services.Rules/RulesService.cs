using Campaigns.Core.Data;
using Campaigns.Model;
using Services.Calculation;
using Services.Rules.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Services.Rules
{
    public class RulesService : IRulesService
    {
        private IEntityRepository<Campaigns.Model.Attribute> _attributesDb;
        private IEntityRepository<Campaigns.Model.AttributeContribution> _contributionsDb;
        private IEntityRepository<Campaigns.Model.CharacterSheet> _characterSheetsDb;

        private ICalculationService _calculationService;

        public RulesService(
            IEntityRepository<Campaigns.Model.Attribute> attributesDb,
            IEntityRepository<Campaigns.Model.AttributeContribution> contributionsDb,
            IEntityRepository<Campaigns.Model.CharacterSheet> characterSheetsDb)
        {
            _attributesDb = attributesDb;
            _contributionsDb = contributionsDb;
            _characterSheetsDb = characterSheetsDb;

            //_characterSheetStore = characterSheetStore;
            _calculationService = new CalculationService();
        }

        // TODO: this should create default archetype
        public CharacterSheet CreateCharacterSheet()
        {
            return CreateCharacterSheet(new CharacterSpecification());
        }

        // TODO: create from archetype
        public CharacterSheet CreateCharacterSheet(CharacterSpecification specification)
        {
            var rules = new CalculationRules(_attributesDb, _contributionsDb, specification);
            var results = _calculationService.Calculate(rules);

            var characterSheet = new CharacterSheet
            {
                Specification = specification,
                AttributeValues = results.AttributeValues,
            };

            _characterSheetsDb.Add(characterSheet);

            return characterSheet;
        }

        private static IEnumerable<T> EmptyOnNull<T>(IEnumerable<T> xs)
        {
            return xs ?? new List<T>();
        }

        private void validate(CharacterUpdate update)
        {
            var attributeCounts = new[]
            {
                update.AddedAttributes,
                update.RemovedAttributes,
                update.AddedAllocations?.Select(a => a.Target),
                update.RemovedAllocations?.Select(a => a.Target),
            }
            .SelectMany(g => EmptyOnNull(g)) // flatten the list<list<attrib>> -> list<attrib>
            .GroupBy(a => a) // group same attributes together
            .ToDictionary(g => g.Key, g => g.Count()); // create dictionary: attrib -> count 

            var multipleAttributeViolations = attributeCounts
                .Where(kvp => kvp.Value > 1);

            if (multipleAttributeViolations.Count() > 0)
            {
                var violationStrings =
                    multipleAttributeViolations.Select(kvp => string.Format("{0}:{1}", kvp.Key.Name, kvp.Key.Category));

                var violationMessage = string.Join(", ", violationStrings);

                throw new Exception(string.Format("update contains multiple references to {0} attributes: {1}",
                    multipleAttributeViolations.Count(), violationMessage));
            }

            //var addedAndRemovedAttributes = update.AddedAttributes.Intersect(update.RemovedAttributes);
            //if (addedAndRemovedAttributes.Count() > 0)
            //{
            //    throw new Exception(string.Format("update contains {0} attributes that were both added and removed: {1}", 
            //        addedAndRemovedAttributes.Count(),
            //        string.Join(", ", addedAndRemovedAttributes.Select(a => a.Name))));
            //}

            ////
            //// find any allocations that are both added and removed or updated
            ////

            //var duplicateGroups = new[]
            //{
            //    new {
            //        OverlapType = "added and removed",
            //        Overlap = update.AddedAllocations.Intersect(update.RemovedAllocations)
            //    },
            //    new {
            //        OverlapType = "updated and added",
            //        Overlap = update.UpdatedAllocations.Intersect(update.AddedAllocations)
            //    },
            //    new {
            //        OverlapType = "updated and removed",
            //        Overlap = update.UpdatedAllocations.Intersect(update.RemovedAllocations)
            //    },
            //};

            //foreach (var duplicates in duplicateGroups)
            //{
            //    if (duplicates.Overlap.Count() > 0)
            //    {
            //        var overlapsString = string.Join(", ", duplicates.Overlap.Select(a => a.Target.Name));
            //        throw new Exception(string.Format("update contains {0} allocations that were both {1}: {2}",
            //            duplicates.Overlap.Count(), duplicates.OverlapType, overlapsString));
            //    }
            //}
        }

        private void validate(CharacterSpecification specification)
        {
            var attributeCounts = new[]
            {
                specification.Attributes,
                specification.Allocations?.Select(a => a.Target),
            }
            .SelectMany(g => EmptyOnNull(g)) // flatten the list<list<attrib>> -> list<attrib>
            .GroupBy(a => a) // group same attributes together
            .ToDictionary(g => g.Key, g => g.Count()); // create dictionary: attrib -> count 

            var multipleAttributeViolations = attributeCounts
                .Where(kvp => kvp.Value > 1);

            if (multipleAttributeViolations.Count() > 0)
            {
                var violationStrings =
                    multipleAttributeViolations.Select(kvp => string.Format("{0}:{1}", kvp.Key.Name, kvp.Key.Category));

                var violationMessage = string.Join(", ", violationStrings);

                throw new Exception(string.Format("update contains multiple references to {0} attributes: {1}",
                    multipleAttributeViolations.Count(), violationMessage));
            }
        }

        private CharacterSpecification mergeUpdate(CharacterSpecification orig, CharacterUpdate update)
        {
            validate(update);

            var attributes = orig.Attributes;
            if (null != update.RemovedAttributes) attributes = attributes.Except(update.RemovedAttributes);
            if (null != update.AddedAttributes) attributes = attributes.Union(update.AddedAttributes);

            var allocations = orig.Allocations;
            if (null != update.RemovedAllocations) allocations = allocations.Except(update.RemovedAllocations);
            if (null != update.AddedAllocations) allocations =
                from alloc in allocations
                let updateAlloc = update.AddedAllocations.FirstOrDefault(updAlloc => updAlloc.Target == alloc.Target)
                select updateAlloc ?? alloc;

            var updated = new CharacterSpecification
            {
                Attributes = attributes,
                Allocations = allocations,
            };

            validate(updated);
            
            return updated;
        }

        public CharacterSheet UpdateCharacterSheet(CharacterSheet orig, CharacterUpdate update)
        {
            var updatedAllocations = mergeUpdate(orig.Specification, update);
            var rules = new CalculationRules(_attributesDb, _contributionsDb, updatedAllocations);
            var results = _calculationService.Calculate(rules);

            var characterSheet = new CharacterSheet
            {
                Specification = updatedAllocations,
                AttributeValues = results.AttributeValues,
            };

            _characterSheetsDb.Add(characterSheet);

            return characterSheet;
        }

        //public CharacterSheet EnsureValid(CharacterSheet characterSheet)
        //{
        //    characterSheet.Description = characterSheet.Description ?? new CharacterDescription
        //    {
        //        Name = "",
        //        Text = ""
        //    };

        //    // TODO: this shouldnt reference the rules directly
        //    var levelInfo = DnD5.LevelInfo.FindBestFit(characterSheet.Xp, characterSheet.Level);
        //    characterSheet.Xp = levelInfo.XP;
        //    characterSheet.Level = levelInfo.Level;

        //    AddStandardAttributesTo(characterSheet);

        //    return characterSheet;
        //}
    }
}
