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

        [TestInitialize]
        public void TestInit()
        {
            context = new TestContext();
            service = new CalculationService();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void SimpleAttributecontributionTest()
        {
            context.SetInitialValue(context.Race("gnome"), 0);
            context.SetInitialValue(context.Ability("int"), 8);
            var result = service.Calculate(context);
            result.AssertAttribValue(context.Ability("int"), 10);
        }

        [TestMethod]
        public void AttributeContributionsCascade()
        {
            context.SetInitialValue(context.Race("gnome"), 0);

            context.SetInitialValue(context.Ability("str"), 8);
            context.SetInitialValue(context.Ability("int"), 8);

            var result = service.Calculate(context);

            result.AssertAttribValue(context.Race("gnome"), 0);

            result.AssertAttribValue(context.Ability("str"), 8);
            result.AssertAttribValue(context.Ability("int"), 10);

            result.AssertAttribValue(context.AbilityMod("str"), -1);
            result.AssertAttribValue(context.AbilityMod("int"), 0);

            result.AssertAttribValue(context.Skill("athletics"), -1);
            result.AssertAttribValue(context.Skill("arcana"), 0);

            result.AssertAttribContributionFrom(context.Ability("int"), context.Race("gnome"));
            result.AssertAttribContributionFrom(context.Skill("arcana"), context.AbilityMod("int"));
        }

        [TestMethod]
        public void UnrelatedAttributeContributionsDoNotCascade()
        {
            context.SetInitialValue(context.Race("human"), 0);

            context.SetInitialValue(context.Ability("str"), 8);
            context.SetInitialValue(context.Ability("int"), 8);

            var result = service.Calculate(context);

            result.AssertNoAttrib(context.Race("gnome"));
            result.AssertAttribValue(context.Ability("int"), 8);
            result.AssertAttribValue(context.Skill("arcana"), -1);
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

        public static void AssertNoAttrib(this CalculationResult result, Attribute attrib)
        {
            var count = result.AttributeValues.Count(val => val.Attribute == attrib);
            Assert.AreEqual(0, count);
        }

        public static void AssertAttribContributionFrom(this CalculationResult result, Attribute attrib, Attribute contributingAttrib)
        {
            var values = result.AttributeValues.Where(val => val.Attribute == attrib);
            Assert.IsTrue(values.Count() == 1);
            Assert.IsTrue(values.First().Contributions.Count(c => c.Source == contributingAttrib) == 1);
        }
    }
}
