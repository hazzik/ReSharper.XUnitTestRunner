namespace ReSharper.XUnitTestProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Sdk;

    public static class TypeUtility
    {
        public static bool ContainsTestMethods(ITypeInfo type)
        {
            return GetTestMethods(type).Any();
        }

        public static IEnumerable<IMethodInfo> GetTestMethods(ITypeInfo type)
        {
            return type.GetMethods().Where(MethodUtility.IsTest);
        }

        public static bool HasRunWith(ITypeInfo type)
        {
            return type.HasAttribute(typeof (RunWithAttribute));
        }

        public static bool IsAbstract(ITypeInfo type)
        {
            return type.IsAbstract;
        }

        public static bool IsStatic(ITypeInfo type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        public static bool IsTestClass(ITypeInfo type)
        {
            return (IsStatic(type) || !IsAbstract(type)) && (HasRunWith(type) || ContainsTestMethods(type));
        }
    }
}