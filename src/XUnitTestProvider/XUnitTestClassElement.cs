namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.TaskRunnerFramework;
    using JetBrains.ReSharper.UnitTestFramework;
    using XUnitTestRunner;

    public sealed class XunitTestClassElement : XunitTestElementBase, IEquatable<XunitTestClassElement>
    {
        private readonly ICollection<IUnitTestElement> children = new List<IUnitTestElement>();

        internal XunitTestClassElement(IUnitTestProvider provider, ProjectModelElementEnvoy project, string id, IClrTypeName typeName, string assemblyLocation, XunitTestClassElement parent)
            : base(provider, project, id, typeName, parent)
        {
            AssemblyLocation = assemblyLocation;
        }

        public override string Kind
        {
            get { return "xUnit.net Test Class"; }
        }

        public override string ShortName
        {
            get { return TypeName.ShortName; }
        }

        public string AssemblyLocation { get; private set; }

        public override ICollection<IUnitTestElement> Children
        {
            get { return children; }
        }

        #region IEquatable<XunitTestClassElement> Members

        public bool Equals(XunitTestClassElement other)
        {
            return other != null &&
                   Equals(Id, other.Id);
        }

        #endregion

        public override IDeclaredElement GetDeclaredElement()
        {
            IProject project = GetProject();
            if (project == null)
                return null;

            ISolution solution = project.GetSolution();
            
            IPsiModule primaryPsiModule = PsiModuleManager.GetInstance(solution).GetPrimaryPsiModule(project);
            
            return project.GetSolution().GetPsiServices().CacheManager
                .GetDeclarationsCache(primaryPsiModule, false, true)
                .GetTypeElementByCLRName(TypeName);
        }

        public override IEnumerable<IProjectFile> GetProjectFiles()
        {
            IDeclaredElement declaredElement = GetDeclaredElement();
            if (declaredElement == null)
            {
                return null;
            }

            return declaredElement
                .GetSourceFiles()
                .Select(sf => sf.ToProjectFile());
        }

        public override string GetPresentation()
        {
            return TypeName.ShortName;
        }

        public override IList<UnitTestTask> GetTaskSequence(IList<IUnitTestElement> explicitElements)
        {
            return new List<UnitTestTask>
                       {
                           new UnitTestTask(null, new AssemblyLoadTask(AssemblyLocation)),
                           new UnitTestTask(null, new XunitTestAssemblyTask(AssemblyLocation)),
                           new UnitTestTask(this, new XunitTestClassTask(AssemblyLocation, TypeName.FullName, explicitElements.Contains(this)))
                       };
        }

        public override bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitTestClassElement);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XunitTestClassElement);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override void WriteToXml(XmlElement xml)
        {
            xml.SetAttribute("TypeName", TypeName.FullName);
            IProject project = GetProject();
            if (project != null)
                xml.SetAttribute("Project", project.GetPersistentID());
        }

        public static IUnitTestElement ReadFromXml(XmlElement xml, IUnitTestElement parent, XunitElementFactory factory, ISolution solution)
        {
            var classElement = parent as XunitTestClassElement;
            IClrTypeName typeName = new ClrTypeName(xml.GetAttribute("TypeName"));
            string projectId = xml.GetAttribute("Project");
            var project = (IProject)ProjectUtil.FindProjectElementByPersistentID(solution, projectId);
            if (project == null)
                return null;
            return factory.GetOrCreateClassElement(typeName, project, ProjectModelElementEnvoy.Create(project), classElement);
        }

        public void AppendChild(IUnitTestElement element)
        {
            children.Add(element);
        }

        public void RemoveChild(IUnitTestElement element)
        {
            if (!children.Remove(element))
                throw new InvalidOperationException("No such element");
        }
    }
}
