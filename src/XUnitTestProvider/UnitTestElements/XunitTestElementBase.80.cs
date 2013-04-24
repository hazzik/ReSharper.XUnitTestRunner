namespace ReSharper.XUnitTestProvider
{
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.ReSharper.UnitTestFramework.Strategy;

    public partial class XunitTestElementBase
    {
        public IUnitTestRunStrategy GetRunStrategy(IHostProvider hostProvider)
        {
            return ServiceProvider.Strategy;
        }
    }
}