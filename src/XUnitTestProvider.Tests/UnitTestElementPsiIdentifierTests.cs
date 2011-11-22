namespace XUnitTestProvider.Tests
{
    using System;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Tree;
    using NUnit.Framework;
    using ReSharper.XUnitTestProvider;

    public class UnitTestElementPsiIdentifierTests : PsiFileBaseTests
    {
        [Test]
        public void ShouldMarkPublicClassAsPublic()
        {
            bool isPublic = false;
            WithEachPsiFile(file =>
                            file.ProcessDescendants(
                                new RecursiveElementProcessor(
                                    node =>
                                        {
                                            var @class = node as IClass;
                                            if (@class != null)
                                            {
                                                if (@class.ShortName == "PublicClass")
                                                {
                                                    isPublic = UnitTestElementPsiIdentifier.IsPublic(@class);
                                                }
                                            }
                                        })), "PublicClass.cs");
            Assert.IsTrue(isPublic);
        }
    }

    public class RecursiveElementProcessor : IRecursiveElementProcessor
    {
        private readonly Action<IDeclaredElement> action;

        public RecursiveElementProcessor(Action<IDeclaredElement> action)
        {
            this.action = action;
        }

        #region IRecursiveElementProcessor Members

        public bool InteriorShouldBeProcessed(ITreeNode element)
        {
            return !(element is ITypeMemberDeclaration) || element is ITypeDeclaration;
        }

        public void ProcessBeforeInterior(ITreeNode element)
        {
            var declaration = element as IDeclaration;
            if (declaration != null)
                action((declaration).DeclaredElement);
        }

        public void ProcessAfterInterior(ITreeNode element)
        {
        }

        public bool ProcessingIsFinished
        {
            get { return false; }
        }

        #endregion
    }
}