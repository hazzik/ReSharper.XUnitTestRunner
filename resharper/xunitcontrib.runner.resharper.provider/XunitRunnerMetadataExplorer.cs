using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitRunnerMetadataExplorer
    {
        private readonly UnitTestElementConsumer consumer;
        private readonly IUnitTestRunnerProvider unitTestProvider;

        public XunitRunnerMetadataExplorer(IUnitTestRunnerProvider unitTestProvider, UnitTestElementConsumer consumer)
        {
            this.unitTestProvider = unitTestProvider;
            this.consumer = consumer;
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
            XunitRunnerTestClassElement classUnitTestElement = GetOrCreateClassElement(typeName, assemblyLocation);
            consumer(classUnitTestElement);

            foreach (IMethodInfo method in methods.Where(MethodUtility.IsTest))
            {
                ProcessTestMethod(classUnitTestElement, method);
            }
        }

        private void ProcessTestMethod(XunitRunnerTestClassElement classUnitTestElement, IMethodInfo method)
        {
            XunitRunnerTestMethodElement methodUnitTestElement = GetOrCreateMethodElement(classUnitTestElement, method.TypeName, method.Name);
            methodUnitTestElement.ExplicitReason = MethodUtility.GetSkipReason(method);
            // TODO: Categories?
            consumer(methodUnitTestElement);
        }

        protected virtual XunitRunnerTestMethodElement GetOrCreateMethodElement(XunitRunnerTestClassElement parent, string typeName, string methodName)
        {
            return new XunitRunnerTestMethodElement(unitTestProvider, parent, typeName, methodName);
        }

        protected virtual XunitRunnerTestClassElement GetOrCreateClassElement(string typeName, string assemblyLocation)
        {
            return new XunitRunnerTestClassElement(unitTestProvider, typeName, assemblyLocation);
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

        // ReSharper's IMetadataAssembly.GetExportedTypes always seems to return an empty list, so
        // let's roll our own. MSDN says that Assembly.GetExportTypes is looking for "The only types
        // visible outside an assembly are public types and public types nested within other public types."
        // TODO: It might be nice to randomise this list:
        // However, this returns items in alphabetical ordering. Assembly.GetExportedTypes returns back in
        // the order in which classes are compiled (so the order in which their files appear in the msbuild file!)
        // with dependencies appearing first. 
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
