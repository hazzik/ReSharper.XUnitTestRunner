namespace tests.xunit
{
    namespace TestsInDerivedClasses
    {
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