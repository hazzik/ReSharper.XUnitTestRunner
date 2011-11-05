namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Xml;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.UnitTestFramework;

    [SolutionComponent]
    public class XunitTestElementSerializer : IUnitTestElementSerializer
    {
        private readonly XunitTestProvider provider;
        private readonly XunitElementFactory factory;
        private readonly ISolution solution;

        public XunitTestElementSerializer(XunitTestProvider provider, XunitElementFactory factory, ISolution solution)
        {
            this.provider = provider;
            this.factory = factory;
            this.solution = solution;
        }

        public IUnitTestElement DeserializeElement(XmlElement parent, IUnitTestElement parentElement)
        {
            if (!parent.HasAttribute("type"))
                throw new ArgumentException(@"Element is not Xunit", "parent");
            switch (parent.GetAttribute("type"))
            {
                case "XunitTestClassElement":
                    return XunitTestClassElement.ReadFromXml(parent, factory, solution);
                case "XunitTestMethodElement":
                    return XunitTestMethodElement.ReadFromXml(parent, parentElement, factory, solution);
                default:
                    throw new ArgumentException(@"Element is not Xunit", "parent");
            }
        }

        public IUnitTestProvider Provider
        {
            get { return provider;  }
        }

        public void SerializeElement(XmlElement parent, IUnitTestElement element)
        {
            parent.SetAttribute("type", element.GetType().Name);
            
            var testElement = element as XunitTestElementBase;
            if (testElement == null)
                throw new ArgumentException(string.Format("Element {0} is not MSTest", element.GetType()), "element");
            
            testElement.WriteToXml(parent);
        }
    }
}