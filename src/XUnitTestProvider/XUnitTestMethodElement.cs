namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Xml;
    using JetBrains.Util;
    using XUnitTestRunner;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Application;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.Psi.Util;
    using JetBrains.ReSharper.UnitTestFramework;

    public class XunitTestMethodElement : XunitTestElementBase, IEquatable<XunitTestMethodElement>
    {
        internal XunitTestMethodElement(IUnitTestProvider provider,
                                        XunitTestClassElement @class,
                                        ProjectModelElementEnvoy project,
                                        string typeName,
                                        string methodName)
            : base(provider, @class, project, typeName)
        {
            Class = @class;
            MethodName = methodName;
        }

        public XunitTestClassElement Class { get; private set; }
        public string MethodName { get; private set; }

        public override sealed string ShortName
        {
            get { return MethodName; }
        }

        public override sealed string Id
        {
            get { return string.Format("{0}.{1}", Class.TypeName, MethodName); }
        }

        public override string Kind
        {
            get { return "xUnit.net Test"; }
        }

        public bool Equals(XunitTestMethodElement other)
        {
            return (other != null && Equals(MethodName, other.MethodName)) && Equals(TypeName, other.TypeName);
        }

        public override sealed bool Equals(object obj)
        {
            return Equals(obj as XunitTestMethodElement);
        }

        public override sealed bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitTestMethodElement);
        }

        public override sealed int GetHashCode()
        {
            int result = 0;
            result = (result*397) ^ TypeName.GetHashCode();
            return ((result*397) ^ MethodName.GetHashCode());
        }

        public override IEnumerable<IProjectFile> GetProjectFiles()
        {
            ITypeElement declaredType = GetDeclaredType();
            if (declaredType != null)
            {
                List<IProjectFile> result = declaredType
                    .GetSourceFiles()
                    .Select(sf => sf.ToProjectFile())
                    .ToList();
                if (result.Count == 1)
                    return result;
            }
            IDeclaredElement declaredElement = GetDeclaredElement();
            if (declaredElement == null)
                return EmptyArray<IProjectFile>.Instance;
            return declaredElement
                .GetSourceFiles()
                .Select(sf => sf.ToProjectFile())
                .ToList();
        }

        public override sealed IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            var tasks = Class.GetTaskSequence(explicitElements);
            tasks.Add(new UnitTestTask(this, new XunitTestMethodTask(Class.AssemblyLocation, Class.TypeName, MethodName, explicitElements.Contains(this))));
            return tasks;
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

        public override IDeclaredElement GetDeclaredElement()
        {
            ITypeElement declaredType = GetDeclaredType();
            if (declaredType != null)
            {
                return (from member in declaredType.EnumerateMembers(MethodName, true)
                        let method = member as IMethod
                        where method != null && !method.IsAbstract && method.TypeParameters.Count <= 0 && method.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC
                        select member).FirstOrDefault();
            }

            return null;
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
            return string.Format("{0}.{1}", Class.ShortName, MethodName);
        }

        public override void WriteToXml(XmlElement parent)
        {
            parent.SetAttribute("MethodName", MethodName);
            IProject project = GetProject();
            if (project != null)
                parent.SetAttribute("Project", project.GetPersistentID());
        }

        public static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement, XunitTestProvider provider)
        {
            var testClassElement = parentElement as XunitTestClassElement;
            if (testClassElement == null)
                return null;
            string methodName = parent.GetAttribute("MethodName");
            string projectId = parent.GetAttribute("Project");
            var project = (IProject)ProjectUtil.FindProjectElementByPersistentID(provider.Solution, projectId);
            if (project == null)
                return null;
            return provider.GetOrCreateMethodElement(testClassElement.TypeName, methodName, project, testClassElement, ProjectModelElementEnvoy.Create(project));
        }
    }
}
