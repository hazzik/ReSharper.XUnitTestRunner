namespace XUnitTestProvider.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.ReSharper.UnitTestFramework;
    using NUnit.Framework;
    using ReSharper.XUnitTestProvider;

    [TestFixture]
    public class XunitFileExplorerTests : XunitFileExplorerTestsBase
    {
        private static XunitTestClassElement AssertTestClass(IUnitTestElement unitTestElement, object shortName)
        {
            Assert.IsInstanceOf<XunitTestClassElement>(unitTestElement);
            Assert.AreEqual(shortName, unitTestElement.ShortName);
            Assert.AreEqual(UnitTestElementState.Valid, unitTestElement.State);
            return (XunitTestClassElement) unitTestElement;
        }

        private static XunitTestMethodElement AssertTestMethod(IUnitTestElement unitTestElement, string shortName)
        {
            Assert.IsInstanceOf<XunitTestMethodElement>(unitTestElement);
            Assert.AreEqual(shortName, unitTestElement.ShortName);
            Assert.AreEqual(UnitTestElementState.Valid, unitTestElement.State);
            return (XunitTestMethodElement) unitTestElement;
        }

        [Test]
        public void ShouldBeAbleToFindRegularFact()
        {
            List<IUnitTestElement> tests = FindUnitTestElements("single_test.cs").ToList();

            Assert.AreEqual(2, tests.Count);
            
            AssertTestClass(tests[0], "FailingTests");
            AssertTestMethod(tests[1], "FailsDueToThrownException");
        }

        [Test]
        public void ShouldBeAbleToFindAllTestsInAllPartialParts()
        {
            List<IUnitTestElement> elements = FindUnitTestElements("PartialClass.File1.cs", "PartialClass.File2.cs").ToList();
            
            Assert.AreEqual(4, elements.Count);
            
            AssertTestClass(elements[0], "PartialClass");
            AssertTestMethod(elements[1], "TestInFile1");
            AssertTestMethod(elements[2], "TestInSecondInstanceInFile1");
            AssertTestMethod(elements[3], "TestInFile2");
        }

        [Test]
        public void ShouldBeAbleToDiscoverSkippedTest()
        {
            List<IUnitTestElement> elements = FindUnitTestElements("SkippedTests.cs").ToList();
            
            Assert.AreEqual(2, elements.Count);

            AssertTestClass(elements[0], "SkippedTests");
            AssertTestMethod(elements[1], "SkippedTestMethod");
        }

        [Test]
        public void ShouldNotMarkFactsInAbstractClassIfItHasNoInheritors()
        {
            List<IUnitTestElement> elements = FindUnitTestElements("AbstractBaseClassWithoutInheritors.cs").ToList();
            
            Assert.AreEqual(0, elements.Count);
        }

        [Test]
        public void ShouldBeAbleToDiscoverTestsFromBaseClass()
        {
            var elements = FindUnitTestElements("DerivedFromConcreteBaseClass.cs").ToList();
            
            Assert.AreEqual(4, elements.Count);

            AssertTestClass(elements[0], "ConcreteBaseClass");
            XunitTestMethodElement method1 = AssertTestMethod(elements[1], "BaseTestMethod");
            Assert.AreEqual("BaseTestMethod", method1.GetPresentation());

            XunitTestClassElement @class = AssertTestClass(elements[2], "DerivedFromConcreteBaseClass");

            Assert.AreEqual(2, @class.Children.Count);
            XunitTestMethodElement method2 = AssertTestMethod(@class.Children.First(), "BaseTestMethod");
            Assert.AreEqual("ConcreteBaseClass.BaseTestMethod", method2.GetPresentation()); 
            AssertTestMethod(@class.Children.Last(), "DerivedTestMethod");
            
            AssertTestMethod(elements[3], "DerivedTestMethod");
        }
    }
}