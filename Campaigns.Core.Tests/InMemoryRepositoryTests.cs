using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Campaigns.Core.Testing;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Campaigns.Core.Tests
{
    [TestClass]
    public class InMemoryEntityRepositoryTests
    {
        InMemoryEntityRepository<Group> _groups;
        InMemoryEntityRepository<Person> _people;
        InMemoryEntityRepository<Pet> _pets;

        [TestInitialize]
        public void InitTests()
        {
            _people = new InMemoryEntityRepository<Person>();
            _groups = new InMemoryEntityRepository<Group>();
            _pets = new InMemoryEntityRepository<Pet>();

            _groups.AddForeignStore(_people);
            _people.AddForeignStore(_groups);
            _pets.AddForeignStore(_people);

            var g1 = new Group { Name = "Students" };
            _groups.Add(g1);

            var p1 = new Person { Name = "Amy", Group = g1 };
            _people.Add(p1);
            _people.Add(new Person { Name = "Bernie", GroupId = g1.Id });

            _pets.Add(new Pet { Name = "Charlie", Owner = p1 });
            _pets.Add(new Pet { Name = "Dover", OwnerId = p1.Id });
        }

        [TestMethod]
        public void TestReposContainExpectedData()
        {
            var amy = _people.GetById(1);
            Assert.IsNotNull(amy);
            Assert.AreEqual("Amy", amy.Name);

            Assert.AreEqual(1, _groups.EntityTable.Count());
            Assert.AreEqual(2, _people.EntityTable.Count());
            Assert.AreEqual(2, _pets.EntityTable.Count());
        }

        [TestMethod]
        public void TestForegnKeyRelationshipsAreSyncd()
        {
            var g1 = _groups.EntityTable.First();
            var p1 = _people.EntityTable.First();

            foreach (var person in _people.EntityTable)
            {
                Assert.AreEqual(g1, person.Group);
                Assert.AreEqual(g1.Id, person.GroupId);
            }

            foreach (var pet in _pets.EntityTable)
            {
                Assert.AreEqual(p1, pet.Owner);
                Assert.AreEqual(p1.Id, pet.OwnerId);
            }
        }

        [TestMethod]
        public void TestEntitiesMayHaveNullForeignKeys()
        {
            _people.Add(new Person { Name = "Paul" });

            var p1 = _people.EntityTable.FirstOrDefault(p => p.Name == "Paul");
            Assert.IsNotNull(p1);

            Assert.IsNull(p1.Group);
            Assert.AreEqual(0, p1.GroupId);
        }

        [TestMethod]
        public void TestEntityEquality()
        {
            Assert.AreEqual(new Person { Id = 1 }, new Person { Id = 1 });
        }
    }

    public class Person : BaseEntity
    {
        public string Name { get; set; }
        [ForeignKey("Group")]
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }

    public class Group : BaseEntity
    {
        public string Name { get; set; }
    }

    public class Pet : BaseEntity
    {
        public string Name { get; set; }
        public int? OwnerId { get; set; } 
        [ForeignKey("OwnerId")]
        public Person Owner { get; set; }
    }
}
