using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ReSharper.TaskRunnerFramework;
    using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
    using JetBrains.Util;

    public class XunitTestRunnerProvider : IUnitTestRunnerProvider
    {
        public string ID
        {
            get { return XunitTaskRunner.RunnerId; }
        }

        public string Name
        {
            get { return "xUnit.net"; }
        }

        /// Provides Reflection-like metadata of a physical assembly, called at startup (if the
        /// assembly exists) and whenever the assembly is recompiled. It allows us to retrieve
        /// the tests that will actually get executed, as opposed to ExploreFile, which is about
        /// identifying tests as they are being written, and finding their location in the source
        /// code.
        /// It would be nice to check to see that the assembly references xunit before iterating
        /// through all the types in the assembly - a little optimisation. Unfortunately,
        /// when an assembly is compiled, only assemblies that have types that are directly
        /// referenced are embedded as references. In other words, if I use something from
        /// xunit.extensions, but not from xunit (say I only use a DerivedFactAttribute),
        /// then only xunit.extensions is listed as a referenced assembly. xunit will still
        /// get loaded at runtime, because it's a referenced assembly of xunit.extensions.
        /// It's also needed at compile time, but it's not a direct reference.
        /// So I'd need to recurse into the referenced assemblies references, and I don't
        /// quite know how to do that, and it's suddenly making our little optimisation
        /// rather complicated. So (at least for now) we'll leave well enough alone and
        /// just explore all the types
        public void ExploreAssembly(string assemblyLocation, UnitTestElementConsumer consumer)
        {
			var resolver = new DefaultAssemblyResolver(new FileSystemPath[0]);
			resolver.AddPath(new FileSystemPath(assemblyLocation).Directory);
			IMetadataAssembly assembly = new MetadataLoader(resolver).LoadFrom(new FileSystemPath(assemblyLocation), Predicate.True);
            //new XunitRunnerMetadataExplorer(this, consumer).ExploreAssembly(assembly);
            new XunitRunnerMetadataExplorer(this, consumer).ExploreAssembly(assembly);
		}

        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }
    }
}