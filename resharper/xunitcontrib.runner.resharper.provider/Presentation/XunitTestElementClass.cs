using System;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using System.Linq;
    using JetBrains.Annotations;
    using JetBrains.ReSharper.Psi.Tree;

    internal class XunitTestElementClass : XUnitRunnerTestClassElement, IUnitTestViewElement
    {
        private readonly CacheManager cacheManager;

        internal XunitTestElementClass(IUnitTestRunnerProvider provider,
		                               IProject project,
		                               string typeName,
		                               string assemblyLocation,
		                               CacheManager cacheManager)
            : this(provider, typeName, assemblyLocation)
		{
            if (project == null)
                throw new ArgumentNullException("project");

            this.project = project;
            projectPointer = project.CreatePointer();
            this.cacheManager = cacheManager;
		}

        private XunitTestElementClass(IUnitTestRunnerProvider provider, string typeName, string assemblyLocation)
            : base(provider, typeName, assemblyLocation)
        {
        }

        public virtual string Kind
		{
			get { return "xUnit.net Test Class"; }
		}

        public virtual IDeclaredElement GetDeclaredElement()
		{
			var solution = GetSolution();
			if(solution == null)
				return null;

			var modules = PsiModuleManager.GetInstance(solution).GetPsiModules(GetProject());
			var projectModule = modules.Count > 0 ? modules[0] : null;
			var cache = cacheManager.GetDeclarationsCache(projectModule, false, true);
			return cache.GetTypeElementByCLRName(GetTypeClrName());
		}

        public virtual string GetTitle()
		{
			return new ClrTypeName(GetTypeClrName()).ShortName;
		}

        public virtual bool Equals(IUnitTestViewElement other)
		{
			return Equals(other as object);
		}

        public virtual UnitTestElementDisposition GetDisposition()
	    {
	        var element = GetDeclaredElement();
	        if(element == null || !element.IsValid())
	            return UnitTestElementDisposition.InvalidDisposition;

	        var locations = from declaration in element.GetDeclarations()
	                        let file = declaration.GetContainingFile()
	                        where file != null
	                        select
	                            new UnitTestElementLocation(file.GetSourceFile().ToProjectFile(),
	                                                        declaration.GetNameDocumentRange().TextRange,
	                                                        declaration.GetDocumentRange().TextRange);

	        return new UnitTestElementDisposition(locations.ToList(), this);
	    }

        public virtual IProject GetProject()
        {
            return project;
        }

        public virtual IProjectModelElementPointer GetProjectPointer()
        {
            return projectPointer;
        }

        public virtual string GetTypeClrName()
        {
            return TypeName;
        }

        [CanBeNull]
        protected virtual ISolution GetSolution()
        {
            IProject project = GetProject();
            if (project != null)
            {
                return project.GetSolution();
            }
            return null;
        }

        public virtual UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(new ClrTypeName(TypeName).GetNamespaceName());
        }

        private readonly IProject project;

        private readonly IProjectModelElementPointer projectPointer;

//        public override bool Equals(object obj)
//		{
//			if(base.Equals(obj))
//			{
//				var element = (XunitTestElement) obj;
//
//				if(Equals(element.project, project))
//					return (element.TypeName == TypeName);
//			}
//
//			return false;
//		}
//
//		public override int GetHashCode()
//		{
//			unchecked
//			{
//				var result = base.GetHashCode();
//				result = (result*397) ^ (project != null ? project.GetHashCode() : 0);
//				result = (result*397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
//				return result;
//			}
//		}

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