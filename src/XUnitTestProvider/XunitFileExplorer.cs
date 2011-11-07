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
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.UnitTestFramework;
    using Xunit.Sdk;

    internal class XunitFileExplorer : IRecursiveElementProcessor
    {
        private readonly XunitElementFactory factory;
        private readonly UnitTestElementLocationConsumer consumer;
        private readonly CheckForInterrupt interrupted;
        private readonly IProject project;
        private readonly Dictionary<ITypeElement, IUnitTestElement> classes = new Dictionary<ITypeElement, IUnitTestElement>();
        private readonly IProjectFile projectFile;
        private readonly ProjectModelElementEnvoy envoy;

        public XunitFileExplorer([NotNull] XunitElementFactory factory, [NotNull] IFile file, UnitTestElementLocationConsumer consumer, CheckForInterrupt interrupted)
        {
            if (factory == null) 
                throw new ArgumentNullException("factory");
            if (file == null)
                throw new ArgumentNullException("file");

            this.consumer = consumer;
            this.interrupted = interrupted;
            this.factory = factory;
            this.projectFile = file.GetSourceFile().ToProjectFile();
            project = this.projectFile.GetProject();
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
            IUnitTestElement testElement;
            if (testClass == null || !IsValidTestClass(testClass) || !classes.TryGetValue(testClass, out testElement))
                return;

            foreach (var child in testElement.Children.Where(x => x.State == UnitTestElementState.Pending).ToList())
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
            if (testMethod != null)
                testElement = ProcessTestMethod(testMethod) ?? testElement;

            if (testElement == null)
                return;

            // Ensure that the method has been implemented, i.e. it has a name and a document
            var nameRange = declaration.GetNameDocumentRange().TextRange;
            var documentRange = declaration.GetDocumentRange().TextRange;
            if (nameRange.IsValid && documentRange.IsValid)
            {
                var disposition = new UnitTestElementDisposition(testElement, projectFile, nameRange, documentRange);
                consumer(disposition);
            }
        }

        private IUnitTestElement ProcessTestClass(IClass testClass)
        {
            if (!IsValidTestClass(testClass))
                return null;

            IUnitTestElement testElement;

            if (!classes.TryGetValue(testClass, out testElement))
            {
                testElement = factory.GetOrCreateClassElement(testClass.GetClrName().FullName, project, envoy);
                foreach (var child in testElement.Children.ToList())
                    child.State = UnitTestElementState.Pending;
                classes.Add(testClass, testElement);
            }

            return testElement;
        }

        private static bool IsValidTestClass(IClass testClass)
        {
            return UnitTestElementIdentifier.IsUnitTestContainer(testClass) && !HasUnsupportedRunWith(testClass.AsTypeInfo());
        }

        private static bool HasUnsupportedRunWith(ITypeInfo typeInfo)
        {
            return TypeUtility.HasRunWith(typeInfo);
        }

        private IUnitTestElement ProcessTestMethod(IMethod method)
        {
            var type = method.GetContainingType();
            var @class = type as IClass;
            if (type == null || @class == null || !IsValidTestClass(@class))
                return null;

            var command = TestClassCommandFactory.Make(@class.AsTypeInfo());
            if (command == null)
                return null;

            var classElement = classes[type];
            if (classElement == null)
                return null;

            if (command.IsTestMethod(method.AsMethodInfo()))
            {
                return factory.GetOrCreateMethodElement(type.GetClrName().FullName, method.ShortName, project, (XunitTestClassElement)classElement, envoy);
            }

            return null;
        }
    }
}