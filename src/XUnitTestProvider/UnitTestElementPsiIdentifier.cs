namespace ReSharper.XUnitTestProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Util;

    internal static class UnitTestElementPsiIdentifier
    {
        private static readonly IClrTypeName propertyDataAttribute = new ClrTypeName("Xunit.Extensions.PropertyDataAttribute");
        private static readonly IClrTypeName hasRunWithAttributeName = new ClrTypeName("Xunit.RunWithAttribute");
        private static readonly IClrTypeName factAttributeName = new ClrTypeName("Xunit.FactAttribute");

        public static bool ContainsTestMethods(IClass @class)
        {
            return GetMethods(@class)
                .Any(method => !method.IsAbstract && IsTest(method));
        }

        [UsedImplicitly]
        public static string GetSkipReason(IMethod method)
        {
            return GetCustomAttributes(method, factAttributeName)
                .Select(attribute => GetPropertyValue<string>(attribute, "Skip"))
                .FirstOrDefault();
        }

        public static bool HasRunWith(IClass @class)
        {
            return HasAttribute(@class, hasRunWithAttributeName);
        }

        public static bool IsAbstract(IClass @class)
        {
            return @class.IsAbstract;
        }

        public static bool IsAnyUnitTestElement(IDeclaredElement element)
        {
            return IsUnitTestContainer(element) ||
                   IsUnitTest(element) ||
                   IsUnitTestStuff(element);
        }

        public static bool IsPublic(IAccessRightsOwner element)
        {
            return element.GetAccessRights() == AccessRights.PUBLIC;
        }

        public static bool IsStatic(IClass @class)
        {
            return @class.IsAbstract && @class.IsSealed;
        }

        public static bool IsUnitTest(IDeclaredElement element)
        {
            var method = element as IMethod;
            return method != null && IsTest(method);
        }

        public static bool IsUnitTestContainer(IDeclaredElement element)
        {
            var @class = element as IClass;
            return @class != null &&
                   IsPublic(@class) &&
                   (IsStatic(@class) || !IsAbstract(@class)) &&
                   (HasRunWith(@class) || ContainsTestMethods(@class));
        }

        public static bool IsUnitTestStuff(IDeclaredElement element)
        {
            return IsContainingUnitTestClass(element as IClass) ||
                   IsUnitTestDataProperty(element) ||
                   IsUnitTestClassConstructor(element);
        }

        private static IEnumerable<IAttributeInstance> GetCustomAttributes(IAttributesSet attributesSet, IClrTypeName typeName)
        {
            return attributesSet.GetAttributeInstances(false)
                .Where(attribute => IsAssignableFrom(typeName, attribute.AttributeType));
        }

        private static IEnumerable<IMethod> GetMethods(ITypeElement psiType)
        {
            // IClass.Methods returns only the methods of this class
            IEnumerable<IMethod> publicStaticMethods = from method in psiType.Methods
                                                       where method.IsStatic && method.GetAccessRights() == AccessRights.PUBLIC
                                                       select method;

            // Let R#'s TypeElementUtil walk the super class chain - we don't have to worry about circular references, etc...
            IEnumerable<IMethod> allPublicInstanceMethods = from typeMemberInstance in psiType.GetAllClassMembers()
                                                            let typeMember = typeMemberInstance.Member as IMethod
                                                            where typeMember != null && !typeMember.IsStatic && typeMember.GetAccessRights() == AccessRights.PUBLIC
                                                            select typeMember;

            return allPublicInstanceMethods.Concat(publicStaticMethods);
        }

        private static T GetPropertyValue<T>(IAttributeInstance attribute, string skip)
        {
            var parameter = attribute.NamedParameter(skip);
            return parameter.IsConstant
                       ? (T) parameter.ConstantValue.Value
                       : default(T);
        }

        private static bool HasAttribute(IAttributesSet attributesSet, IClrTypeName typeName)
        {
            return GetCustomAttributes(attributesSet, typeName)
                .Any();
        }

        private static bool IsAssignableFrom(IClrTypeName typeName, IDeclaredType c)
        {
            return Equals(typeName, c.GetClrName()) ||
                   c.GetAllSuperTypes().Any(superType => Equals(typeName, superType.GetClrName()));
        }

        private static bool IsContainingUnitTestClass(IClass @class)
        {
            return @class != null && IsPublic(@class) && @class.NestedTypes.Any(IsAnyUnitTestElement);
        }

        private static bool IsTest(IMethod method)
        {
            return !method.IsAbstract && HasAttribute(method, factAttributeName);
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
            IEnumerable<string> propertyNames = from method in element.GetContainingType().Methods
                                                from attributeInstance in method.GetAttributeInstances(propertyDataAttribute, false)
                                                select attributeInstance.PositionParameter(0).ConstantValue.Value as string;
            return propertyNames.Any(name => name == element.ShortName);
        }

        private static bool IsUnitTestClassConstructor(IDeclaredElement element)
        {
            var constructor = element as IConstructor;
            return constructor != null && constructor.IsDefault && IsUnitTestContainer(constructor.GetContainingType());
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
    }
}