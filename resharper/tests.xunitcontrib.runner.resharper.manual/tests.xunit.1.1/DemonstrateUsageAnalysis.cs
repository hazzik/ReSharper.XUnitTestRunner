﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace tests.xunit
{
    namespace DemonstrateUsageAnalysis
    {
        // TEST: This namespace should be marked as in use
        namespace NamespaceContainingSingleClassMarkedAsUsed
        {
            // TEST: This class should be marked as in use
            public class SingleTestClass
            {
                // TEST: This method should be marked as in use
                [Fact]
                public void TestMethodMarkedAsInUse()
                {
                    Assert.Equal(1, 1);
                }
            }
        }

        // For completeness...
        namespace EmptyNamespaceMarkedAsNotInUse
        {
        }

        // TEST: Parent class should be marked as in use
        public class ParentClassMarkedAsInUse
        {
            // TEST: Nested class should be marked as in use
            // TEST: This class should get a test class marker
            public class NestedClassMarkedAsInUse
            {
                [Fact]
                public void TestMethodMarkedAsInUse()
                {
                    Assert.Equal(1, 1);
                }
            }
        }

        // TEST: This class should be marked as in use
        // TEST: This class should be flagged as a test class
        public class ClassInUse
        {
            // TEST: This method should be marked as in use
            // TEST: This method should *NOT* be flagged as a test
            [Fact]
            public void TestMethodMarkedInUse()
            {
            }
        }

        // TEST: This class should be marked as in use
        public class BaseClassFixtureMarkedAsInUse
        {
        }

        // TEST: This class should be marked as in use
        public class DerivedClassMarkedAsInUse : BaseClassFixtureMarkedAsInUse
        {
            [Fact]
            public void TestMethod()
            {
            }
        }

        public class PropertyDataTheoryTest
        {
            [Theory]
            [PropertyData("TheoryDataEnumerator")]
            public void DataFromProperty(int value)
            {
                Console.WriteLine("DataFromProperty({0})", value);
            }

            // TEST: This property should be marked as in use
            public static IEnumerable<object[]> TheoryDataEnumerator
            {
                get { return Enumerable.Range(1, 10).Select(x => new object[] { x }); }
            }

            // TEST: This propert should be marked as *NOT* in use
            public static IEnumerable<object[]> NotTheoryDataEnumerator
            {
                get { return null; }
            }
        }
    }
}