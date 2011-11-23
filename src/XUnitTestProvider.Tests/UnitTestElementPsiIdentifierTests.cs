namespace XUnitTestProvider.Tests
{
    using System;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Tree;
    using NUnit.Framework;
    using ReSharper.XUnitTestProvider;

    public class UnitTestElementPsiIdentifierTests : PsiFileBaseTests
    {
        private void DoTest(Action<IDeclaredElement> action, params string[] sources)
        {
            WithEachPsiFile(file => file.ProcessDescendants(new RecursiveElementProcessor(action)), sources);
        }

        private class RecursiveElementProcessor : IRecursiveElementProcessor
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
                    action(declaration.DeclaredElement);
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

        [Test]
        public void ShouldMarkPublicClassAsPublic()
        {
            bool isPublic = false;
            Action<IDeclaredElement> action =
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
                    };
            DoTest(action, "IsPublicTests.cs");
            Assert.IsTrue(isPublic);
        }
        
        [Test]
        public void ShouldMarkPublicMethodInPublicClassAsPublic()
        {
            bool isPublic = false;
            Action<IDeclaredElement> action =
                node =>
                    {
                        var method = node as IMethod;
                        if (method != null)
                        {
                            if (method.ShortName == "PublicMethod")
                            {
                                isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                            }
                        }
                    };
            DoTest(action, "IsPublicTests.cs");
            Assert.IsTrue(isPublic);
        }
        
        [Test]
        public void ShouldMarkPublicStaticMethodInPublicClassAsPublic()
        {
            bool isPublic = false;
            Action<IDeclaredElement> action =
                node =>
                    {
                        var method = node as IMethod;
                        if (method != null)
                        {
                            if (method.ShortName == "PublicStaticMethod")
                            {
                                isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                            }
                        }
                    };
            DoTest(action, "IsPublicTests.cs");
            Assert.IsTrue(isPublic);
        }
        
        [Test]
        public void ShouldNotMarkProtectedMethodInPublicClassAsPublic()
        {
            bool isPublic = true;
            Action<IDeclaredElement> action =
                node =>
                    {
                        var method = node as IMethod;
                        if (method != null)
                        {
                            if (method.ShortName == "ProtectedMethod")
                            {
                                isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                            }
                        }
                    };
            DoTest(action, "IsPublicTests.cs");
            Assert.False(isPublic);
        }
        
        [Test]
        public void ShouldNotMarkProtectedStaticMethodInPublicClassAsPublic()
        {
            bool isPublic = true;
            Action<IDeclaredElement> action =
                node =>
                    {
                        var method = node as IMethod;
                        if (method != null)
                        {
                            if (method.ShortName == "ProtectedStaticMethod")
                            {
                                isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                            }
                        }
                    };
            DoTest(action, "IsPublicTests.cs");
            Assert.False(isPublic);
        }
        
        [Test]
        public void ShouldNotMarkPrivateMethodInPublicClassAsPublic()
        {
            bool isPublic = true;
            Action<IDeclaredElement> action =
                node =>
                    {
                        var method = node as IMethod;
                        if (method != null)
                        {
                            if (method.ShortName == "PrivateMethod")
                            {
                                isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                            }
                        }
                    };
            DoTest(action, "IsPublicTests.cs");
            Assert.False(isPublic);
        }

        [Test]
        public void ShouldNotMarkPrivateStaticMethodInPublicClassAsPublic()
        {
            bool isPublic = true;
            Action<IDeclaredElement> action =
                node =>
                    {
                        var method = node as IMethod;
                        if (method != null)
                        {
                            if (method.ShortName == "PrivateStaticMethod")
                            {
                                isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                            }
                        }
                    };
            DoTest(action, "IsPublicTests.cs");
            Assert.False(isPublic);
        }
        
        [Test]
        public void ShouldMarkNestedPublicClassAsPublic()
        {
            bool isPublic = false;
            Action<IDeclaredElement> action =
                node =>
                    {
                        var @class = node as IClass;
                        if (@class != null)
                        {
                            if (@class.ShortName == "NestedPublicClass")
                            {
                                isPublic = UnitTestElementPsiIdentifier.IsPublic(@class);
                            }
                        }
                    };
            DoTest(action, "IsPublicTests.cs");
            Assert.IsTrue(isPublic);
        }

        [Test]
        public void ShouldMarkPublicMethodInNestedPublicClassAsPublic()
        {
            bool isPublic = false;
            Action<IDeclaredElement> action =
                node =>
                {
                    var method = node as IMethod;
                    if (method != null)
                    {
                        if (method.ShortName == "NestedPublicMethod")
                        {
                            isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                        }
                    }
                };
            DoTest(action, "IsPublicTests.cs");
            Assert.IsTrue(isPublic);
        }

        [Test]
        public void ShouldMarkPublicStaticMethodInNestedPublicClassAsPublic()
        {
            bool isPublic = false;
            Action<IDeclaredElement> action =
                node =>
                {
                    var method = node as IMethod;
                    if (method != null)
                    {
                        if (method.ShortName == "NestedPublicStaticMethod")
                        {
                            isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                        }
                    }
                };
            DoTest(action, "IsPublicTests.cs");
            Assert.IsTrue(isPublic);
        }

        [Test]
        public void ShouldNotMarkProtectedMethodInNestedPublicClassAsPublic()
        {
            bool isPublic = true;
            Action<IDeclaredElement> action =
                node =>
                {
                    var method = node as IMethod;
                    if (method != null)
                    {
                        if (method.ShortName == "NestedProtectedMethod")
                        {
                            isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                        }
                    }
                };
            DoTest(action, "IsPublicTests.cs");
            Assert.False(isPublic);
        }

        [Test]
        public void ShouldNotMarkProtectedStaticMethodInNestedPublicClassAsPublic()
        {
            bool isPublic = true;
            Action<IDeclaredElement> action =
                node =>
                {
                    var method = node as IMethod;
                    if (method != null)
                    {
                        if (method.ShortName == "NestedProtectedStaticMethod")
                        {
                            isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                        }
                    }
                };
            DoTest(action, "IsPublicTests.cs");
            Assert.False(isPublic);
        }

        [Test]
        public void ShouldNotMarkPrivateMethodInNestedPublicClassAsPublic()
        {
            bool isPublic = true;
            Action<IDeclaredElement> action =
                node =>
                {
                    var method = node as IMethod;
                    if (method != null)
                    {
                        if (method.ShortName == "NestedPrivateMethod")
                        {
                            isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                        }
                    }
                };
            DoTest(action, "IsPublicTests.cs");
            Assert.False(isPublic);
        }

        [Test]
        public void ShouldNotMarkPrivateStaticMethodInNestedPublicClassAsPublic()
        {
            bool isPublic = true;
            Action<IDeclaredElement> action =
                node =>
                {
                    var method = node as IMethod;
                    if (method != null)
                    {
                        if (method.ShortName == "NestedPrivateStaticMethod")
                        {
                            isPublic = UnitTestElementPsiIdentifier.IsPublic(method);
                        }
                    }
                };
            DoTest(action, "IsPublicTests.cs");
            Assert.False(isPublic);
        }
    }
}