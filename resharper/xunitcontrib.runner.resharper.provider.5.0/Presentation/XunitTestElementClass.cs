using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Filtering;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitTestElementClass : XunitTestElement
    {
        readonly string assemblyLocation;

        public XunitTestElementClass(IUnitTestProvider provider,
                                     IProjectModelElement project,
                                     string typeName,
                                     string assemblyLocation)
            : base(provider, null, project, typeName)
        {
            this.assemblyLocation = assemblyLocation;
        }

        public string AssemblyLocation
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
            var scope = DeclarationsScopeFactory.ModuleScope(projectModule, false);
            var cache = PsiManager.GetInstance(solution).GetDeclarationsCache(scope, true);
            return cache.GetTypeElementByCLRName(GetTypeClrName());
        }

        public override string GetKind()
        {
            return "xUnit.net Test Class";
        }

        public override string ShortName
        {
            get { return GetTitle(); }
        }

        public override string GetTitle()
        {
            return new CLRTypeName(GetTypeClrName()).ShortName;
        }

        public override bool Matches(string filter, IdentifierMatcher matcher)
        {
            return matcher.Matches(GetTypeClrName());
        }
    }
}