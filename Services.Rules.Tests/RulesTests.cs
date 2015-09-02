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

        RulesService _rules;
        CharacterSpecification _gnomeAllocations;
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
                _characterSheets);

            _gnomeAllocations = new CharacterSpecification
            {
                Attributes = new[]
                {
                    Attrib("gnome", "race"),
                },
                Allocations = new[]
                {
                    Attrib("str", "abilities").ConstantContributionFrom(null, 8),
                    Attrib("int", "abilities").ConstantContributionFrom(null, 8),
                }
            };

            _superStrongUpdate = new CharacterUpdate
            {
                AddedAllocations = new []
                {
                    Attrib("str", "abilities").ConstantContributionFrom(null, 20),
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
            var characterSheet = _rules.CreateCharacterSheet(_gnomeAllocations);
            
            Assert.AreEqual(8, AttribValue(characterSheet.AttributeValues, "str", "abilities").Value);
            Assert.AreEqual(10, AttribValue(characterSheet.AttributeValues, "int", "abilities").Value);

            Assert.AreEqual(-1, AttribValue(characterSheet.AttributeValues, "str", "ability-mods").Value);
            Assert.AreEqual(0, AttribValue(characterSheet.AttributeValues, "int", "ability-mods").Value);
        }

        [TestMethod]
        public void TestSimpleCharacterUpdate()
        {
            var orig = _rules.CreateCharacterSheet(_gnomeAllocations);
            var updated = _rules.UpdateCharacterSheet(orig, _superStrongUpdate);

            Assert.AreEqual(20, AttribValue(updated.AttributeValues, "str", "abilities").Value);
            Assert.AreEqual(10, AttribValue(updated.AttributeValues, "int", "abilities").Value);

            Assert.AreEqual(5, AttribValue(updated.AttributeValues, "str", "ability-mods").Value);
            Assert.AreEqual(0, AttribValue(updated.AttributeValues, "int", "ability-mods").Value);
        }

        [TestMethod]
        public void TestCharacterDiff()
        {
            var orig = _rules.CreateCharacterSheet(_gnomeAllocations);
            var updated = _rules.UpdateCharacterSheet(orig, _superStrongUpdate);

            var diff = CharacterSheet.Diff(orig, updated);

            Assert.IsNull(diff.RemovedAttributes?.FirstOrDefault());
            Assert.IsNull(diff.RemovedAllocations?.FirstOrDefault());

            foreach (var contrib in _superStrongUpdate.AddedAllocations)
            {
                Assert.IsTrue(diff.AddedAllocations.Contains(contrib));
            }
        }
    }
}
