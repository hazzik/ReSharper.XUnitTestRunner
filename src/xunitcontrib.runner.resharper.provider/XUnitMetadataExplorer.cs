namespace ReSharper.XUnitTestProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.Util;
    using Xunit.Sdk;

    public sealed class XunitMetadataExplorer
    {
        private readonly UnitTestElementConsumer consumer;
        private readonly IProject project;
        private readonly XunitTestProvider provider;
        private readonly ProjectModelElementEnvoy envoy;

        public XunitMetadataExplorer(XunitTestProvider provider, IProject project, UnitTestElementConsumer consumer)
        {
            this.provider = provider;
            this.project = project;
            this.consumer = consumer;
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
            XunitTestClassElement classUnitTestElement = provider.GetOrCreateClassElement(typeName, project, envoy);
            consumer(classUnitTestElement);

            foreach (IMethodInfo method in methods.Where(MethodUtility.IsTest))
            {
                ProcessTestMethod(classUnitTestElement, method);
            }
        }

        private void ProcessTestMethod(XunitTestClassElement classUnitTestElement, IMethodInfo method)
        {
            XunitTestMethodElement methodUnitTestElement = provider.GetOrCreateMethodElement(method.TypeName, method.Name, project, classUnitTestElement, envoy);
            methodUnitTestElement.ExplicitReason = MethodUtility.GetSkipReason(method);
            // TODO: Categories?
            consumer(methodUnitTestElement);
        }

        public void ExploreAssembly(IMetadataAssembly assembly)
        {
            //if (!assembly.ReferencedAssembliesNames.Any(reference => ((reference != null) && "xunit".Equals(reference.Name, StringComparison.InvariantCultureIgnoreCase))))
            // return;
            foreach (IMetadataTypeInfo metadataTypeInfo in GetExportedTypes(assembly.GetTypes()))
            {
                ProcessTypeInfo(metadataTypeInfo);
            }
        }

        private static IEnumerable<IMetadataTypeInfo> GetExportedTypes(IEnumerable<IMetadataTypeInfo> types)
        {
            foreach (IMetadataTypeInfo type in (types ?? Enumerable.Empty<IMetadataTypeInfo>()).Where(IsPublic))
            {
                foreach (IMetadataTypeInfo nestedType in GetExportedTypes(type.GetNestedTypes()))
                {
                    yield return nestedType;
                }

                yield return type;
            }
        }

        private static bool IsPublic(IMetadataTypeInfo type)
        {
            // Hmmm. This seems a little odd. Resharper reports public nested types with IsNestedPublic,
            // while IsPublic is false
            return (type.IsNested && type.IsNestedPublic) || type.IsPublic;
        }
    }
}
