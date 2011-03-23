using System;
using System.Drawing;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.CommonControls;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.TreeModels;
using JetBrains.UI.TreeView;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider.Properties;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [UnitTestProvider, UsedImplicitly]
    public class XunitTestProvider : XUnitTestRunnerProvider, IUnitTestProvider, IUnitTestPresenter
    {
        private static readonly XunitBrowserPresenter Presenter = new XunitBrowserPresenter();
        private static readonly AssemblyLoader AssemblyLoader = new AssemblyLoader();
        private static readonly UnitTestElementComparer comparer;
        private readonly ISolution solution;
        private readonly CacheManager cacheManager;

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
            comparer = new UnitTestElementComparer(new[] { typeof(XUnitTestMethodElement), typeof(XUnitTestClassElement) });
        }

        public XunitTestProvider(ISolution solution, 
            CacheManager cacheManager, 
            PsiModuleManager psiModuleManager, 
            UnitTestingCategoriesProvider categoriesProvider)
        {
            this.solution = solution;
            this.cacheManager = cacheManager;
        }

        public IUnitTestViewElement DeserializeElement(XmlElement parent, IUnitTestViewElement parentElement)
        {
            return null;
        }

        public Image Icon
        {
            get { return Resources.xunit; }
        }

        public ISolution Solution
        {
            get { return solution; }
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

        // It's rather useful to put a breakpoint here. When this gets hit, you can then attach
        // to the task runner process

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

        public bool IsElementOfKind(IUnitTestElement element, UnitTestElementKind elementKind)
        {
            switch (elementKind)
            {
                case UnitTestElementKind.Unknown:
                    return !(element is XUnitTestElementBase);

                case UnitTestElementKind.Test:
                    return element is XUnitTestMethodElement;

                case UnitTestElementKind.TestContainer:
                    return element is XUnitTestClassElement;

                case UnitTestElementKind.TestStuff:
                    return element is XUnitTestElementBase;
            }

            return false;
        }

        public void Present(IUnitTestViewElement element, IPresentableItem presentableItem, TreeModelNode node, PresentationState state)
        {
            Presenter.UpdateItem(element, node, presentableItem, state);
        }
    }
}
