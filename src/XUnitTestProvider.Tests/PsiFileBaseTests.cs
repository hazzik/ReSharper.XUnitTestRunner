namespace XUnitTestProvider.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Application;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.TestFramework;
    using NUnit.Framework;

    public abstract class PsiFileBaseTests : BaseTestWithSingleProject
    {
        protected void WithEachPsiFile(Action<IFile> action, params string[] sources)
        {
            WithSingleProjectGuarded(sources, project =>
                                                  {
                                                      foreach (var src in sources)
                                                          using (ReadLockCookie.Create())
                                                              action(GetFile(src, project));
                                                  });
        }

        protected void WithSingleProjectGuarded(IEnumerable<string> testSrc, Action<IProject> action)
        {
            WithSingleProject(testSrc, (lifetime, project) => Locks.ReentrancyGuard.Execute("THIS IS SPARTA!", () => action(project)));
        }

        protected IFile GetFile(string testSrc, IProjectFolder project)
        {
            var item = (IProjectFile) project.GetSubItem(testSrc);
            Assert.NotNull(item);

            IPsiSourceFile sourceFile = item.ToSourceFile();

            Assert.IsNotNull(sourceFile);

            Assert.True(sourceFile.IsValid());

            IFile file = sourceFile.EnumeratePsiFiles().First();
            return file;
        }
    }
}