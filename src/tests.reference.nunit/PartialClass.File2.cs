namespace tests.xunit
{
    namespace PartialClasses
    {
        using NUnit.Framework;

        public partial class PartialClass
        {
            [TestCase]
            public void TestInFile2()
            {
                Assert.AreEqual(3, 3);
            }
        }
    }
}