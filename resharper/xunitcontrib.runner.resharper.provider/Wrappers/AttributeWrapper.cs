using System;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
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
                return (TValue)attribute.NamedParameter(propertyName).ConstantValue.Value;
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