namespace ReSharper.XUnitTestProvider
{
    using System.IO;
    using JetBrains.Application;
    using JetBrains.Application.Settings;
    using JetBrains.DataFlow;
    using Properties;

    [ShellComponent]
    public class XunitDefaultTemplates : IHaveDefaultSettingsStream
    {
        public Stream GetDefaultSettingsStream(Lifetime lifetime)
        {
            var stream = new MemoryStream(Resources.LiveTemplates);
            lifetime.AddDispose(stream);
            return stream;
        }

        public string Name
        {
            get { return "xUnit.net Default Templates"; }
        }
    }
}
