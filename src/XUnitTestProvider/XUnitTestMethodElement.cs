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

        internal XunitTestMethodElement(XunitServiceProvider provider, ProjectModelElementEnvoy project, string id, IClrTypeName typeName, string methodName)
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

        public bool Equals(XunitTestMethodElement other)
        {
            return other != null &&
                   Equals(Id, other.Id);
        }

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
            var declaredType = GetDeclaredType();
            if (declaredType == null)
                return null;

            var result = declaredType
                .GetSourceFiles()
                .Select(sf => sf.ToProjectFile())
                .ToList();

            if (result.Count == 1)
                return result;

            var declaredMethod = FindDeclaredMethod(declaredType);
            if (declaredMethod == null)
                return null;

            return declaredMethod
                .GetSourceFiles()
                .Select(sf => sf.ToProjectFile())
                .ToList();
        }

        public override IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch)
        {
            var tasks = Parent.GetTaskSequence(explicitElements, launch);
            tasks.Add(new UnitTestTask(this, new XunitTestMethodTask(Parent.AssemblyLocation, Parent.TypeName.FullName, methodName, explicitElements.Contains(this))));
            return tasks;
        }

        private ITypeElement GetDeclaredType()
        {
            var project = GetProject();
            if (project == null)
                return null;

            using (ReadLockCookie.Create())
            {
                var primaryPsiModule = ServiceProvider.PsiModuleManager.GetPrimaryPsiModule(project);

                return ServiceProvider.CacheManager
                    .GetSymbolScope(primaryPsiModule, project.GetResolveContext(), true, true)
                    .GetTypeElementByCLRName(TypeName);
            }
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            var declaredType = GetDeclaredType();
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

        public override string GetPresentation(IUnitTestElement parent)
        {
            var fakeElement = parent as XunitTestFakeElement;
            if (Parent != null)
            {
                if (fakeElement == null)
                {
                    if (!Equals(Parent.TypeName, TypeName))
                    {
                        return string.Format("{0}.{1}", TypeName.ShortName, methodName);
                    }
                }
                else
                {
                    if (!Equals(Parent.TypeName, fakeElement.TypeName))
                    {
                        return string.Format("{0}.{1}", Parent.TypeName.ShortName, methodName);
                    }
                }
            }
            return methodName;
        }

        public override void WriteToXml(XmlElement xml)
        {
            xml.SetAttribute("MethodName", methodName);
            var project = GetProject();
            if (project != null)
                xml.SetAttribute("Project", project.GetPersistentID());
        }

        public static IUnitTestElement ReadFromXml(XmlElement xml, IUnitTestElement parent, XunitElementFactory factory, ISolution solution)
        {
            var classElement = parent as XunitTestClassElement;
            if (classElement == null)
                return null;
            var methodName = xml.GetAttribute("MethodName");
            var projectId = xml.GetAttribute("Project");
            var project = (IProject) ProjectUtil.FindProjectElementByPersistentID(solution, projectId);
            if (project == null)
                return null;
            return factory.GetOrCreateMethodElement(classElement.TypeName, methodName, project, classElement, ProjectModelElementEnvoy.Create(project), null);
        }
    }
}