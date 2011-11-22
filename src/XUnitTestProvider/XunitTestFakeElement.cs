namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.Util;

    public sealed class XunitTestFakeElement : IUnitTestElement
    {
        private readonly string myMethodName;
        private readonly IProject myProject;

        public XunitTestFakeElement(IUnitTestProvider provider, IProject project, IClrTypeName typeName, string methodName)
        {
            Provider = provider;
            myProject = project;
            TypeName = typeName;
            myMethodName = methodName;
            State = UnitTestElementState.Fake;
        }

        public IClrTypeName TypeName { get; private set; }

        #region IUnitTestElement Members

        public string ShortName
        {
            get { return myMethodName; }
        }

        public string Kind
        {
            get { return "Xunit Test"; }
        }

        public ICollection<IUnitTestElement> Children
        {
            get { return EmptyArray<IUnitTestElement>.Instance; }
        }

        public string ExplicitReason
        {
            get { return null; }
        }

        public bool Explicit
        {
            get { return true; }
        }

        public IEnumerable<UnitTestElementCategory> Categories
        {
            get { return UnitTestElementCategory.Uncategorized; }
        }

        public string Id
        {
            get { return string.Format("{0}.{1}", TypeName, myMethodName); }
        }

        public IUnitTestProvider Provider { get; private set; }

        public IUnitTestElement Parent
        {
            get { return null; }
            set { }
        }

        public UnitTestElementState State { get; set; }

        public string GetPresentation()
        {
            return myMethodName;
        }

        public IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            throw new InvalidOperationException("Test from abstract fixture is not runnable itself");
        }

        public IDeclaredElement GetDeclaredElement()
        {
            return null;
        }

        public IEnumerable<IProjectFile> GetProjectFiles()
        {
            throw new InvalidOperationException("Test from abstract fixture should not appear in Unit Test Explorer");
        }

        public bool Equals(IUnitTestElement other)
        {
            return this == other;
        }

        public UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(TypeName.GetNamespaceName());
        }

        public UnitTestElementDisposition GetDisposition()
        {
            IDeclaredElement element = GetDeclaredElement();
            if (element != null && element.IsValid())
            {
               var locations = (from declaration in element.GetDeclarations()
                                let file = declaration.GetContainingFile()
                                where file != null
                                select
                                new UnitTestElementLocation(file.GetSourceFile().ToProjectFile(), declaration.GetNameDocumentRange().TextRange, declaration.GetDocumentRange().TextRange))
                    .ToList();
                return new UnitTestElementDisposition(locations, this);
            }
            return UnitTestElementDisposition.InvalidDisposition;
        }

        public IProject GetProject()
        {
            return myProject;
        }

        #endregion
    }
}