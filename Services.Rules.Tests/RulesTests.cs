using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Campaigns.Core.Testing;
using Services.Calculation;
using System.Collections.Generic;
using Campaigns.Model;

namespace Services.Rules.Tests
{
    /// <summary>
    /// These tests all assume the same attributes and contributions:
    ///    -                 'str:abilities' -> 'str:ability-mods'
    ///    - 'gnome:race' -> 'int:abilities' -> 'int:ability-mods'
    ///  
    /// </summary>
    [TestClass]
    public class RulesServiceTests
    {
        InMemoryEntityRepository<Campaigns.Model.Attribute> _attributes;
        InMemoryEntityRepository<Campaigns.Model.AttributeContribution> _contributions;
        InMemoryEntityRepository<Campaigns.Model.CharacterSheet> _characterSheets;
        InMemoryEntityRepository<Campaigns.Model.Character> _characters;

        RulesService _rules;
        IEnumerable<AttributeAllocation> _gnomeAllocations;
        CharacterUpdate _superStrongUpdate;

        private static Campaigns.Model.AttributeContribution Contrib(IEnumerable<AttributeContribution> contributions, string name, string category)
        {
            return contributions?.FirstOrDefault(c => c.Target.Name == name && c.Target.Category == category);
        }

        private static Campaigns.Model.AttributeValue AttribValue(IEnumerable<AttributeValue> vals, string name, string category)
        {
            return vals.First(val => val.Attribute.Name == name && val.Attribute.Category == category);
        }

        private static Campaigns.Model.Attribute Attrib(IEnumerable<Campaigns.Model.Attribute> attributes, string name, string category)
        {
            return attributes?.FirstOrDefault(a => a.Name == name && a.Category == category);
        }

        private Campaigns.Model.Attribute Attrib(string name, string category)
        {
            return Attrib(_attributes.AsQueryable, name, category);
        }

        [TestInitialize]
        public void TestInit()
        {
            _attributes = new InMemoryEntityRepository<Campaigns.Model.Attribute>();
            _contributions = new InMemoryEntityRepository<Campaigns.Model.AttributeContribution>();
            _contributions.AddForeignStore(_attributes);

            _characterSheets = new InMemoryEntityRepository<Campaigns.Model.CharacterSheet>();
            _characters = new InMemoryEntityRepository<Campaigns.Model.Character>();
            _characters.AddForeignStore(_characterSheets);

            _attributes.AddRange(new[]
            {
                new Campaigns.Model.Attribute { Name = "human", Category = "race", IsStandard = false },
                new Campaigns.Model.Attribute { Name = "gnome", Category = "race", IsStandard = false },
                new Campaigns.Model.Attribute { Name = "str", Category = "abilities", IsStandard = true },
                new Campaigns.Model.Attribute { Name = "int", Category = "abilities", IsStandard = true },
                new Campaigns.Model.Attribute { Name = "str", Category = "ability-mods", IsStandard = true },
                new Campaigns.Model.Attribute { Name = "int", Category = "ability-mods", IsStandard = true },
            });

            _contributions.AddRange(new[]
            {
                Attrib("gnome", "race").ConstantContributionTo(Attrib("int", "abilities"), 2),
                Attrib("str", "abilities").ContributionTo(Attrib("str", "ability-mods"), n => n / 2 - 5),
                Attrib("int", "abilities").ContributionTo(Attrib("int", "ability-mods"), n => n / 2 - 5),
            });

            _rules = new RulesService(
                _attributes,
                _contributions,
                _characterSheets,
                _characters);

            _gnomeAllocations = new []
            {
                new AttributeAllocation { Attribute = Attrib("gnome", "race") },
                new AttributeAllocation { Attribute = Attrib("str", "abilities"), Value = 8 },
                new AttributeAllocation { Attribute = Attrib("int", "abilities"), Value = 8 },
            };

            _superStrongUpdate = new CharacterUpdate
            {
                AddedOrUpdatedAllocations = new []
                {
                    new AttributeAllocation { Attribute = Attrib("str", "abilities"), Value = 20 },
                }
            };
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestSimpleCharacterCreationWithSpecification()
        {
            var character = _rules.CreateCharacter("", "", _gnomeAllocations);
            var characterSheet = character.Sheet;
            
            Assert.AreEqual(8, AttribValue(characterSheet.AttributeValues, "str", "abilities").Value);
            Assert.AreEqual(10, AttribValue(characterSheet.AttributeValues, "int", "abilities").Value);

            Assert.AreEqual(-1, AttribValue(characterSheet.AttributeValues, "str", "ability-mods").Value);
            Assert.AreEqual(0, AttribValue(characterSheet.AttributeValues, "int", "ability-mods").Value);
        }

        [TestMethod]
        public void TestSimpleCharacterUpdate()
        {
            var orig = _rules.CreateCharacter("", "", _gnomeAllocations);
            var updated = _rules.UpdateCharacter(orig, _superStrongUpdate);
            var updatedSheet = updated.Sheet;

            Assert.AreEqual(20, AttribValue(updatedSheet.AttributeValues, "str", "abilities").Value);
            Assert.AreEqual(10, AttribValue(updatedSheet.AttributeValues, "int", "abilities").Value);

            Assert.AreEqual(5, AttribValue(updatedSheet.AttributeValues, "str", "ability-mods").Value);
            Assert.AreEqual(0, AttribValue(updatedSheet.AttributeValues, "int", "ability-mods").Value);
        }

        [TestMethod]
        public void TestCharacterDiff()
        {
            var orig = _rules.CreateCharacter("", "", _gnomeAllocations);
            var origSheet = orig.Sheet;
            var updated = _rules.UpdateCharacter(orig, _superStrongUpdate);
            var updatedSheet = updated.Sheet;

            var diff = CharacterSheet.Diff(origSheet, updatedSheet);

            Assert.IsNull(diff.RemovedAllocations?.FirstOrDefault());
            Assert.IsNull(diff.RemovedAllocations?.FirstOrDefault());

            foreach (var contrib in _superStrongUpdate.AddedOrUpdatedAllocations)
            {
                Assert.IsTrue(diff.AddedOrUpdatedAllocations.Contains(contrib));
            }
        }
    }
}
