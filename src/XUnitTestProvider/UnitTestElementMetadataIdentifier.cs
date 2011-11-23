namespace ReSharper.XUnitTestProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ReSharper.Psi;

    internal static class UnitTestElementMetadataIdentifier
    {
        private static readonly IClrTypeName runWithAttributeName = new ClrTypeName("Xunit.RunWithAttribute");
        private static readonly IClrTypeName factAttributeName = new ClrTypeName("Xunit.FactAttribute");

        public static string GetSkipReason(IMetadataMethod method)
        {
            return GetCustomAttributes(method, factAttributeName)
                .Select(x => GetPropertyValue<string>(x, "Skip"))
                .FirstOrDefault();
        }

        public static IEnumerable<IMetadataMethod> GetTestMethods(IMetadataTypeInfo @class)
        {
            return GetMethods(@class).Where(IsTest);
        }

        public static bool IsPublic(IMetadataTypeInfo @class)
        {
            // Hmmm. This seems a little odd. Resharper reports public nested types with IsNestedPublic,
            // while IsPublic is false
            return @class.IsPublic || (@class.IsNested && @class.IsNestedPublic);
        }

        public static bool IsUnitTestContainer(IMetadataTypeInfo @class)
        {
            return IsPublic(@class) &&
                   (IsStatic(@class) || !@class.IsAbstract) &&
                   (HasRunWith(@class) || ContainsTestMethods(@class));
        }

        private static bool ContainsTestMethods(IMetadataTypeInfo @class)
        {
            return GetTestMethods(@class).Any();
        }

        private static IEnumerable<IMetadataCustomAttribute> GetCustomAttributes(IMetadataEntity @class, IClrTypeName typeName)
        {
            return @class.CustomAttributes.Where(attribute => IsAssignableFrom(typeName, attribute.UsedConstructor.DeclaringType));
        }

        private static IEnumerable<IMetadataMethod> GetMethods(IMetadataTypeInfo @class)
        {
            // This can theoretically cause an infinite loop if the class inherits from itself,
            // but seeing as we're wrapping metadata from a physical assembly, I think it would
            // be very difficult to get into that situation
            IMetadataTypeInfo currentType = @class;
            do
            {
                foreach (IMetadataMethod method in currentType.GetMethods())
                    yield return method;

                currentType = currentType.Base.Type;
            } while (currentType.Base != null);
        }

        private static T GetPropertyValue<T>(IMetadataCustomAttribute attribute, string propertyName)
            where T : class
        {
            return attribute.InitializedProperties
                .Where(prop => prop.Property.Name == propertyName)
                .Select(prop => (T) prop.Value.Value)
                .FirstOrDefault();
        }

        private static bool HasAttribute(IMetadataEntity @class, IClrTypeName typeName)
        {
            return GetCustomAttributes(@class, typeName).Any();
        }

        private static bool HasRunWith(IMetadataTypeInfo @class)
        {
            return HasAttribute(@class, runWithAttributeName);
        }

        private static bool IsAssignableFrom(IClrTypeName typeName, IMetadataTypeInfo c)
        {
            // TODO: Can this cause an infinite loop/stack overflow?
            // I think maybe not, because we're dealing with metadata, which means we've successfully compiled
            // and it'll be very hard to compile circular inheritance chains

            return typeName.FullName == c.FullyQualifiedName ||
                   c.Base != null && IsAssignableFrom(typeName, c.Base.Type);
        }

        private static bool IsStatic(IMetadataTypeInfo @class)
        {
            return @class.IsAbstract && @class.IsSealed;
        }

        private static bool IsTest(IMetadataMethod method)
        {
            return !method.IsAbstract && HasAttribute(method, factAttributeName);
        }
    }
}