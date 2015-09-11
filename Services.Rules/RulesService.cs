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
        private IEntityRepository<Campaigns.Model.Character> _charactersDb;

        private ICalculationService _calculationService;

        public RulesService(
            IEntityRepository<Campaigns.Model.Attribute> attributesDb,
            IEntityRepository<Campaigns.Model.AttributeContribution> contributionsDb,
            IEntityRepository<Campaigns.Model.CharacterSheet> characterSheetsDb,
            IEntityRepository<Campaigns.Model.Character> charactersDb)
        {
            _attributesDb = attributesDb;
            _contributionsDb = contributionsDb;
            _characterSheetsDb = characterSheetsDb;
            _charactersDb = charactersDb;

            //_characterSheetStore = characterSheetStore;
            _calculationService = new CalculationService();
        }

        public void DeleteCharacterById(int id)
        {
            var character = _charactersDb.GetById(id);
            _charactersDb.Remove(character);

            //CharacterSheet characterSheet = _charDb.CharacterSheets.Find(id);
            //foreach (var o in characterSheet.AbilityAllocations.ToList())
            //    _charDb.Entry(o).State = EntityState.Deleted;
            //foreach (var o in characterSheet.SkillAllocations.ToList())
            //    _charDb.Entry(o).State = EntityState.Deleted;
            //_charDb.CharacterSheets.Remove(characterSheet);
            //_charDb.SaveChanges();
        }

        // TODO: this should create default archetype
        public Character CreateCharacter(string name, string description)
        {
            return CreateCharacter(name, description, new List<AttributeAllocation>());
        }

        // TODO: create from archetype
        public Character CreateCharacter(string name, string description, IEnumerable<AttributeAllocation> allocations)
        {
            var rules = new CalculationRules(_attributesDb, _contributionsDb, allocations);
            var results = _calculationService.Calculate(rules);

            var characterSheet = new CharacterSheet
            {
                AttributeAllocations = allocations.ToList(),
                AttributeValues = results.AttributeValues,
            };

            var character = new Character
            {
                Name = name,
                Description = description,
                Sheet = characterSheet
            };

            _charactersDb.Add(character);

            return character;
        }

        public IQueryable<Character> GetCharacters()
        {
            return _charactersDb.AsQueryableIncluding("Sheet.AttributeValues.Contributions");
        }

        public Character GetCharacter(int id)
        {
            return _charactersDb.GetByIdIncluding(id, "Sheet.AttributeValues.Contributions");
        }

        private static IEnumerable<T> EmptyOnNull<T>(IEnumerable<T> xs)
        {
            return xs ?? new List<T>();
        }

        private void validate(CharacterUpdate update)
        {
            var attributeCounts = new[]
            {
                update.AddedOrUpdatedAllocations?.Select(a => a.Attribute),
                update.RemovedAllocations?.Select(a => a.Attribute),
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

        private void validate(IEnumerable<AttributeAllocation> allocations)
        {
            var attributeCounts = allocations
                .Select(a => a.Attribute)
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

        private IEnumerable<T> NullSafe<T>(IEnumerable<T> collection)
        {
            return collection ?? new List<T>();
        }

        private IEnumerable<AttributeAllocation> mergeUpdate(IEnumerable<AttributeAllocation> orig, CharacterUpdate update)
        {
            validate(update);

            var overwrittenAttribs = NullSafe(update.AddedOrUpdatedAllocations).Select(a => a.Attribute).Distinct().ToList();
            var overwritten = orig.Where(a => overwrittenAttribs.Contains(a.Attribute));

            var updated = orig;
            updated = updated.Except(NullSafe(update.RemovedAllocations));
            updated = updated.Except(overwritten);
            updated = updated.Union(NullSafe(update.AddedOrUpdatedAllocations));

            validate(updated);
            
            return updated;
        }

        public Character UpdateCharacter(Character character, CharacterUpdate update)
        {
            var updatedAllocations = mergeUpdate(character.Sheet.AttributeAllocations, update);
            var rules = new CalculationRules(_attributesDb, _contributionsDb, updatedAllocations);
            var results = _calculationService.Calculate(rules);

            var characterSheet = new CharacterSheet
            {
                AttributeAllocations = updatedAllocations.ToList(),
                AttributeValues = results.AttributeValues,
            };

            _characterSheetsDb.Add(characterSheet);

            character.Sheet = characterSheet;
            _charactersDb.Update(character);

            return character;
        }

        public IQueryable<Campaigns.Model.Attribute> GetAllAttributes()
        {
            return _attributesDb.AsQueryable;
        }

        public IQueryable<Campaigns.Model.Attribute> GetAttributesByCategory(string category)
        {
            return _attributesDb.GetAttributesInCategory(category);
        }

        public Campaigns.Model.Attribute GetAttributeById(int id)
        {
            return _attributesDb.GetById(id);
        }
    }
}
