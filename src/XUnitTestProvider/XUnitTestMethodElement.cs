namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Xml;
    using JetBrains.ReSharper.Psi.Util;
    using XUnitTestRunner;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Application;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
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

        public override sealed IList<UnitTestTask> GetTaskSequence(IList<IUnitTestElement> explicitElements)
        {
            var tasks = Class.GetTaskSequence(explicitElements);
            tasks.Add(new UnitTestTask(this, new XunitTestMethodTask(Class.AssemblyLocation, Class.TypeName, MethodName, explicitElements.Contains(this))));
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
            var declaredType = GetDeclaredType();
            if (declaredType == null)
                return null;
            return FindDeclaredMethod(declaredType);
        }

        private IDeclaredElement FindDeclaredMethod(ITypeElement declaredType)
        {
            return declaredType.EnumerateMembers(MethodName, true)
                .OfType<IMethod>()
                .FirstOrDefault(method => !method.IsAbstract && method.TypeParameters.Count == 0 && method.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC);
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

        public static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement, XunitElementFactory factory, ISolution solution)
        {
            var testClassElement = parentElement as XunitTestClassElement;
            if (testClassElement == null)
                return null;
            string methodName = parent.GetAttribute("MethodName");
            string projectId = parent.GetAttribute("Project");
            var project = (IProject)ProjectUtil.FindProjectElementByPersistentID(solution, projectId);
            if (project == null)
                return null;
            return factory.GetOrCreateMethodElement(testClassElement.TypeName, methodName, project, testClassElement, ProjectModelElementEnvoy.Create(project));
        }
   }
}
