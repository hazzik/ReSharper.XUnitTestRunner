namespace XUnitTestProvider.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Search;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.TestFramework;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.ReSharper.UnitTestFramework.Elements;
    using Moq;
    using ReSharper.XUnitTestProvider;

    [TestReferences(@"D:\hazzik\Projects\ReSharper.XUnitTestRunner\3rdParty\xUnit.net-1.8\xunit.dll", @"D:\hazzik\Projects\ReSharper.XUnitTestRunner\3rdParty\xUnit.net-1.8\xunit.extensions.dll")]
    public abstract class XunitFileExplorerTestsBase : PsiFileBaseTests
    {
        protected IEnumerable<UnitTestElementDisposition> FindUnitTestElementDispositions(params string[] testSrc)
        {
            var tests = new List<UnitTestElementDisposition>();
            WithEachPsiFile(file => DoFindUnitTestElementDisposition(tests, file), testSrc);
            return tests;
        }

        private static object DoFindUnitTestElementDisposition(ICollection<UnitTestElementDisposition> tests, ITreeNode file)
        {
            var factory = new XunitElementFactory(new XunitTestProvider(), new Mock<IUnitTestElementManager>().Object);

            var explorer = new XunitFileExplorer(factory,
                                                 file,
                                                 SearchDomainFactory.Instance,
                                                 tests.Add,
                                                 () => false);
            file.ProcessDescendants(explorer);

            return null;
        }

        protected IEnumerable<IUnitTestElement> FindUnitTestElements(params string[] fileNames)
        {
            return FindUnitTestElementDispositions(fileNames)
                .Select(x => x.UnitTestElement)
                .Distinct()
                .ToList();
        }
    }
}