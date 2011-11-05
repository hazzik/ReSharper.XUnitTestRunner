namespace ReSharper.XUnitTestProvider
{
    using JetBrains.ProjectModel;
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

        public XunitTestClassElement GetOrCreateClassElement(string typeName, IProject project, ProjectModelElementEnvoy envoy)
        {
            IUnitTestElement element = unitTestElementManager.GetElementById(project, typeName);
            if (element != null)
            {
                return (element as XunitTestClassElement);
            }

            return new XunitTestClassElement(provider, envoy, typeName, UnitTestManager.GetOutputAssemblyPath(project).FullPath);
        }

        public XunitTestMethodElement GetOrCreateMethodElement(string typeName, string methodName, IProject project, XunitTestClassElement parent, ProjectModelElementEnvoy envoy)
        {
            IUnitTestElement element = unitTestElementManager.GetElementById(project, string.Format("{0}.{1}", typeName, methodName));
            if (element != null)
            {
                var xunitTestMethodElement = element as XunitTestMethodElement;
                if (xunitTestMethodElement != null)
                {
                    xunitTestMethodElement.State = UnitTestElementState.Valid;
                }
                return xunitTestMethodElement;
            }
            return new XunitTestMethodElement(provider, parent, envoy, typeName, methodName);
        }
    }
}