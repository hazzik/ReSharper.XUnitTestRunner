using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class XunitAssemblyExplorer : IMetadataTypeProcessor
    {
        private readonly IUnitTestRunnerProvider unitTestProvider;
        private readonly IMetadataAssembly assembly;
        private readonly UnitTestElementConsumer consumer;

        internal XunitAssemblyExplorer(IUnitTestRunnerProvider unitTestProvider, 
            IMetadataAssembly assembly, 
            UnitTestElementConsumer consumer)
        {
            this.unitTestProvider = unitTestProvider;
            this.assembly = assembly;
            this.consumer = consumer;
        }

        public void ProcessTypeInfo(IMetadataTypeInfo metadataTypeInfo)
        {
            var typeInfo = metadataTypeInfo.AsTypeInfo();
            // TODO: What about HasRunWith support? Not supported in previous R# versions
            if (!UnitTestElementIdentifier.IsUnitTestContainer(metadataTypeInfo)) 
                return;
            var testClassCommand = TestClassCommandFactory.Make(typeInfo);
            if (testClassCommand == null)
                return;

            ProcessTestClass(metadataTypeInfo.FullyQualifiedName, testClassCommand.EnumerateTestMethods());
        }

        private void ProcessTestClass(string typeName, IEnumerable<IMethodInfo> methods)
        {
            var classUnitTestElement = new XUnitRunnerTestClassElement(unitTestProvider, typeName, assembly.Location.FullPath);
            consumer(classUnitTestElement);

            foreach (var method in methods.Where(MethodUtility.IsTest))
            {
                ProcessTestMethod(classUnitTestElement, method);
            }
        }

        private void ProcessTestMethod(XUnitRunnerTestClassElement classUnitTestElement, IMethodInfo method)
        {
            var methodUnitTestElement = new XUnitRunnerTestMethodElement(unitTestProvider,
                                                                         classUnitTestElement,
                                                                         method.TypeName, 
                                                                         method.Name)
                                            {
                                                ExplicitReason = MethodUtility.GetSkipReason(method)
                                            };
            // TODO: Categories?
            consumer(methodUnitTestElement);
        }
    }
}