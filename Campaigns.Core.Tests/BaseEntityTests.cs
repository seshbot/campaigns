using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Core.Tests
{
    [TestClass]
    public class BaseEntityTests
    {
        [TestMethod]
        public void TestEntityEquality()
        {
            Assert.AreEqual(new SimplePerson { Id = 1 }, new SimplePerson { Id = 1 });
        }
    }

    public class SimplePerson : BaseEntity
    {
        public string Name { get; set; }
    }
}
