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
        public class DerivedFromConcreateBaseClass : ConcreteBaseClass
        {
            // TEST: Should be flagged as test method
            [Fact]
            public void DerivedTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        public abstract class AbstractBaseClass
        {
            // TEST: Should be flagged as test method and 
            // should have ability to run all derived classes
            // should have name `DerivedFromAbstractBaseClass.AbstractBaseTestMethod`
            [Fact]
            public void AbstractBaseTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        // TEST: Should have 2 tests; should also include "AbstractBaseClass.BaseTestMethod"
        public class DerivedFromAbstractBaseClass : AbstractBaseClass
        {
            // TEST: Should be flagged as test
            [Fact]
            public void DerivedTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        //TEST: should not throw exceptions
        //NOTE: nUnit also throws exception
        public abstract partial class AbstractPartialBaseClass { }
        public abstract partial class AbstractPartialBaseClass
        {
            // TEST: Should be flagged as test method and 
            // should have ability to run all derived classes 
            [Fact]
            public void AbstractBaseTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        // TEST: Should have 2 tests; should also include "AbstractBaseClass.BaseTestMethod"
        public class DerivedFromAbstractPartialBaseClass : AbstractPartialBaseClass
        {
            // TEST: Should be flagged as test
            [Fact]
            public void DerivedTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        public abstract class AbstractBaseClassWithoutInheritors
        {
            // TEST: Should not be flagged as test method
            [Fact]
            public void AbstractBaseTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }
    }
}
