namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Xml;
    using JetBrains.Annotations;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.TaskRunnerFramework;
    using JetBrains.ReSharper.UnitTestFramework;
    using Properties;
    using XUnitTestRunner;

    [UnitTestProvider, UsedImplicitly]
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

        public XunitTestProvider(ISolution solution)
        {
            this.solution = solution;
        }

        public string ID
        {
            get { return XunitTaskRunner.RunnerId; }
        }

        public string Name
        {
            get { return "xUnit.net (hazzik)"; }
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
            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }

        public IUnitTestElement DeserializeElement(XmlElement xml, IUnitTestElement parent)
        {
            if (!xml.HasAttribute("type"))
                throw new ArgumentException(@"Element is not Xunit", "xml");
            switch (xml.GetAttribute("type"))
            {
                case "XunitTestClassElement":
                    return XunitTestClassElement.ReadFromXml(xml, parent, this, solution);
                case "XunitTestMethodElement":
                    return XunitTestMethodElement.ReadFromXml(xml, parent, this, solution);
                default:
                    throw new ArgumentException(@"Element is not Xunit", "xml");
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

        public void SerializeElement(XmlElement xml, IUnitTestElement parent)
        {
            xml.SetAttribute("type", parent.GetType().Name);
            
            var testElement = parent as XunitTestElementBase;
            if (testElement == null)
                throw new ArgumentException(string.Format("Element {0} is not MSTest", parent.GetType()), "parent");
            
            testElement.WriteToXml(xml);
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

        public XunitTestClassElement GetOrCreateClassElement(IClrTypeName typeName, IProject project, ProjectModelElementEnvoy envoy, XunitTestClassElement parent)
        {
            var persistentTypeName = typeName.GetPersistent();
            var id = persistentTypeName.FullName;

            var element = GetElementById(project, id) as XunitTestClassElement ??
                          new XunitTestClassElement(this, envoy, id, persistentTypeName, UnitTestManager.GetOutputAssemblyPath(project).FullPath);

            element.State = UnitTestElementState.Valid;
            element.Parent = parent;
            
            return element;
        }

        public XunitTestMethodElement GetOrCreateMethodElement(IClrTypeName typeName, string methodName, IProject project, XunitTestClassElement parent, ProjectModelElementEnvoy envoy)
        {
            var persistentTypeName = typeName.GetPersistent();
            var parts = new[]
                            {
                                parent.TypeName.FullName,
                                parent.TypeName.Equals(persistentTypeName) ? null : persistentTypeName.ShortName,
                                methodName
                            }
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            var id = string.Join(".", parts);

            var element = GetElementById(project, id) as XunitTestMethodElement ??
                          new XunitTestMethodElement(this, envoy, id, persistentTypeName, methodName);

            element.State = UnitTestElementState.Valid;
            element.Parent = parent;

            return element;
        }

        public IUnitTestElement CreateFakeElement(IProject project, IClrTypeName getClrName, string shortName)
        {
            return new XunitTestFakeElement(this, project, getClrName.GetPersistent(), shortName);
        }

        public IUnitTestElement GetElementById(IProject project, string id)
        {
            return UnitTestManager.GetInstance(Solution).GetElementById(project, id);
        }
    }
}
