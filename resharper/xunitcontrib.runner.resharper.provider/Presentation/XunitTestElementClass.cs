using System;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.Util;

    internal class XunitTestElementClass : XUnitTestElementBase, IUnitTestViewElement
    {
		private readonly string assemblyLocation;
		private readonly CacheManager cacheManager;

        internal XunitTestElementClass(IUnitTestRunnerProvider provider,
		                               IProject project,
		                               string typeName,
		                               string assemblyLocation,
		                               CacheManager cacheManager)
			: this(provider, null, project, typeName)
		{
			this.assemblyLocation = assemblyLocation;
			this.cacheManager = cacheManager;
		}

		internal string AssemblyLocation
		{
			get { return assemblyLocation; }
		}

		public virtual string Kind
		{
			get { return "xUnit.net Test Class"; }
		}

		public override string Id
		{
			get { return TypeName; }
		}

		public override string ShortName
		{
			get { return GetTitle(); }
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

		public override bool Equals(IUnitTestElement other)
		{
			return Equals(other as object);
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

        public override IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            // We don't have to do anything explicit for a test class, because when a class is run
            // we get called for each method, and each method already adds everything we need (loading
            // the assembly and the class)
            return EmptyArray<UnitTestTask>.Instance;
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

                public string TypeName { get; private set; }
        private readonly IProject project;
        private readonly IProjectModelElementPointer projectPointer;

        private XunitTestElementClass(IUnitTestRunnerProvider provider,
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