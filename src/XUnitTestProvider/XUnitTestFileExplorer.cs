namespace ReSharper.XUnitTestProvider
{
    using JetBrains.Application;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Search;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.UnitTestFramework;

    [FileUnitTestExplorer]
    public class XUnitTestFileExplorer : IUnitTestFileExplorer
    {
        private readonly XunitTestProvider provider;
        private readonly XunitElementFactory factory;
        private readonly SearchDomainFactory searchDomainFactory;

        public XUnitTestFileExplorer(XunitTestProvider provider, XunitElementFactory factory, SearchDomainFactory searchDomainFactory)
        {
            this.provider = provider;
            this.factory = factory;
            this.searchDomainFactory = searchDomainFactory;
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
        }

        public void ExploreFile(IFile psiFile, UnitTestElementLocationConsumer consumer, CheckForInterrupt interrupted)
        {
            if (psiFile.Language.Name != "CSHARP" && psiFile.Language.Name != "VBASIC")
                return;

            psiFile.ProcessDescendants(new XunitFileExplorer(factory, psiFile, searchDomainFactory, consumer, interrupted));
        }
    }
}
