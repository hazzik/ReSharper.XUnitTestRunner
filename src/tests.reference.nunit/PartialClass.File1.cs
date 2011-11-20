namespace tests.xunit
{
    namespace PartialClasses
    {
        using NUnit.Framework;

        [TestFixture]
        public partial class PartialClass
        {
            [TestCase]
            public void TestInFile1()
            {
                Assert.AreEqual(2, 2);
            }
        }

        public partial class PartialClass
        {
            [TestCase]
            public void TestInSecondInstanceInFile1()
            {
            }
        }
    }
}