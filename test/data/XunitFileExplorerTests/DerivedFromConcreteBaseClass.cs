using Xunit;

namespace tests.xunit
{
    namespace TestsInDerivedClasses
    {
        public class ConcreteBaseClass
        {
            // TEST: Should be flagged as test method
            [Fact]
            public void BaseTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        // TEST: Should have 2 tests; should also include "ConcreteBaseClass.BaseTestMethod"
        public class DerivedFromConcreteBaseClass : ConcreteBaseClass
        {
            // TEST: Should be flagged as test method
            [Fact]
            public void DerivedTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }
    }
}
