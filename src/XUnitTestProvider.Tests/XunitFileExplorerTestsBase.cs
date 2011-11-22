namespace XUnitTestProvider.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Application;
    using JetBrains.DataFlow;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Search;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.TestFramework;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.ReSharper.UnitTestFramework.Elements;
    using Moq;
    using NUnit.Framework;
    using ReSharper.XUnitTestProvider;

    [TestReferences(@"D:\hazzik\Projects\ReSharper.XUnitTestRunner\3rdParty\xUnit.net-1.8\xunit.dll", @"D:\hazzik\Projects\ReSharper.XUnitTestRunner\3rdParty\xUnit.net-1.8\xunit.extensions.dll")]
    public abstract class XunitFileExplorerTestsBase : BaseTestWithSingleProject
    {
        public ICollection<UnitTestElementDisposition> FindUnitTestElementDispositions(params string[] testSrc)
        {
            var tests = new List<UnitTestElementDisposition>();
            Action<Lifetime, IProject> action =
                (lifetime, project) =>
                Locks.ReentrancyGuard.Execute("THIS IS SPARTA!",
                                              () =>
                                                  {
                                                      foreach (var s in testSrc)
                                                      {
                                                          using (ReadLockCookie.Create())
                                                              DoFindUnitTestElementDisposition(s, project, tests);
                                                      }
                                                  });
            WithSingleProject(testSrc, action);
            return tests;
        }

        private static void DoFindUnitTestElementDisposition(string testSrc, IProjectFolder project, ICollection<UnitTestElementDisposition> tests)
        {
            var item = (IProjectFile) project.GetSubItem(testSrc);
            Assert.NotNull(item);

            IPsiSourceFile sourceFile = item.ToSourceFile();
            
            Assert.IsNotNull(sourceFile);
            
            Assert.True(sourceFile.IsValid());
            
            IFile file = sourceFile.EnumeratePsiFiles().First();
            
            var factory = new XunitElementFactory(new XunitTestProvider(), new Mock<IUnitTestElementManager>().Object);
            
            var explorer = new XunitFileExplorer(factory,
                                                 file,
                                                 SearchDomainFactory.Instance,
                                                 tests.Add,
                                                 () => false);
            file.ProcessDescendants(explorer);
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