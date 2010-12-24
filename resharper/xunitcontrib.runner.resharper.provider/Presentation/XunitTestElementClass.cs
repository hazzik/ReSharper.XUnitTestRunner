using System;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Text;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class XunitTestElementClass : XunitTestElement
    {
        readonly string assemblyLocation;
        private readonly CacheManager cacheManager;

        internal XunitTestElementClass(IUnitTestProvider provider, IProject project, string typeName,
                                       string assemblyLocation, CacheManager cacheManager)
            : base(provider, null, project, typeName)
        {
            this.assemblyLocation = assemblyLocation;
            this.cacheManager = cacheManager;
        }

        internal string AssemblyLocation
        {
            get { return assemblyLocation; }
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            var solution = GetSolution();
            if (solution == null)
                return null;

            var modules = PsiModuleManager.GetInstance(solution).GetPsiModules(GetProject());
            var projectModule = modules.Count > 0 ? modules[0] : null;
            var cache = cacheManager.GetDeclarationsCache(projectModule,false,true);
            return cache.GetTypeElementByCLRName(GetTypeClrName());
        }

        public override string Kind
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

        public override bool Equals(IUnitTestElement other)
        {
            return Equals(other as object);
        }

        public override string GetTitle()
        {
            return new CLRTypeName(GetTypeClrName()).ShortName;
        }

        public override bool Equals(IUnitTestViewElement other)
        {
            return Equals(other as object);
        }

        public virtual bool Matches(string filter, IdentifierMatcher matcher)
        {
            return matcher.Matches(GetTypeClrName());
        }
    }
}