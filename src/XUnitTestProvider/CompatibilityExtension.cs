namespace ReSharper.XUnitTestProvider
{
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.Util;

    internal static class CompatibilityExtension
    {
        public static IDeclaredType GetAttributeType(this IAttributeInstance attribute)
        {
            return attribute.AttributeType;
        } 
 
        public static FileSystemPath GetOutputFilePath(this IProject project)
        {
            return UnitTestManager.GetOutputAssemblyPath(project);
        }

        public static IDeclarationsCache GetSymbolScope(this CacheManager cacheManager, IPsiModule module, EmptyResolveContext instance, bool withReferences, bool caseSensitive)
        {
            return cacheManager.GetDeclarationsCache(module, withReferences, caseSensitive);
        }

        public static EmptyResolveContext GetResolveContext(this IProject project)
        {
            return null;
        }
    }

    internal class EmptyResolveContext
    {
        public static EmptyResolveContext Instance
        {
            get { return null; }
        }
    }
}