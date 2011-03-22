using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public abstract class UnitTestElement : XUnitTestElementBase, IUnitTestViewElement
    {
        // Fields

        // Methods
        protected UnitTestElement(IUnitTestProvider provider, UnitTestElement parent) : base(provider, parent)
        {
        }

        #region IUnitTestElement Members

        #endregion

        #region IUnitTestViewElement Members

        public abstract IDeclaredElement GetDeclaredElement();

        public abstract UnitTestElementDisposition GetDisposition();

        public abstract string Kind { get; }
        
        public abstract UnitTestNamespace GetNamespace();

        public abstract IProject GetProject();

        public abstract IProjectModelElementPointer GetProjectPointer();

        [NotNull]
        public abstract string GetTitle();

        public abstract bool Equals(IUnitTestViewElement other);

        #endregion

        public override bool Equals(object obj)
        {
            return (ReferenceEquals(this, obj) ||
                    ((obj.GetType() == GetType()) && (Provider == ((UnitTestElement) obj).Provider)));
        }

        public override int GetHashCode()
        {
            return Provider.ID.GetHashCode();
        }

        [CanBeNull]
        protected ISolution GetSolution()
        {
            IProject project = GetProject();
            if (project != null)
            {
                return project.GetSolution();
            }
            return null;
        }
    }
}