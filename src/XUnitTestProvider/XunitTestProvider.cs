namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Xml;
    using JetBrains.Annotations;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.TaskRunnerFramework;
    using JetBrains.ReSharper.UnitTestFramework;
    using Properties;
    using XUnitTestRunner;
    using JetBrains.Util;

    [UnitTestProvider]
    [UsedImplicitly]
    public class XunitTestProvider : IUnitTestProvider
    {
        private static readonly AssemblyLoader AssemblyLoader = new AssemblyLoader();
        private static readonly UnitTestElementComparer Comparer;
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
            Comparer = new UnitTestElementComparer(new[] {typeof (XunitTestMethodElement), typeof (XunitTestClassElement)});
        }

        public XunitTestProvider(ISolution solution,
                                 CacheManager cacheManager,
                                 PsiModuleManager psiModuleManager,
                                 UnitTestingCategoriesProvider categoriesProvider)
        {
            this.solution = solution;
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

        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
#if DEBUG
            // Causes the external test runner to display a message box before running, very handy for attaching the debugger
            // and while it's a bit crufty here, we know this method gets called before a test run
//            UnitTestManager.GetInstance(Solution).EnableDebugInternal = true;
#endif
            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }

        public IUnitTestElement DeserializeElement(XmlElement parent, IUnitTestElement parentElement)
        {
            if (!parent.HasAttribute("type"))
                throw new ArgumentException(@"Element is not Xunit", "parent");
            switch (parent.GetAttribute("type"))
            {
                case "XunitTestClassElement":
                    return XunitTestClassElement.ReadFromXml(parent, this);
                case "XunitTestMethodElement":
                    return XunitTestMethodElement.ReadFromXml(parent, parentElement, this);
                default:
                    throw new ArgumentException(@"Element is not Xunit", "parent");
            }
        }

        public bool IsSupported(IHostProvider hostProvider)
        {
            return true;
        }

        public int CompareUnitTestElements(IUnitTestElement x, IUnitTestElement y)
        {
            return Comparer.Compare(x, y);
        }

        public void SerializeElement(XmlElement parent, IUnitTestElement element)
        {
            parent.SetAttribute("type", element.GetType().Name);
            
            var testElement = element as XunitTestElementBase;
            if (testElement == null)
                throw new ArgumentException(string.Format("Element {0} is not MSTest", element.GetType()), "element");
            
            testElement.WriteToXml(parent);
        }

        public void ExploreExternal(UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore anything that's not a part of the solution + projects world
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

        public XunitTestClassElement GetOrCreateClassElement(string typeName, IProject project, ProjectModelElementEnvoy envoy)
        {
            IUnitTestElement element = UnitTestManager.GetInstance(Solution).GetElementById(project, typeName);
            if (element != null)
            {
                return (element as XunitTestClassElement);
            }

            return new XunitTestClassElement(this, envoy, typeName, UnitTestManager.GetOutputAssemblyPath(project).FullPath);
        }

        public XunitTestMethodElement GetOrCreateMethodElement(string typeName, string methodName, IProject project, XunitTestClassElement parent, ProjectModelElementEnvoy envoy)
        {
            IUnitTestElement element = UnitTestManager.GetInstance(Solution).GetElementById(project, string.Format("{0}.{1}", typeName, methodName));
            if (element != null)
            {
                var xunitTestMethodElement = element as XunitTestMethodElement;
                if (xunitTestMethodElement != null)
                {
                    xunitTestMethodElement.State = UnitTestElementState.Valid;
                }
                return xunitTestMethodElement;
            }
            return new XunitTestMethodElement(this, parent, envoy, typeName, methodName);
        }
    }
}
