namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using JetBrains.Application;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Util;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.Util;
    using XUnitTestRunner;

    public sealed class XunitTestMethodElement : XunitTestElementBase, IEquatable<XunitTestMethodElement>
    {
        private readonly string methodName;

        internal XunitTestMethodElement(IUnitTestProvider provider, ProjectModelElementEnvoy project, string id, IClrTypeName typeName, string methodName)
            : base(provider, project, id, typeName)
        {
            this.methodName = methodName;
        }

        public override string ShortName
        {
            get { return methodName; }
        }

        public override string Kind
        {
            get { return "xUnit.net Test"; }
        }

        public override ICollection<IUnitTestElement> Children
        {
            get { return EmptyArray<IUnitTestElement>.Instance; }
        }

        #region IEquatable<XunitTestMethodElement> Members

        public bool Equals(XunitTestMethodElement other)
        {
            return other != null &&
                   Equals(Id, other.Id);
        }

        #endregion

        public override bool Equals(object obj)
        {
            return Equals(obj as XunitTestMethodElement);
        }

        public override bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitTestMethodElement);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override IEnumerable<IProjectFile> GetProjectFiles()
        {
            ITypeElement declaredType = GetDeclaredType();
            if (declaredType == null)
                return null;

            List<IProjectFile> result = declaredType
                .GetSourceFiles()
                .Select(sf => sf.ToProjectFile())
                .ToList();

            if (result.Count == 1)
                return result;

            IDeclaredElement declaredMethod = FindDeclaredMethod(declaredType);
            if (declaredMethod == null)
                return null;

            return declaredMethod
                .GetSourceFiles()
                .Select(sf => sf.ToProjectFile())
                .ToList();
        }

        public override IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            IList<UnitTestTask> tasks = Parent.GetTaskSequence(explicitElements);
            tasks.Add(new UnitTestTask(this, new XunitTestMethodTask(Parent.AssemblyLocation, Parent.TypeName.FullName, methodName, explicitElements.Contains(this))));
            return tasks;
        }

        private ITypeElement GetDeclaredType()
        {
            IProject project = GetProject();
            if (project == null)
                return null;

            using (ReadLockCookie.Create())
            {
                ISolution solution = project.GetSolution();

                IPsiModule primaryPsiModule = PsiModuleManager.GetInstance(solution).GetPrimaryPsiModule(project);

                return solution.GetPsiServices().CacheManager
                    .GetDeclarationsCache(primaryPsiModule, true, true)
                    .GetTypeElementByCLRName(TypeName);
            }
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            ITypeElement declaredType = GetDeclaredType();
            if (declaredType == null)
                return null;
            return FindDeclaredMethod(declaredType);
        }

        private IDeclaredElement FindDeclaredMethod(ITypeElement declaredType)
        {
            return declaredType.EnumerateMembers(methodName, true)
                .OfType<IMethod>()
                .FirstOrDefault(method => !method.IsAbstract && method.TypeParameters.Count == 0 && method.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC);
        }

        public override string GetPresentation()
        {
            return Parent != null && !Equals(Parent.TypeName, TypeName)
                       ? string.Format("{0}.{1}", TypeName.ShortName, methodName)
                       : methodName;
        }

        public string GetPresentation(IUnitTestElement parent)
        {
            var fakeElement = parent as XunitTestFakeElement;
            if (fakeElement == null)
                return GetPresentation();

            return Parent != null && !Equals(Parent.TypeName, fakeElement.TypeName)
                       ? string.Format("{0}.{1}", Parent.TypeName.ShortName, methodName)
                       : methodName;
        }

        public override void WriteToXml(XmlElement xml)
        {
            xml.SetAttribute("MethodName", methodName);
            IProject project = GetProject();
            if (project != null)
                xml.SetAttribute("Project", project.GetPersistentID());
        }

        public static IUnitTestElement ReadFromXml(XmlElement xml, IUnitTestElement parent, XunitTestProvider provider, ISolution solution)
        {
            var classElement = parent as XunitTestClassElement;
            if (classElement == null)
                return null;
            string methodName = xml.GetAttribute("MethodName");
            string projectId = xml.GetAttribute("Project");
            var project = (IProject) ProjectUtil.FindProjectElementByPersistentID(solution, projectId);
            if (project == null)
                return null;
            return provider.GetOrCreateMethodElement(classElement.TypeName, methodName, project, classElement, ProjectModelElementEnvoy.Create(project));
        }
    }
}