namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using JetBrains.Annotations;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.UnitTestFramework;

    public abstract partial class XunitTestElementBase : IUnitTestElement
    {
        private readonly IEnumerable<UnitTestElementCategory> categories = UnitTestElementCategory.Uncategorized;
        private readonly XunitServiceProvider provider;
        private readonly ProjectModelElementEnvoy project;
        private XunitTestClassElement parent;

        protected XunitTestElementBase([NotNull] XunitServiceProvider provider, ProjectModelElementEnvoy project, string id, IClrTypeName typeName)
        {
            if (provider == null) 
                throw new ArgumentNullException("provider");
            if (project == null)
                throw new ArgumentNullException("project");
            this.provider = provider;
            this.project = project;
            TypeName = typeName;
            Id = id;
        }

        public IClrTypeName TypeName { get; private set; }

        public string ExplicitReason { get; set; }

        public IEnumerable<UnitTestElementCategory> Categories
        {
            get { return categories; }
        }

        public abstract ICollection<IUnitTestElement> Children { get; }

        public bool Explicit
        {
            get { return (ExplicitReason != null); }
        }

        public UnitTestElementState State { get; set; }

        public IUnitTestProvider Provider
        {
            get { return provider.Provider; }
        }

        IUnitTestElement IUnitTestElement.Parent
        {
            get { return Parent; }
            set { Parent = value as XunitTestClassElement; }
        }

        [NotNull]
        public abstract string ShortName { get; }

        public string Id { get; private set; }

        public abstract bool Equals(IUnitTestElement other);

        public abstract string GetPresentation(IUnitTestElement parent);

        public string GetPresentation()
        {
            return GetPresentation(null);
        }

        public abstract IDeclaredElement GetDeclaredElement();
        
        public abstract IEnumerable<IProjectFile> GetProjectFiles();

        /// This method gets called to generate the tasks that the remote runner will execute
        /// When we run all the tests in a class (by e.g. clicking the menu in the margin marker)
        /// this method is called with a class element and the list of explicit elements contains
        /// one item - the class. We should add all tasks required to prepare to run this class
        /// (e.g. loading the assembly and loading the class via reflection - but NOT running the
        /// test methods)
        /// It is then subsequently called with all method elements, and with the same list (that
        /// only has the class as an explicit element). We should return a new sequence of tasks
        /// that would allow running the test methods. This will probably be a superset of the class
        /// sequence of tasks (e.g. load the assembly, load the class via reflection and a task
        /// to run the given method)
        /// Because the tasks implement IEquatable, they can be compared to collapse the multiple
        /// lists of tasks into a tree (or set of trees) with only one instance of each unique
        /// task in order, so in the example, we load the assmbly once, then we load the class once,
        /// and then we have multiple tasks for each running test method. If there are multiple classes
        /// run, then multiple class lists will be created and collapsed under a single assembly node.
        /// If multiple assemblies are run, then there will be multiple top level assembly nodes, each
        /// with one or more uniqe class nodes, each with one or more unique test method nodes
        /// If you explicitly run a test method from its menu, then it will appear in the list of
        /// explicit elements.
        /// ReSharper provides an AssemblyLoadTask that can be used to load the assembly via reflection.
        /// In the remote runner process, this task is serviced by a runner that will create an app domain
        /// shadow copy the files (which we can affect via ProfferConfiguration) and then cause the rest
        /// of the nodes (e.g. the classes + methods) to get executed (by serializing the nodes, containing
        /// the remote tasks from these lists, over into the new app domain). This is the default way the
        /// nunit and mstest providers work.
        public abstract IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch);

        public IList<UnitTestTask> GetTaskSequence(IList<IUnitTestElement> explicitElements)
        {
            return GetTaskSequence(explicitElements, null);
        }

        public abstract string Kind { get; }

        public XunitTestClassElement Parent
        {
            get { return parent; }
            set
            {
                var val = value;
                if (val == parent)
                    return;
                if (parent != null)
                    parent.RemoveChild(this);
                parent = val;
                if (parent == null)
                    return;
                parent.AppendChild(this);
            }
        }

        public UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(TypeName.GetNamespaceName());
        }

        public IProject GetProject()
        {
            return project.GetValidProjectElement() as IProject;
        }

        public UnitTestElementDisposition GetDisposition()
        {
            IDeclaredElement element = GetDeclaredElement();
            if (element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            var locations = from declaration in element.GetDeclarations()
                            let file = declaration.GetContainingFile()
                            where file != null
                            select new UnitTestElementLocation(file.GetSourceFile().ToProjectFile(),
                                                               declaration.GetNameDocumentRange().TextRange,
                                                               declaration.GetDocumentRange().TextRange);

            return new UnitTestElementDisposition(locations.ToList(), this);
        }

        public abstract void WriteToXml(XmlElement xml);

        protected XunitServiceProvider ServiceProvider
        {
            get { return provider; }
        }
    }
}