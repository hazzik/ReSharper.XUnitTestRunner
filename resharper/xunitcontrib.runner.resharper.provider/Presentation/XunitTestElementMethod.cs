namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Application;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.Psi.Util;
    using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
    using JetBrains.ReSharper.UnitTestFramework;

    internal class XunitTestElementMethod : XUnitRunnerTestMethodElement, IUnitTestViewElement, IEquatable<XunitTestElementMethod>
    {
        private readonly IProject project;
        private readonly IProjectModelElementPointer projectPointer;

        internal XunitTestElementMethod(IUnitTestRunnerProvider provider,
                                        XUnitTestClassElement @class,
                                        IProject project,
                                        string declaringTypeName,
                                        string methodName,
                                        int order)
            : base(provider, @class, declaringTypeName, methodName)
        {
            this.project = project;
            projectPointer = project.CreatePointer();
        }

        public bool Equals(XunitTestElementMethod other)
        {
            return Equals(other as XUnitRunnerTestMethodElement);
        }

        public bool Equals(IUnitTestViewElement other)
        {
            return Equals(other as XUnitRunnerTestMethodElement);
        }

        public IDeclaredElement GetDeclaredElement()
        {
            ITypeElement declaredType = GetDeclaredType();
            if (declaredType != null)
            {
                return (from member in declaredType.EnumerateMembers(MethodName, true)
                        let method = member as IMethod
                        where method != null && !method.IsAbstract && method.TypeParameters.Length <= 0 && method.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC
                        select member).FirstOrDefault();
            }

            return null;
        }

        public UnitTestElementDisposition GetDisposition()
        {
            IDeclaredElement element = GetDeclaredElement();
            if (element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            IEnumerable<UnitTestElementLocation> locations = from declaration in element.GetDeclarations()
                                                             let file = declaration.GetContainingFile()
                                                             where file != null
                                                             select new UnitTestElementLocation(file.GetSourceFile().ToProjectFile(),
                                                                                                declaration.GetNameDocumentRange().TextRange,
                                                                                                declaration.GetDocumentRange().TextRange);

            return new UnitTestElementDisposition(locations.ToList(), this);
        }

        public UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(new ClrTypeName(TypeName).GetNamespaceName());
        }

        public IProject GetProject()
        {
            //return projectPointer.GetValidProjectElement(((XunitTestProvider) Provider).Solution) as IProject;
            return project;
        }

        public IProjectModelElementPointer GetProjectPointer()
        {
            return projectPointer;
        }

        public string GetTitle()
        {
            return string.Format("{0}.{1}", Class.GetTitle(), MethodName);
        }

        IEnumerable<UnitTestElementCategory> IUnitTestViewElement.Categories
        {
            get { return Categories; }
        }

        string IUnitTestViewElement.ExplicitReason
        {
            get { return ExplicitReason; }
        }

        public string Kind
        {
            get { return "xUnit.net Test"; }
        }

        private ITypeElement GetDeclaredType()
        {
            IProject project = GetProject();
            if (project == null)
            {
                return null;
            }
            PsiManager manager = PsiManager.GetInstance(project.GetSolution());
            using (ReadLockCookie.Create())
            {
                IPsiModule primaryPsiModule = PsiModuleManager.GetInstance(project.GetSolution()).GetPrimaryPsiModule(project);
                return CacheManager.GetInstance(manager.Solution)
                    .GetDeclarationsCache(primaryPsiModule, true, true)
                    .GetTypeElementByCLRName(TypeName);
            }
        }
    }
}
