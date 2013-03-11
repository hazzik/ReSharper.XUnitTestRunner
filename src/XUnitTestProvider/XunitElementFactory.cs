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
        private readonly IUnitTestElementManager unitTestElementManager;
        private readonly XunitServiceProvider provider;

        public XunitElementFactory(XunitServiceProvider provider, IUnitTestElementManager unitTestElementManager)
        {
            this.unitTestElementManager = unitTestElementManager;
            this.provider = provider;
        }

        public XunitTestClassElement GetOrCreateClassElement(IClrTypeName typeName, IProject project, ProjectModelElementEnvoy envoy, XunitTestClassElement parent)
        {
            var persistentTypeName = typeName.GetPersistent();
            var id = persistentTypeName.FullName;

            var element = GetElementById(project, id) as XunitTestClassElement ??
                          new XunitTestClassElement(provider, envoy, id, persistentTypeName);

            element.State = UnitTestElementState.Valid;
            element.Parent = parent;
            element.AssemblyLocation = UnitTestManager.GetOutputAssemblyPath(project).FullPath;
            
            return element;
        }

        public XunitTestMethodElement GetOrCreateMethodElement(IClrTypeName typeName, string methodName, IProject project, XunitTestClassElement parent, ProjectModelElementEnvoy envoy, string explicitReason)
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
            element.ExplicitReason = explicitReason;

            return element;
        }

        public IUnitTestElement CreateFakeElement(IProject project, IClrTypeName getClrName, string shortName)
        {
            return new XunitTestFakeElement(provider.Provider, project, getClrName.GetPersistent(), shortName);
        }

        public IUnitTestElement GetElementById(IProject project, string id)
        {
            return unitTestElementManager.GetElementById(project, id);
        }
    }
}