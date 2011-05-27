using System;
using JetBrains.Util;

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
    using JetBrains.ReSharper.TaskRunnerFramework;
    using JetBrains.ReSharper.UnitTestFramework;
    using RemoteRunner;

    public class XunitTestMethodElement : XunitTestElementBase, IEquatable<XunitTestMethodElement>
    {
        internal XunitTestMethodElement(IUnitTestProvider provider,
                                        XunitTestClassElement @class,
                                        IProjectModelElement project,
                                        string declaringTypeName,
                                        string methodName)
            : base(provider, @class, project, declaringTypeName)
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
            XunitTestClassElement testClass = Class;

            return new List<UnitTestTask>
                       {
                           new UnitTestTask(null, CreateAssemblyTask(testClass.AssemblyLocation)),
                           new UnitTestTask(testClass, CreateClassTask(testClass, explicitElements)),
                           new UnitTestTask(this, CreateMethodTask(this, explicitElements))
                       };
        }

        private static RemoteTask CreateAssemblyTask(string assemblyLocation)
        {
            return new XunitTestAssemblyTask(assemblyLocation);
        }

        private static RemoteTask CreateClassTask(XunitTestClassElement testClass, IEnumerable<IUnitTestElement> explicitElements)
        {
            return new XunitTestClassTask(testClass.AssemblyLocation, testClass.TypeName, explicitElements.Contains(testClass));
        }

        private static RemoteTask CreateMethodTask(XunitTestMethodElement testMethod, IEnumerable<IUnitTestElement> explicitElements)
        {
            return new XunitTestMethodTask(testMethod.Class.AssemblyLocation, testMethod.Class.TypeName, testMethod.MethodName, explicitElements.Contains(testMethod));
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
    }
}
