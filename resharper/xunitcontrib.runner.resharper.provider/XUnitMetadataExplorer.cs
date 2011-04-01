using System;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitMetadataExplorer : XunitRunnerMetadataExplorer
    {
        private readonly IProject project;
        private readonly XunitTestProvider provider;

        public XunitMetadataExplorer(XunitTestProvider provider, IProject project, UnitTestElementConsumer consumer)
            : base(provider, consumer)
        {
            this.provider = provider;
            this.project = project;
        }

        protected override XunitRunnerTestClassElement GetOrCreateClassElement(string fullyQualifiedName, string assemblyLocation)
        {
            return provider.GetOrCreateClassElement(fullyQualifiedName, project);
        }

        protected override XunitRunnerTestMethodElement GetOrCreateMethodElement(XunitRunnerTestClassElement testClass, string typeName, string methodName)
        {
            return provider.GetOrCreateMethodElement(typeName + "." + methodName, project, testClass);
        }
    }
}
