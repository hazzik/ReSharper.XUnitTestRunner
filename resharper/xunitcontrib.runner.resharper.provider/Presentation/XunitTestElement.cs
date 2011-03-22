using System;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using JetBrains.Annotations;
    using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;

    internal abstract class XunitTestElement : XUnitTestElementBase
	{
		public readonly string TypeName;
		private readonly IProject project;
		private readonly IProjectModelElementPointer projectPointer;

		protected XunitTestElement(IUnitTestRunnerProvider provider,
		                           XUnitTestElementBase parent,
		                           IProject project,
		                           string typeName)
			: base(provider, parent)
		{
			if(project == null)
				throw new ArgumentNullException("project");

			if(typeName == null)
				throw new ArgumentNullException("typeName");

			this.project = project;
			TypeName = typeName;
			projectPointer = project.CreatePointer();
		}

        protected ITypeElement GetDeclaredType()
		{
			IProject project = GetProject();
			if(project == null)
			{
				return null;
			}
			PsiManager manager = PsiManager.GetInstance(project.GetSolution());
			using (ReadLockCookie.Create())
			{
				return CacheManager.GetInstance(manager.Solution)
					.GetDeclarationsCache(PsiModuleManager.GetInstance(project.GetSolution()).GetPrimaryPsiModule(project), true, true)
					.GetTypeElementByCLRName(TypeName);
			}
		}

        public virtual UnitTestNamespace GetNamespace()
		{
			return new UnitTestNamespace(new ClrTypeName(TypeName).GetNamespaceName());
		}

		public virtual IProject GetProject()
		{
			return project;
		}

		public virtual IProjectModelElementPointer GetProjectPointer()
		{
			return projectPointer;
		}

		public string GetTypeClrName()
		{
			return TypeName;
		}

		public override bool Equals(object obj)
		{
			if(base.Equals(obj))
			{
				var element = (XunitTestElement) obj;

				if(Equals(element.project, project))
					return (element.TypeName == TypeName);
			}

			return false;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = base.GetHashCode();
				result = (result*397) ^ (project != null ? project.GetHashCode() : 0);
				result = (result*397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
				return result;
			}
		}

	    public abstract IDeclaredElement GetDeclaredElement();

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

//        public override bool Equals(object obj)
//        {
//            return (ReferenceEquals(this, obj) ||
//                    ((obj.GetType() == GetType()) && (Provider == ((UnitTestElement) obj).Provider)));
//        }

//        public override int GetHashCode()
//        {
//            return Provider.ID.GetHashCode();
//        }
	}
}