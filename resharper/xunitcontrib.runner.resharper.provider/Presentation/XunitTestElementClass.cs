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
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.Util;

    internal class XunitTestElementClass : XunitTestElement, IUnitTestViewElement
    {
		private readonly string assemblyLocation;
		private readonly CacheManager cacheManager;

		internal XunitTestElementClass(IUnitTestProvider provider,
		                               IProject project,
		                               string typeName,
		                               string assemblyLocation,
		                               CacheManager cacheManager)
			: base(provider, null, project, typeName)
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

		public override IDeclaredElement GetDeclaredElement()
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
    }
}