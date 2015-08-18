using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Model.Calculations.Tests
{
    [TestClass]
    public class CalculationServiceTests
    {
        TestContext context;
        CalculationService service;

        Attribute Attrib(string name, string category)
        {
            return context.GetAttribute(name, category);
        }

        [TestInitialize]
        public void TestInit()
        {
            context = new TestContext();
            service = new CalculationService();
        }

        [TestMethod]
        public void TestContributionsCascade()
        {
            context.SetInitialValue(Attrib("str", "ability"), 8);
            context.SetInitialValue(Attrib("int", "ability"), 8);

            var result = service.Calculate(context);

            result.AssertAttribValue(Attrib("gnome", "race"), 0);

            result.AssertAttribValue(Attrib("str", "ability"), 8);
            result.AssertAttribValue(Attrib("int", "ability"), 10);

            result.AssertAttribValue(Attrib("str", "ability-modifier"), -1);
            result.AssertAttribValue(Attrib("int", "ability-modifier"), 0);

            result.AssertAttribValue(Attrib("athletics", "skill"), -1);
            result.AssertAttribValue(Attrib("arcana", "skill"), 0);

            result.AssertAttribContributionFrom(Attrib("int", "ability"), Attrib("gnome", "race"));
            result.AssertAttribContributionFrom(Attrib("arcana", "skill"), Attrib("int", "ability-modifier"));
        }
    }

    static class CalculationResultHelper
    {
        public static void AssertAttribValue(this CalculationResult result, Attribute attrib, int value)
        {
            var values = result.AttributeValues.Where(val => val.Attribute == attrib);
            Assert.IsTrue(values.Count() == 1);
            Assert.AreEqual(value, values.First().Value);
        }

        public static void AssertAttribContributionFrom(this CalculationResult result, Attribute attrib, Attribute contributingAttrib)
        {
            var values = result.AttributeValues.Where(val => val.Attribute == attrib);
            Assert.IsTrue(values.Count() == 1);
            Assert.IsTrue(values.First().Contributions.Count(c => c.Source == contributingAttrib) == 1);
        }
    }
}
