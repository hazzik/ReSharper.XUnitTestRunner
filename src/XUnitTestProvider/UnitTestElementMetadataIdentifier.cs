namespace ReSharper.XUnitTestProvider
{
    using JetBrains.Metadata.Reader.API;

    internal static class UnitTestElementMetadataIdentifier
    {
        public static bool IsUnitTestContainer(IMetadataTypeInfo metadataTypeInfo)
        {
            return IsPublic(metadataTypeInfo) && TypeUtility.IsTestClass(metadataTypeInfo.AsTypeInfo());
        }

        public static bool IsPublic(IMetadataTypeInfo type)
        {
            // Hmmm. This seems a little odd. Resharper reports public nested types with IsNestedPublic,
            // while IsPublic is false
            return type.IsPublic || (type.IsNested && type.IsNestedPublic);
        }
    }
}