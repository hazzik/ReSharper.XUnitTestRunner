namespace ReSharper.XUnitTestProvider
{
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Caches;

    [SolutionComponent]
    public class XunitServiceProvider
    {
        private readonly XunitTestProvider provider;
        private readonly PsiModuleManager psiModuleManager;
        private readonly CacheManager cacheManager;

        public XunitServiceProvider(XunitTestProvider provider, PsiModuleManager psiModuleManager, CacheManager cacheManager)
        {
            this.provider = provider;
            this.psiModuleManager = psiModuleManager;
            this.cacheManager = cacheManager;
        }

        public PsiModuleManager PsiModuleManager
        {
            get { return psiModuleManager; }
        }

        public CacheManager CacheManager
        {
            get { return cacheManager; }
        }

        public XunitTestProvider Provider
        {
            get { return provider; }
        }
    }
}