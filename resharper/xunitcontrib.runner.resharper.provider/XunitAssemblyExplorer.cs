using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class XunitAssemblyExplorer : IMetadataTypeProcessor
    {
        private readonly IUnitTestProvider unitTestProvider;
        private readonly IMetadataAssembly assembly;
        private readonly IProject project;
        private readonly UnitTestElementConsumer consumer;
        private readonly CacheManager cacheManager;

        internal XunitAssemblyExplorer(IUnitTestProvider unitTestProvider, IMetadataAssembly assembly, IProject project,
                                       UnitTestElementConsumer consumer, CacheManager cacheManager)
        {
            this.unitTestProvider = unitTestProvider;
            this.assembly = assembly;
            this.project = project;
            this.consumer = consumer;
            this.cacheManager = cacheManager;
        }

        public void ProcessTypeInfo(IMetadataTypeInfo metadataTypeInfo)
        {
            var typeInfo = metadataTypeInfo.AsTypeInfo();
            if (UnitTestElementIdentifier.IsUnitTestContainer(metadataTypeInfo))    // TODO: What about HasRunWith support? Not supported in previous R# versions
            {
                var testClassCommand = TestClassCommandFactory.Make(typeInfo);
                if (testClassCommand == null)
                    return;

                ProcessTestClass(metadataTypeInfo.FullyQualifiedName, testClassCommand.EnumerateTestMethods());
            }
        }

        private void ProcessTestClass(string typeName, IEnumerable<IMethodInfo> methods)
        {
            var classUnitTestElement = new XunitTestElementClass(unitTestProvider, project, typeName, assembly.Location.FullPath, cacheManager);
            consumer(classUnitTestElement);

            var order = 1;
            foreach (var method in methods.Where(MethodUtility.IsTest))
            {
                ProcessTestMethod(classUnitTestElement, method, order++);
            }
        }

        private void ProcessTestMethod(XunitTestElementClass classUnitTestElement, IMethodInfo method, int order)
        {
            var methodUnitTestElement = new XunitTestElementMethod(unitTestProvider,
                                                                   classUnitTestElement, project,
                                                                   method.TypeName, method.Name,
                                                                   order)
                                            {
                                                ExplicitReason = MethodUtility.GetSkipReason(method)
                                            };
            // TODO: Categories?
            consumer(methodUnitTestElement);
        }
    }
}