namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ReSharper.Psi;
    using Xunit;
    using Xunit.Sdk;

    internal static class MethodUtility
    {
        /// <summary>
        /// Gets the skip reason from a test method.
        /// 
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>
        /// The skip reason
        /// </returns>
        public static string GetSkipReason(IMethodInfo method)
        {
            IEnumerable<IAttributeInfo> attributes = method.GetCustomAttributes(typeof (FactAttribute));
            using (IEnumerator<IAttributeInfo> enumerator = attributes.GetEnumerator())
            {
                if (enumerator.MoveNext())
                    return enumerator.Current.GetPropertyValue<string>("Skip");
            }
            return null;
        }

        /// <summary>
        /// Determines whether a method is a test method. A test method must be decorated
        ///             with the <see cref="T:Xunit.FactAttribute"/> (or derived class) and must not be abstract.
        /// 
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>
        /// True if the method is a test method; false, otherwise
        /// </returns>
        public static bool IsTest(IMethodInfo method)
        {
            return !method.IsAbstract && method.HasAttribute(typeof(FactAttribute));
        }

        public static bool IsTest(IMethod method)
        {
            return IsTest(method.AsMethodInfo());
        }
    }

    internal static class TypeUtility
    {
        private static bool HasRunWith(ITypeInfo type)
        {
            return type.HasAttribute(typeof (RunWithAttribute));
        }

        public static bool IsTestClass(IClass @class)
        {
            var type = @class.AsTypeInfo();
            if (type.IsAbstract && !type.IsSealed)
            {
                return false;
            }
            return HasRunWith(type) || type.GetMethods().Any(MethodUtility.IsTest);
        }

        private static object HasRunWith(TypeWrapper.PsiClassWrapper type)
        {
            return type.GetCustomAttributes(typeof(RunWithAttribute)).Any();
        }

        public static bool HasRunWith(IClass testClass)
        {
            return HasRunWith(testClass.AsTypeInfo());
        }

        public static bool IsTestClass(IMetadataTypeInfo metadataTypeInfo)
        {
            ITypeInfo type = metadataTypeInfo.AsTypeInfo();
            if (type.IsAbstract && !type.IsSealed)
            {
                return false;
            }
            return HasRunWith(type) || type.GetMethods().Any(MethodUtility.IsTest);
        }
    }
}