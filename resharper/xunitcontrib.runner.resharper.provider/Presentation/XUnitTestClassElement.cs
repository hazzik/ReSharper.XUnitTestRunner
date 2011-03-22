namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
    using JetBrains.ReSharper.UnitTestFramework;

    internal class XUnitTestClassElement : XUnitRunnerTestClassElement, IUnitTestViewElement, IEquatable<XUnitTestClassElement>
    {
        private readonly IProject project;
        private readonly IProjectModelElementPointer projectPointer;

        public XUnitTestClassElement(IUnitTestRunnerProvider provider,
                                       IProject project,
                                       string typeName,
                                       string assemblyLocation)
            : base(provider, typeName, assemblyLocation)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            this.project = project;
            projectPointer = project.CreatePointer();
        }

        public bool Equals(XUnitTestClassElement other)
        {
            return Equals(other as XUnitRunnerTestClassElement);
        }

        public bool Equals(IUnitTestViewElement other)
        {
            return Equals(other as XUnitRunnerTestClassElement);
        }

        public IDeclaredElement GetDeclaredElement()
        {
            IProject project = GetProject();
            ISolution solution = project
                .GetSolution();

            IPsiModule primaryPsiModule = PsiModuleManager
                .GetInstance(solution)
                .GetPrimaryPsiModule(project);

            return CacheManager.GetInstance(PsiManager.GetInstance(solution).Solution)
                .GetDeclarationsCache(primaryPsiModule, false, true)
                .GetTypeElementByCLRName(TypeName);
        }

        public UnitTestElementDisposition GetDisposition()
        {
            IDeclaredElement element = GetDeclaredElement();
            if (element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            IEnumerable<UnitTestElementLocation> locations = from declaration in element.GetDeclarations()
                                                             let file = declaration.GetContainingFile()
                                                             where file != null
                                                             select
                                                                 new UnitTestElementLocation(file.GetSourceFile().ToProjectFile(),
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
            //return projectPointer.GetValidProjectElement(((XunitTestProvider)Provider).Solution) as IProject;
            return project;
        }

        public IProjectModelElementPointer GetProjectPointer()
        {
            return projectPointer;
        }

        public string GetTitle()
        {
            return ShortName;
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
            get { return "xUnit.net Test Class"; }
        }
    }
}
