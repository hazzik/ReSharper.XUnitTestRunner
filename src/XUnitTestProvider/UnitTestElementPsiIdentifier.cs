namespace ReSharper.XUnitTestProvider
{
    using System.Linq;
    using JetBrains.ReSharper.Psi;
    using Xunit.Sdk;

    internal static class UnitTestElementPsiIdentifier
    {
        private static readonly ClrTypeName PropertyDataAttributeName = new ClrTypeName("Xunit.Extensions.PropertyDataAttribute");

        public static bool IsAnyUnitTestElement(IDeclaredElement element)
        {
            return IsUnitTestContainer(element) ||
                   IsUnitTest(element) ||
                   IsUnitTestStuff(element);
        }

        public static bool IsUnitTestContainer(IDeclaredElement element)
        {
            var @class = element as IClass;
            return @class != null && IsPublic(@class) && TypeUtility.IsTestClass(@class.AsTypeInfo());
        }

        public static bool IsUnitTest(IDeclaredElement element)
        {
            var method = element as IMethod;
            return method != null && MethodUtility.IsTest(method.AsMethodInfo());
        }

        public static bool IsUnitTestStuff(IDeclaredElement element)
        {
            return IsContainingUnitTestClass(element as IClass) ||
                   IsUnitTestDataProperty(element) ||
                   IsUnitTestClassConstructor(element);
        }

        private static bool IsContainingUnitTestClass(IClass @class)
        {
            return @class != null && IsPublic(@class) && @class.NestedTypes.Any(IsAnyUnitTestElement);
        }

        private static bool IsUnitTestDataProperty(IDeclaredElement element)
        {
            var accessor = element as IAccessor;
            if (accessor != null)
            {
                return accessor.Kind == AccessorKind.GETTER && IsTheoryPropertyDataProperty(accessor.OwnerMember);
            }

            var property = element as IProperty;
            return property != null && IsTheoryPropertyDataProperty(property);
        }

        private static bool IsUnitTestClassConstructor(IDeclaredElement element)
        {
            var constructor = element as IConstructor;
            return constructor != null && constructor.IsDefault && IsUnitTestContainer(constructor.GetContainingType());
        }

        private static bool IsTheoryPropertyDataProperty(ITypeMember element)
        {
            if (!element.IsStatic || !IsPublic(element))
            {
                return false;
            }

            // According to msdn, parameters to the constructor are positional parameters, and any
            // public read-write fields are named parameters. The name of the property we're after
            // is not a public field/property, so it's a positional parameter
            var containingType = element.GetContainingType();
            if (containingType == null)
            {
                return false;
            }

            var propertyNames = from method in containingType.Methods
                from attributeInstance in method.GetAttributeInstances(PropertyDataAttributeName, false)
                select attributeInstance.PositionParameter(0).ConstantValue.Value as string;
            return propertyNames.Any(name => name == element.ShortName);
        }

        public static bool IsPublic(IAccessRightsOwner element)
        {
            return element.GetAccessRights() == AccessRights.PUBLIC;
        }

        public static string GetSkipReason(IMethod method)
        {
            return MethodUtility.GetSkipReason(method.AsMethodInfo());
        }
    }
}