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

        public XunitTestClassElement GetOrCreateClassElement(IClrTypeName typeName, IProject project, ProjectModelElementEnvoy envoy, XunitTestClassElement parent)
        {
            var persistentTypeName = typeName.GetPersistent();
            var id = persistentTypeName.FullName;

            var element = GetElementById(project, id) as XunitTestClassElement ??
                          new XunitTestClassElement(provider, envoy, id, persistentTypeName, UnitTestManager.GetOutputAssemblyPath(project).FullPath);

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
                          new XunitTestMethodElement(provider, envoy, id, persistentTypeName, methodName);

            element.State = UnitTestElementState.Valid;
            element.Parent = parent;

            return element;
        }

        public IUnitTestElement CreateFakeElement(IProject project, IClrTypeName getClrName, string shortName)
        {
            return new XunitTestFakeElement(provider, project, getClrName.GetPersistent(), shortName);
        }

        public IUnitTestElement GetElementById(IProject project, string id)
        {
            return unitTestElementManager.GetElementById(project, id);
        }
    }
}