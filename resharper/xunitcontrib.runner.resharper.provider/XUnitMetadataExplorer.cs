namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.UnitTestFramework;
    using Xunit.Sdk;

    public class XunitMetadataExplorer
    {
        private readonly UnitTestElementConsumer consumer;
        private readonly IProject project;
        private readonly XunitTestProvider provider;

        public XunitMetadataExplorer(XunitTestProvider provider, IProject project, UnitTestElementConsumer consumer)
        {
            this.provider = provider;
            this.project = project;
            this.consumer = consumer;
        }

        protected virtual XunitTestClassElement GetOrCreateClassElement(string fullyQualifiedName)
        {
            return provider.GetOrCreateClassElement(fullyQualifiedName, project);
        }

        protected virtual XunitTestMethodElement GetOrCreateMethodElement(XunitTestClassElement testClass, string typeName, string methodName)
        {
            return provider.GetOrCreateMethodElement(typeName + "." + methodName, project, testClass);
        }

        private void ProcessTypeInfo(IMetadataAssembly assembly, IMetadataTypeInfo metadataTypeInfo)
        {
            ITypeInfo typeInfo = metadataTypeInfo.AsTypeInfo();
            // TODO: What about HasRunWith support? Not supported in previous R# versions
            if (!UnitTestElementIdentifier.IsUnitTestContainer(metadataTypeInfo))
                return;
            ITestClassCommand testClassCommand = TestClassCommandFactory.Make(typeInfo);
            if (testClassCommand == null)
                return;

            ProcessTestClass(metadataTypeInfo.FullyQualifiedName, testClassCommand.EnumerateTestMethods(), assembly.Location.FullPath);
        }

        private void ProcessTestClass(string typeName, IEnumerable<IMethodInfo> methods, string assemblyLocation)
        {
            XunitTestClassElement classUnitTestElement = GetOrCreateClassElement(typeName);
            consumer(classUnitTestElement);

            foreach (IMethodInfo method in methods.Where(MethodUtility.IsTest))
            {
                ProcessTestMethod(classUnitTestElement, method);
            }
        }

        private void ProcessTestMethod(XunitTestClassElement classUnitTestElement, IMethodInfo method)
        {
            XunitTestMethodElement methodUnitTestElement = GetOrCreateMethodElement(classUnitTestElement, method.TypeName, method.Name);
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
                ProcessTypeInfo(assembly, metadataTypeInfo);
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
