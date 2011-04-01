using System;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [MetadataUnitTestExplorer]
    public class XunitTestMetadataExplorer : IUnitTestMetadataExplorer
    {
        private readonly XunitTestProvider provider;

        public XunitTestMetadataExplorer(XunitTestProvider provider)
        {
            this.provider = provider;
        }

        public void ExploreAssembly(IProject project, IMetadataAssembly assembly, UnitTestElementConsumer consumer)
        {
            new XunitMetadataExplorer(provider, project, consumer).ExploreAssembly(assembly);
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
        }
    }
}
