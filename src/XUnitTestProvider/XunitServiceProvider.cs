namespace ReSharper.XUnitTestProvider
{
    using JetBrains.ProjectModel;

    [SolutionComponent]
    public partial class XunitServiceProvider
    {
        private readonly XunitTestProvider provider;

        public XunitTestProvider Provider
        {
            get { return provider; }
        }
    }
}