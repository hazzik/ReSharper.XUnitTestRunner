using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.CommonControls;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.UI;
using JetBrains.TreeModels;
using JetBrains.UI.TreeView;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider.Properties;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [UnitTestProvider, UsedImplicitly]
    public class XunitTestProvider : IUnitTestProvider
    {
        private static readonly XunitBrowserPresenter Presenter = new XunitBrowserPresenter();
        private static readonly AssemblyLoader AssemblyLoader = new AssemblyLoader();

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
            AssemblyLoader.RegisterAssembly(typeof(XunitTaskRunner).Assembly);
        }

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

        public int CompareUnitTestElements(UnitTestElement x, UnitTestElement y)
        {
            if (Equals(x, y))
                return 0;

            int compare = StringComparer.CurrentCultureIgnoreCase.Compare(x.GetTypeClrName(), y.GetTypeClrName());
            if (compare != 0)
                return compare;

            if (x is XunitTestElementMethod && y is XunitTestElementClass)
                return -1;

            if (x is XunitTestElementClass && y is XunitTestElementMethod)
                return 1;

            if (x is XunitTestElementClass && y is XunitTestElementClass)
                return 0;

            var xe = (XunitTestElementMethod)x;
            var ye = (XunitTestElementMethod)y;
            return xe.Order.CompareTo(ye.Order);
        }

        public UnitTestElement Deserialize(ISolution solution, string elementString)
        {
            return null;
        }

        // Provides Reflection-like metadata of a physical assembly, called at startup (if the
        // assembly exists) and whenever the assembly is recompiled. It allows us to retrieve
        // the tests that will actually get executed, as opposed to ExploreFile, which is about
        // identifying tests as they are being written, and finding their location in the source
        // code.
        // It would be nice to check to see that the assembly references xunit before iterating
        // through all the types in the assembly - a little optimisation. Unfortunately,
        // when an assembly is compiled, only assemblies that have types that are directly
        // referenced are embedded as references. In other words, if I use something from
        // xunit.extensions, but not from xunit (say I only use a DerivedFactAttribute),
        // then only xunit.extensions is listed as a referenced assembly. xunit will still
        // get loaded at runtime, because it's a referenced assembly of xunit.extensions.
        // It's also needed at compile time, but it's not a direct reference.
        // So I'd need to recurse into the referenced assemblies references, and I don't
        // quite know how to do that, and it's suddenly making our little optimisation
        // rather complicated. So (at least for now) we'll leave well enough alone and
        // just explore all the types
        public void ExploreAssembly(IMetadataAssembly assembly,
                                    IProject project,
                                    UnitTestElementConsumer consumer)
        {
            assembly.ProcessExportedTypes(new XunitAssemblyExplorer(this, assembly, project, consumer));
        }

        public ProviderCustomOptionsControl GetCustomOptionsControl(ISolution solution)
        {
            return null;
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

            psiFile.ProcessDescendants(new XunitFileExplorer(this, consumer, psiFile, interrupted));
        }

        public void ExploreSolution(ISolution solution, UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore the solution, without going into the projects
        }

        // It's rather useful to put a breakpoint here. When this gets hit, you can then attach
        // to the task runner process
        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }

        // Used to discover the type of the element - unknown, test, test container (class) or
        // something else relating to a test element (e.g. parent class of a nested test class)
        // This method is called to get the icon for the completion lists, amongst other things
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

        public bool IsElementOfKind(UnitTestElement element, UnitTestElementKind elementKind)
        {
            switch (elementKind)
            {
                case UnitTestElementKind.Unknown:
                    return !(element is XunitTestElement);

                case UnitTestElementKind.Test:
                    return element is XunitTestElementMethod;

                case UnitTestElementKind.TestContainer:
                    return element is XunitTestElementClass;

                case UnitTestElementKind.TestStuff:
                    return element is XunitTestElement;
            }

            return false;
        }

        public void Present(UnitTestElement element, IPresentableItem presentableItem, TreeModelNode node, PresentationState state)
        {
            Presenter.UpdateItem(element, node, presentableItem, state);
        }

        public string Serialize(UnitTestElement element)
        {
            return null;
        }
    }
}
