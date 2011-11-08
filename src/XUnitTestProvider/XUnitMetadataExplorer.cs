namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.UnitTestFramework;
    using Xunit.Sdk;

    public sealed class XunitMetadataExplorer
    {
        private readonly UnitTestElementConsumer consumer;
        private readonly IProject project;
        private readonly XunitElementFactory factory;
        private readonly ProjectModelElementEnvoy envoy;

        public XunitMetadataExplorer([NotNull] XunitElementFactory factory, IProject project, UnitTestElementConsumer consumer)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            this.project = project;
            this.consumer = consumer;
            this.factory = factory;
            envoy = ProjectModelElementEnvoy.Create(project);
        }

        private void ProcessTypeInfo(IMetadataTypeInfo metadataTypeInfo)
        {
            ITypeInfo typeInfo = metadataTypeInfo.AsTypeInfo();
            // TODO: What about HasRunWith support? Not supported in previous R# versions
            if (!UnitTestElementIdentifier.IsUnitTestContainer(metadataTypeInfo))
                return;
            ITestClassCommand testClassCommand = TestClassCommandFactory.Make(typeInfo);
            if (testClassCommand == null)
                return;

            ProcessTestClass(metadataTypeInfo.FullyQualifiedName, testClassCommand.EnumerateTestMethods());
        }

        private void ProcessTestClass(string typeName, IEnumerable<IMethodInfo> methods)
        {
            XunitTestClassElement classUnitTestElement = factory.GetOrCreateClassElement(typeName, project, envoy);
            consumer(classUnitTestElement);

            foreach (IMethodInfo method in methods)
            {
                ProcessTestMethod(classUnitTestElement, method);
            }
        }

        private void ProcessTestMethod(XunitTestClassElement classUnitTestElement, IMethodInfo method)
        {
            XunitTestMethodElement methodUnitTestElement = factory.GetOrCreateMethodElement(method.TypeName, method.Name, project, classUnitTestElement, envoy);
            methodUnitTestElement.ExplicitReason = MethodUtility.GetSkipReason(method);
            // TODO: Categories?
            consumer(methodUnitTestElement);
        }

        public void ExploreAssembly(IMetadataAssembly assembly)
        {
            foreach (IMetadataTypeInfo metadataTypeInfo in GetExportedTypes(assembly.GetTypes()))
            {
                ProcessTypeInfo(metadataTypeInfo);
            }
        }

        private static IEnumerable<IMetadataTypeInfo> GetExportedTypes(IEnumerable<IMetadataTypeInfo> types)
        {
            foreach (IMetadataTypeInfo type in (types ?? Enumerable.Empty<IMetadataTypeInfo>()).Where(UnitTestElementIdentifier.IsPublic))
            {
                foreach (IMetadataTypeInfo nestedType in GetExportedTypes(type.GetNestedTypes()))
                {
                    yield return nestedType;
                }

                yield return type;
            }
        }
    }
}
