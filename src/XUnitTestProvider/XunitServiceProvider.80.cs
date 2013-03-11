namespace ReSharper.XUnitTestProvider
{
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.Psi.Modules;
    using JetBrains.ReSharper.TaskRunnerFramework;
    using JetBrains.ReSharper.UnitTestFramework.Strategy;
    using XUnitTestRunner;

    public partial class XunitServiceProvider
    {
        private readonly ISymbolCache cacheManager;
        private readonly IPsiModules psiModuleManager;

        private readonly OutOfProcessUnitTestRunStrategy strategy =
            new OutOfProcessUnitTestRunStrategy(new RemoteTaskRunnerInfo(XunitTaskRunner.RunnerId,
                typeof (XunitTaskRunner)));

        public XunitServiceProvider(XunitTestProvider provider, IPsiModules psiModuleManager, ISymbolCache cacheManager)
        {
            this.provider = provider;
            this.psiModuleManager = psiModuleManager;
            this.cacheManager = cacheManager;
        }

        public OutOfProcessUnitTestRunStrategy Strategy
        {
            get { return strategy; }
        }

        public ISymbolCache CacheManager
        {
            get { return cacheManager; }
        }

        public IPsiModules PsiModuleManager
        {
            get { return psiModuleManager; }
        }
    }
}