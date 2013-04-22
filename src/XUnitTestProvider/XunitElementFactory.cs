namespace ReSharper.XUnitTestProvider
{
    using System.Text;
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
            var id = MakeId(project.GetPersistentID(),
                            persistentTypeName.FullName);

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
            var id = MakeId(project.GetPersistentID(),
                            parent.TypeName.FullName,
                            parent.TypeName.Equals(persistentTypeName) ? null : persistentTypeName.ShortName,
                            methodName);

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

        private static string MakeId(params string[] parts)
        {
            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                if (sb.Length > 0)
                    sb.Append('.');
                if (!string.IsNullOrEmpty(part))
                    sb.Append(part);
            }
            return sb.ToString();
        }
    }
}