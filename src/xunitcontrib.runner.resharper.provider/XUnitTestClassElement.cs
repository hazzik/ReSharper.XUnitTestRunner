namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.TaskRunnerFramework;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.Util;
    using XUnitTestRunner;

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
            return new List<UnitTestTask>
                       {
                           new UnitTestTask(null, new AssemblyLoadTask(AssemblyLocation)),
                           new UnitTestTask(null, new XunitTestAssemblyTask(AssemblyLocation)),
                           new UnitTestTask(this, new XunitTestClassTask(AssemblyLocation, TypeName, explicitElements.Contains(this)))
                       };
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

        public override void WriteToXml(XmlElement parent)
        {
            parent.SetAttribute("TypeName", TypeName);
            IProject project = GetProject();
            if (project != null)
                parent.SetAttribute("Project", project.GetPersistentID());
        }

        public static IUnitTestElement ReadFromXml(XmlElement parent, XunitTestProvider provider)
        {
            string typeName = parent.GetAttribute("TypeName");
            string projectId = parent.GetAttribute("Project");
            var project = (IProject)ProjectUtil.FindProjectElementByPersistentID(provider.Solution, projectId);
            if (project == null)
                return null;
            return provider.GetOrCreateClassElement(typeName, project, ProjectModelElementEnvoy.Create(project));
        }
    }
}
