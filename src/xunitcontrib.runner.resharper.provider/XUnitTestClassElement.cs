namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.Util;

    public class XunitTestClassElement : XunitTestElementBase, IEquatable<XunitTestClassElement>
    {
        public XunitTestClassElement(IUnitTestProvider provider,
                                     ProjectModelElementEnvoy project,
                                     string typeName,
                                     string assemblyLocation)
            : base(provider, null, project, typeName)
        {
            AssemblyLocation = assemblyLocation;
        }

        public override string Kind
        {
            get { return "xUnit.net Test Class"; }
        }

        public override string Id
        {
            get { return TypeName; }
        }

        public override string ShortName
        {
            get { return TypeName.Split('.').Last(); }
        }

        public string AssemblyLocation { get; private set; }

        #region IEquatable<XunitTestClassElement> Members

        public bool Equals(XunitTestClassElement other)
        {
            return ((other != null) && Equals(TypeName, other.TypeName));
        }

        #endregion

        public override IDeclaredElement GetDeclaredElement()
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

        public override IEnumerable<IProjectFile> GetProjectFiles()
        {
            IDeclaredElement declaredElement = GetDeclaredElement();
            if (declaredElement == null)
            {
                return EmptyArray<IProjectFile>.Instance;
            }
            return declaredElement.GetSourceFiles().Select(sf => sf.ToProjectFile());
        }

        public override UnitTestElementDisposition GetDisposition()
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

        public override string GetPresentation()
        {
            return ShortName;
        }

        public override sealed IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            // We don't have to do anything explicit for a test class, because when a class is run
            // we get called for each method, and each method already adds everything we need (loading
            // the assembly and the class)
            return EmptyArray<UnitTestTask>.Instance;
        }

        public override sealed bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitTestClassElement);
        }

        public override sealed bool Equals(object obj)
        {
            return Equals(obj as XunitTestClassElement);
        }

        public override sealed int GetHashCode()
        {
            return TypeName.GetHashCode();
        }
    }
}
