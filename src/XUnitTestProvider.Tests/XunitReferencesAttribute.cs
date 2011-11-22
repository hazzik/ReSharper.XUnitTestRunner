namespace XUnitTestProvider.Tests
{
    using System.IO;
    using JetBrains.ReSharper.TestFramework;

    public class XunitReferencesAttribute : TestReferencesAttribute
    {
        public XunitReferencesAttribute() : base(new string[0])
        {
        }

        public override string[] GetReferences()
        {
            return new[]
                       {
                           Path.GetFullPath("xunit.dll"),
                           Path.GetFullPath("xunit.extensions.dll")
                       };
        }
    }
}