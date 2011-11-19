namespace ReSharper.XUnitTestProvider
{
    using System.Linq;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.ReSharper.UnitTestFramework.Elements;

    [SolutionComponent]
    public class XunitElementFactory
    {
        private readonly XunitTestProvider provider;
        private readonly IUnitTestElementManager unitTestElementManager;

        public XunitElementFactory(XunitTestProvider provider, IUnitTestElementManager unitTestElementManager)
        {
            this.provider = provider;
            this.unitTestElementManager = unitTestElementManager;
        }

        public XunitTestClassElement GetOrCreateClassElement(IClrTypeName typeName, IProject project, ProjectModelElementEnvoy envoy)
        {
            var id = typeName.FullName;

            IUnitTestElement element = unitTestElementManager.GetElementById(project, id);
            if (element == null)
            {
                return new XunitTestClassElement(provider, envoy, id, typeName, UnitTestManager.GetOutputAssemblyPath(project).FullPath);
            }

            var xunitTestClassElement = element as XunitTestClassElement;
            if (xunitTestClassElement == null)
            {
                return null;
            }

            xunitTestClassElement.State = UnitTestElementState.Valid;
            return xunitTestClassElement;
        }

        public XunitTestMethodElement GetOrCreateMethodElement(IClrTypeName typeName, string methodName, IProject project, XunitTestClassElement parent, ProjectModelElementEnvoy envoy)
        {
            var parts = new[]
                            {
                                parent.TypeName.FullName,
                                parent.TypeName.Equals(typeName) ? null : typeName.ShortName,
                                methodName
                            }
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            var id = string.Join(".", parts);

            IUnitTestElement element = unitTestElementManager.GetElementById(project, id);
            if (element == null)
            {
                return new XunitTestMethodElement(provider, envoy, id, typeName, parent, methodName);
            }

            var xunitTestMethodElement = element as XunitTestMethodElement;
            if (xunitTestMethodElement == null)
            {
                return null;
            }

            xunitTestMethodElement.State = UnitTestElementState.Valid;
            xunitTestMethodElement.Parent = parent;
            return xunitTestMethodElement;
        }
    }
}