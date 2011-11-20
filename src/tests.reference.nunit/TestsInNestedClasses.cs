using NUnit.Framework;

namespace tests.reference.nunit
{
    namespace TestsInNestedClasses
    {
        // Not run
        public class ParentClass
        {
            [TestFixture]
            public class NestedClass
            {
                [Test]
                public void NestedTest()
                {
                    Assert.AreEqual(1, 1);
                }
            }
        }
        
        [TestFixture]
        public class ParentClassWithAttribute
        {
            [TestFixture]
            public class NestedClass
            {
                [Test]
                public void NestedTest()
                {
                    Assert.AreEqual(1, 1);
                }
            }
        }

        [TestFixture]
        public class ParentClassWithTests
        {
            [Test]
            public void ParentTest()
            {
                Assert.AreEqual(1, 1);
            }

            [TestFixture]
            public class NestedClass
            {
                [Test]
                public void NestedTest()
                {
                    Assert.AreEqual(1, 1);
                }
            }
        }
    }
}
