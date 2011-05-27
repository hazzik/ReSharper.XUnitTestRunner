using System;
using System.Drawing;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider.Properties;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using JetBrains.Metadata.Reader.API;
    using JetBrains.Util;

    [UnitTestProvider]
    [UsedImplicitly]
    public class XunitTestProvider : IUnitTestProvider
    {
        private static readonly AssemblyLoader AssemblyLoader = new AssemblyLoader();
        private static readonly UnitTestElementComparer comparer;
        private readonly CacheManager cacheManager;
        private readonly ISolution solution;

        static XunitTestProvider()
        {
            // ReSharper automatically adds all test providers to the list of assemblies it uses
            // to handle the AppDomain.Resolve event

            // The test runner process talks to devenv/resharper via remoting, and so devenv needs
            // to be able to resolve the remote assembly to be able to recreate the serialised types.
            // (Aside: the assembly is already loaded - why does it need to resolve it again?)
            // ReSharper automatically adds all unit test provider assemblies to the list of assemblies
            // it uses to handle the AppDomain.Resolve event. Since we've got a second assembly to
            // live in the remote process, we need to add this to the list.
            AssemblyLoader.RegisterAssembly(typeof (XunitTaskRunner).Assembly);
            comparer = new UnitTestElementComparer(new[] {typeof (XunitTestMethodElement), typeof (XunitTestClassElement)});
        }

        public XunitTestProvider(ISolution solution,
                                 CacheManager cacheManager,
                                 PsiModuleManager psiModuleManager,
                                 UnitTestingCategoriesProvider categoriesProvider)
        {
            this.solution = solution;
            this.cacheManager = cacheManager;
        }

        #region IUnitTestProvider Members

        // It's rather useful to put a breakpoint here. When this gets hit, you can then attach
        // to the task runner process

        // Used to discover the type of the element - unknown, test, test container (class) or
        // something else relating to a test element (e.g. parent class of a nested test class)
        // This method is called to get the icon for the completion lists, amongst other things

        #endregion

        public string ID
        {
            get { return XunitTaskRunner.RunnerId; }
        }

        public string Name
        {
            get { return "xUnit.net"; }
        }

        public Image Icon
        {
            get { return Resources.xunit; }
        }

        public ISolution Solution
        {
            get { return solution; }
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
            new XunitMetadataExplorer(this, null, consumer).ExploreAssembly(assembly);
        }

        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }

        public IUnitTestElement DeserializeElement(XmlElement parent, IUnitTestElement parentElement)
        {
            return null;
        }

        public bool IsSupported(IHostProvider hostProvider)
        {
            return true;
        }

        public int CompareUnitTestElements(IUnitTestElement x, IUnitTestElement y)
        {
            return comparer.Compare(x, y);
        }

        public void SerializeElement(XmlElement parent, IUnitTestElement element)
        {
        }

        public void ExploreExternal(UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore anything that's not a part of the solution + projects world
        }

        public void ExploreFile(IFile psiFile,
                                UnitTestElementLocationConsumer consumer,
                                CheckForInterrupt interrupted)
        {
            if (psiFile == null)
                throw new ArgumentNullException("psiFile");

            psiFile.ProcessDescendants(new XunitFileExplorer(this, consumer, psiFile, interrupted, cacheManager));
        }

        public void ExploreSolution(ISolution solution, UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore the solution, without going into the projects
        }

        public bool IsElementOfKind(IDeclaredElement declaredElement, UnitTestElementKind elementKind)
        {
            switch (elementKind)
            {
                case UnitTestElementKind.Unknown:
                    return !UnitTestElementIdentifier.IsAnyUnitTestElement(declaredElement);

                case UnitTestElementKind.Test:
                    return UnitTestElementIdentifier.IsUnitTest(declaredElement);

                case UnitTestElementKind.TestContainer:
                    return UnitTestElementIdentifier.IsUnitTestContainer(declaredElement);

                case UnitTestElementKind.TestStuff:
                    return UnitTestElementIdentifier.IsUnitTestStuff(declaredElement);
            }

            return false;
        }

        public bool IsElementOfKind(IUnitTestElement element, UnitTestElementKind elementKind)
        {
            switch (elementKind)
            {
                case UnitTestElementKind.Unknown:
                    return !(element is XunitTestElementBase);

                case UnitTestElementKind.Test:
                    return element is XunitTestMethodElement;

                case UnitTestElementKind.TestContainer:
                    return element is XunitTestClassElement;

                case UnitTestElementKind.TestStuff:
                    return element is XunitTestElementBase;
            }

            return false;
        }

        public XunitTestClassElement GetOrCreateClassElement(string id, IProject project)
        {
            IUnitTestElement element = UnitTestManager.GetInstance(Solution).GetElementById(project, id);
            if (element != null)
            {
                element.State = UnitTestElementState.Valid;
                return (element as XunitTestClassElement);
            }

            return new XunitTestClassElement(this, project, id, UnitTestManager.GetOutputAssemblyPath(project).FullPath);
        }

        public XunitTestMethodElement GetOrCreateMethodElement(string id, IProject project, XunitTestClassElement parent)
        {
            IUnitTestElement element = UnitTestManager.GetInstance(Solution).GetElementById(project, id);
            if (element != null)
            {
                element.State = UnitTestElementState.Valid;
                return (element as XunitTestMethodElement);
            }
            string[] splitted = id.Split('.');
            string declaringTypeName = StringUtil.Join(splitted.Take((splitted.Length - 1)), ".");
            string name = splitted.Last();
            return new XunitTestMethodElement(this, parent, project, declaringTypeName, name);
        }
    }
}
