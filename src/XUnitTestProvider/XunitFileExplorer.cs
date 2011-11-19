namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using JetBrains.Application;
    using JetBrains.Application.Progress;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Search;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.Psi.Util;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.Util;
    using Xunit.Sdk;

    internal class XunitFileExplorer : IRecursiveElementProcessor
    {
        private readonly XunitElementFactory factory;
        private readonly UnitTestElementLocationConsumer consumer;
        private readonly CheckForInterrupt interrupted;
        private readonly IProject project;
        private readonly Dictionary<ITypeElement, IList<XunitTestClassElement>> classes = new Dictionary<ITypeElement, IList<XunitTestClassElement>>();
        private readonly IProjectFile projectFile;
        private readonly ProjectModelElementEnvoy envoy;
        private readonly SearchDomainFactory searchDomainFactory;

        public XunitFileExplorer([NotNull] XunitElementFactory factory, [NotNull] ITreeNode file, [NotNull] SearchDomainFactory searchDomainFactory, UnitTestElementLocationConsumer consumer, CheckForInterrupt interrupted)
        {
            if (factory == null) 
                throw new ArgumentNullException("factory");
            if (searchDomainFactory == null) 
                throw new ArgumentNullException("searchDomainFactory");
            if (file == null)
                throw new ArgumentNullException("file");

            this.factory = factory;
            this.searchDomainFactory = searchDomainFactory;
            this.consumer = consumer;
            this.interrupted = interrupted;

            projectFile = file.GetSourceFile().ToProjectFile();
            if (projectFile != null) project = projectFile.GetProject();
            envoy = ProjectModelElementEnvoy.Create(project);
        }

        public bool ProcessingIsFinished
        {
            get
            {
                if (interrupted())
                    throw new ProcessCancelledException();

                return false;
            }
        }

        public bool InteriorShouldBeProcessed(ITreeNode element)
        {
            return !(element is ITypeMemberDeclaration) || element is ITypeDeclaration;
        }

        public void ProcessAfterInterior(ITreeNode element)
        {
            var declaration = element as IDeclaration;

            if (declaration == null)
                return;

            var testClass = declaration.DeclaredElement as IClass;
            IList<XunitTestClassElement> classElements;
            bool isAbstract;
            if (testClass == null || !IsValidTestClass(testClass, out isAbstract) || !classes.TryGetValue(testClass, out classElements))
                return;

            foreach (var child in classElements.SelectMany(classElement => classElement.Children.Where(x => x.State == UnitTestElementState.Pending).ToList()))
                child.State = UnitTestElementState.Invalid;
        }

        public void ProcessBeforeInterior(ITreeNode element)
        {
            var declaration = element as IDeclaration;

            if (declaration == null)
                return;

            IUnitTestElement testElement = null;
            var declaredElement = declaration.DeclaredElement;

            var testClass = declaredElement as IClass;
            if (testClass != null)
                testElement = ProcessTestClass(testClass);

            var testMethod = declaredElement as IMethod;
            IList<IUnitTestElement> rowTests = null;
            if (testMethod != null)
                testElement = ProcessTestMethod(testMethod, out rowTests) ?? testElement;

            if (testElement == null)
                return;

            // Ensure that the method has been implemented, i.e. it has a name and a document
            var nameRange = declaration.GetNameDocumentRange().TextRange;
            var documentRange = declaration.GetDocumentRange().TextRange;
            if (nameRange.IsValid && documentRange.IsValid)
            {
                var disposition = new UnitTestElementDisposition(testElement, projectFile, nameRange, documentRange, rowTests);
                consumer(disposition);
            }
        }

        private IUnitTestElement ProcessTestClass(IClass testClass)
        {
            bool isAbstract;
            if (!IsValidTestClass(testClass, out isAbstract))
                return null;
            
            if (isAbstract)
            {
                ProcessAbstractClass(testClass);
                return null;
            }

            IList<XunitTestClassElement> testElements;
            XunitTestClassElement testElement;
            if (!classes.TryGetValue(testClass, out testElements))
            {
                testElement = factory.GetOrCreateClassElement(testClass.GetClrName(), project, envoy);
                classes.Add(testClass, new List<XunitTestClassElement> {testElement});
            }
            else
            {
                testElement = testElements.First();
            }
            
            foreach (var child in ChildrenInThisFile(testElement))
                child.State = UnitTestElementState.Pending;

            foreach (IDeclaredType type in testClass.GetAllSuperTypes())
                ProcessSuperType(testElement, type);

            return testElement;
        }

        private void ProcessSuperType(XunitTestClassElement classElement, IDeclaredType type)
        {
            ITypeElement @class = type.GetTypeElement() as IClass;
            if (@class == null)
                return;

            foreach (IMethod method in @class.GetMembers().Where(UnitTestElementIdentifier.IsUnitTest))
                GetOrCreateMethodElement(classElement, @class, method);
        }

        private XunitTestMethodElement GetOrCreateMethodElement(XunitTestClassElement classElement, ITypeElement @class, IDeclaredElement method)
        {
            var projectElement = classElement.GetProject();
            var projectEnvoy = Equals(projectElement, project)
                                   ? envoy
                                   : ProjectModelElementEnvoy.Create(projectElement);
            return factory.GetOrCreateMethodElement(@class.GetClrName(), method.ShortName, projectElement, classElement, projectEnvoy);
        }

        private IEnumerable<IUnitTestElement> ChildrenInThisFile(IUnitTestElement testElement)
        {
            return from element in testElement.Children
                   let projectFiles = element.GetProjectFiles()
                   where projectFiles == null || projectFiles.IsEmpty() || projectFiles.Contains(projectFile)
                   select element;
        }

        private static bool IsValidTestClass([NotNull] IClass @class, out bool isAbstract)
        {
            isAbstract = false;
            
            var typeInfo = @class.AsTypeInfo();
            if (!UnitTestElementIdentifier.IsPublic(@class) ||
                !TypeUtility.ContainsTestMethods(typeInfo) ||
                TypeUtility.HasRunWith(typeInfo))
            {
                return false;
            }


            if (!TypeUtility.IsStatic(typeInfo) && TypeUtility.IsAbstract(typeInfo))
            {
                isAbstract = true;
            }
            return true;
        }

        private IUnitTestElement ProcessTestMethod(IMethod method, out IList<IUnitTestElement> rowTests)
        {
            rowTests = null;
            var @class = method.GetContainingType() as IClass;
            bool isAbstract;
            if (@class == null || !IsValidTestClass(@class, out isAbstract))
                return null;

            var elements = classes[@class];
            if (elements.Count == 1 && !isAbstract)
            {
                var classElement = elements.First();
                if (classElement == null)
                    return null;

                if (UnitTestElementIdentifier.IsUnitTest(method))
                    return factory.GetOrCreateMethodElement(@class.GetClrName(), method.ShortName, project, classElement, envoy);

                return null;
            }
            rowTests = elements.Select(classElement => GetOrCreateMethodElement(classElement, @class, method))
                .Cast<IUnitTestElement>()
                .ToList();

            return factory.CreateFakeElement(project, @class.GetClrName(), method.ShortName);
        }

        private void ProcessAbstractClass(ITypeElement typeElement)
        {
            ISolution solution = typeElement.GetSolution();
            var inheritorsConsumer = new XunitFileExplorerInheritorsConsumer();
            var fixtures = new List<XunitTestClassElement>();
            solution.GetPsiServices().Finder.FindInheritors(typeElement, searchDomainFactory.CreateSearchDomain(solution, true), inheritorsConsumer, NullProgressIndicator.Instance);
            foreach (var inheritor in inheritorsConsumer.FoundElements)
            {
                IProject projectElement = project;
                ProjectModelElementEnvoy projectEnvoy = envoy;
                var declaration = inheritor.GetDeclarations().FirstOrDefault();
                if (declaration != null)
                {
                    projectElement = declaration.GetProject();
                    if (!Equals(projectElement, project))
                        projectEnvoy = ProjectModelElementEnvoy.Create(projectElement);
                }
                XunitTestClassElement fixtureElement = factory.GetOrCreateClassElement(inheritor.GetClrName(), projectElement, projectEnvoy);
                fixtures.Add(fixtureElement);
                foreach (IDeclaredType type in inheritor.GetAllSuperTypes())
                    ProcessSuperType(fixtureElement, type);
            }
            classes.Add(typeElement, fixtures);
        }
    }
}