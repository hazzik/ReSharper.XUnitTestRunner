using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public abstract class UnitTestElement : IUnitTestElement, IUnitTestViewElement
    {
        // Fields
        private IEnumerable<UnitTestElementCategory> myCategories = UnitTestElementCategory.Uncategorized;

        private IList<IUnitTestElement> myChildren;
        private UnitTestElement myParent;
        private bool myValid = true;
        private string id;

        // Methods
        protected UnitTestElement(IUnitTestProvider provider, UnitTestElement parent)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            Provider = provider;
            Parent = parent;
        }

        public virtual bool Trackable
        {
            get
            {
                if (myChildren != null)
                {
                    return (myChildren.Count == 0);
                }
                return true;
            }
        }

        public bool Valid
        {
            get { return myValid; }
        }

        #region IUnitTestElement Members

        public void Invalidate()
        {
            myValid = false;
        }

        public ICollection<IUnitTestElement> Children
        {
            get { return (myChildren ?? EmptyArray<IUnitTestElement>.Instance); }
        }

        public bool Explicit
        {
            get { return (ExplicitReason != null); }
        }

        public string ExplicitReason { get; set; }

        public UnitTestElementState State { get; set; }

        public IUnitTestElement Parent
        {
            get { return myParent; }
            set
            {
                if (value != myParent)
                {
                    if (myParent != null)
                    {
                        myParent.RemoveChild(this);
                    }
                    myParent = (UnitTestElement) value;
                    if (myParent != null)
                    {
                        myParent.AppendChild(this);
                    }
                }
            }
        }

        public IUnitTestRunnerProvider Provider { get; private set; }

        [NotNull]
        public abstract string ShortName { get; }

        public abstract bool Equals(IUnitTestElement other);

        #endregion

        #region IUnitTestViewElement Members

        public IEnumerable<UnitTestElementCategory> Categories
        {
            get { return myCategories; }
        }

        public abstract IDeclaredElement GetDeclaredElement();

        public abstract UnitTestElementDisposition GetDisposition();

        public abstract string Kind { get; }
        public abstract UnitTestNamespace GetNamespace();

        public abstract IProject GetProject();

        [NotNull]
        public abstract string GetTitle();

        public abstract bool Equals(IUnitTestViewElement other);

        #endregion

        private void AppendChild(UnitTestElement element)
        {
            if (myChildren == null)
            {
                myChildren = new List<IUnitTestElement>();
            }
            myChildren.Add(element);
        }

        public override bool Equals(object obj)
        {
            return (ReferenceEquals(this, obj) ||
                    ((obj.GetType() == GetType()) && (Provider == ((UnitTestElement) obj).Provider)));
        }

        public override int GetHashCode()
        {
            return Provider.ID.GetHashCode();
        }

        public abstract IList<IProjectFile> GetProjectFiles();

        [CanBeNull]
        public ISolution GetSolution()
        {
            IProject project = GetProject();
            if (project != null)
            {
                return project.GetSolution();
            }
            return null;
        }

        public abstract string GetTypeClrName();

        private void RemoveChild(UnitTestElement element)
        {
            if ((myChildren == null) || !myChildren.Remove(element))
            {
                throw new InvalidOperationException("No such element");
            }
        }

        public void SetExplicit(string reason)
        {
            ExplicitReason = reason;
        }

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
        public IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            var testMethod = this as XunitTestElementMethod;
            if (testMethod != null)
            {
                var testClass = testMethod.Class;
                var result = new List<UnitTestTask>
                                 {
                                     new UnitTestTask(null, CreateAssemblyTask(testClass.AssemblyLocation)),
                                     new UnitTestTask(testClass, CreateClassTask(testClass, explicitElements)),
                                     new UnitTestTask(testMethod, CreateMethodTask(testMethod, explicitElements))
                                 };

                return result;
            }

            // We don't have to do anything explicit for a test class, because when a class is run
            // we get called for each method, and each method already adds everything we need (loading
            // the assembly and the class)
            if (this is XunitTestElementClass)
                return EmptyArray<UnitTestTask>.Instance;

            throw new ArgumentException(String.Format("element is not xUnit: '{0}'", this));
        }

        public abstract string Id { get; }

        private static RemoteTask CreateAssemblyTask(string assemblyLocation)
        {
            return new XunitTestAssemblyTask(assemblyLocation);
        }

        private static RemoteTask CreateClassTask(XunitTestElementClass testClass, IEnumerable<IUnitTestElement> explicitElements)
        {
            return new XunitTestClassTask(testClass.AssemblyLocation, testClass.GetTypeClrName(), explicitElements.Contains(testClass));
        }

        private static RemoteTask CreateMethodTask(XunitTestElementMethod testMethod, IEnumerable<IUnitTestElement> explicitElements)
        {
            return new XunitTestMethodTask(testMethod.Class.AssemblyLocation, testMethod.Class.GetTypeClrName(), testMethod.MethodName, explicitElements.Contains(testMethod));
        }
    }
}