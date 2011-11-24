namespace ReSharper.XUnitTestProvider
{
    using System;
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ReSharper.Psi;
    using Xunit.Sdk;

    internal static class AttributeWrapper
    {
        internal static IAttributeInfo AsAttributeInfo(this IAttributeInstance attribute)
        {
            return new AttributeInstanceWrapper(attribute);
        }

        internal static IAttributeInfo AsAttributeInfo(this IMetadataCustomAttribute attribute)
        {
            return new MetadataCustomAttributeWrapper(attribute);
        }

        private class AttributeInstanceWrapper : IAttributeInfo
        {
            readonly IAttributeInstance attribute;

            public AttributeInstanceWrapper(IAttributeInstance attribute)
            {
                this.attribute = attribute;
            }

            public T GetInstance<T>() where T : Attribute
            {
                return null;
            }

            public TValue GetPropertyValue<TValue>(string propertyName)
            {
                var parameter = attribute.NamedParameter(propertyName);
                if (parameter.IsConstant)
                    return (TValue) parameter.ConstantValue.Value;
                return default(TValue);
            }
        }

        private class MetadataCustomAttributeWrapper : IAttributeInfo
        {
            readonly IMetadataCustomAttribute attribute;

            public MetadataCustomAttributeWrapper(IMetadataCustomAttribute attribute)
            {
                this.attribute = attribute;
            }

            public T GetInstance<T>() where T : Attribute
            {
                return null;
            }

            public TValue GetPropertyValue<TValue>(string propertyName)
            {
                foreach (var prop in attribute.InitializedProperties)
                    if (prop.Property.Name == propertyName)
                        return (TValue)prop.Value.Value;

                return default(TValue);
            }
        }
    }
}