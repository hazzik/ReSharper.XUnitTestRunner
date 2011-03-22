namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using JetBrains.Annotations;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
    using JetBrains.ReSharper.UnitTestFramework;

    public abstract class UnitTestElement : XUnitTestElementBase
    {
        protected UnitTestElement(IUnitTestRunnerProvider provider, XUnitTestElementBase parent)
            : base(provider, parent)
        {
        }

        public abstract string Kind { get; }
        
        public abstract IDeclaredElement GetDeclaredElement();

        public abstract IProject GetProject();

        [NotNull]
        public abstract string GetTitle();

        public abstract bool Equals(IUnitTestViewElement other);

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
