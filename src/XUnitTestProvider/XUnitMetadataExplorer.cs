namespace ReSharper.XUnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.UnitTestFramework;

    public sealed class XunitMetadataExplorer
    {
        private readonly UnitTestElementConsumer consumer;
        private readonly IProject project;
        private readonly XunitElementFactory factory;
        private readonly ProjectModelElementEnvoy envoy;

        public XunitMetadataExplorer([NotNull] XunitElementFactory factory, IProject project, UnitTestElementConsumer consumer)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            this.project = project;
            this.consumer = consumer;
            this.factory = factory;
            envoy = ProjectModelElementEnvoy.Create(project);
        }

        private void ProcessTypeInfo(IMetadataTypeInfo metadataTypeInfo)
        {
            // TODO: What about HasRunWith support? Not supported in previous R# versions
            if (!UnitTestElementMetadataIdentifier.IsUnitTestContainer(metadataTypeInfo))
                return;

            ProcessTestClass(metadataTypeInfo);
        }

        private void ProcessTestClass(IMetadataTypeInfo metadataTypeInfo)
        {
            var typeName = new ClrTypeName(metadataTypeInfo.FullyQualifiedName);

            XunitTestClassElement classElement = factory.GetOrCreateClassElement(typeName, project, envoy, GetParentClassElement(metadataTypeInfo));
            consumer(classElement);

            foreach (IMetadataMethod info in UnitTestElementMetadataIdentifier.GetTestMethods(metadataTypeInfo))
            {
                ProcessTestMethod(classElement, info);
            }
        }

        private void ProcessTestMethod(XunitTestClassElement classElement, IMetadataMethod info)
        {
            var typeName = new ClrTypeName(info.DeclaringType.FullyQualifiedName);

            XunitTestMethodElement methodUnitTestElement = factory.GetOrCreateMethodElement(typeName, info.Name, project, classElement, envoy);
            methodUnitTestElement.ExplicitReason = UnitTestElementMetadataIdentifier.GetSkipReason(info);
            // TODO: Categories?
            consumer(methodUnitTestElement);
        }

        private XunitTestClassElement GetParentClassElement(IMetadataTypeInfo type)
        {
            if (!type.IsNested)
            {
                return null;
            }
            return factory.GetElementById(project, type.DeclaringType.FullyQualifiedName) as XunitTestClassElement;
        }

        public void ExploreAssembly(IMetadataAssembly assembly)
        {
            foreach (IMetadataTypeInfo metadataTypeInfo in GetExportedTypes(assembly.GetTypes()))
            {
                ProcessTypeInfo(metadataTypeInfo);
            }
        }

        private static IEnumerable<IMetadataTypeInfo> GetExportedTypes(IEnumerable<IMetadataTypeInfo> types)
        {
            foreach (IMetadataTypeInfo type in (types ?? Enumerable.Empty<IMetadataTypeInfo>()).Where(UnitTestElementMetadataIdentifier.IsPublic))
            {
                yield return type;
                
                foreach (IMetadataTypeInfo nestedType in GetExportedTypes(type.GetNestedTypes()))
                {
                    yield return nestedType;
                }
            }
        }
    }
}
