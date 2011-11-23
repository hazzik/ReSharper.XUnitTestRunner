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
        private readonly ProjectModelElementEnvoy envoy;
        private readonly XunitElementFactory factory;
        private readonly IProject project;

        public XunitMetadataExplorer([NotNull] XunitElementFactory factory, IProject project, UnitTestElementConsumer consumer)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            this.project = project;
            this.consumer = consumer;
            this.factory = factory;
            envoy = ProjectModelElementEnvoy.Create(project);
        }

        public void ExploreAssembly(IMetadataAssembly assembly)
        {
            foreach (var metadataTypeInfo in GetExportedTypes(assembly.GetTypes()))
            {
                ProcessClass(metadataTypeInfo);
            }
        }

        private static IEnumerable<IMetadataTypeInfo> GetExportedTypes(IEnumerable<IMetadataTypeInfo> types)
        {
            foreach (var type in (types ?? Enumerable.Empty<IMetadataTypeInfo>()).Where(UnitTestElementMetadataIdentifier.IsPublic))
            {
                yield return type;

                foreach (var nestedType in GetExportedTypes(type.GetNestedTypes()))
                {
                    yield return nestedType;
                }
            }
        }

        private XunitTestClassElement GetParentClassElement(IMetadataTypeInfo @class)
        {
            if (!@class.IsNested)
            {
                return null;
            }
            return factory.GetElementById(project, @class.DeclaringType.FullyQualifiedName) as XunitTestClassElement;
        }

        private void ProcessClass(IMetadataTypeInfo @class)
        {
            // TODO: What about HasRunWith support? Not supported in previous R# versions
            if (!UnitTestElementMetadataIdentifier.IsUnitTestContainer(@class))
                return;

            ProcessTestClass(@class);
        }

        private void ProcessTestClass(IMetadataTypeInfo @class)
        {
            var typeName = new ClrTypeName(@class.FullyQualifiedName);

            var classElement = factory.GetOrCreateClassElement(typeName, project, envoy, GetParentClassElement(@class));
            consumer(classElement);

            foreach (var info in UnitTestElementMetadataIdentifier.GetTestMethods(@class))
                ProcessTestMethod(classElement, info);
        }

        private void ProcessTestMethod(XunitTestClassElement classElement, IMetadataMethod method)
        {
            var typeName = new ClrTypeName(method.DeclaringType.FullyQualifiedName);

            var methodElement = factory.GetOrCreateMethodElement(typeName, method.Name, project, classElement, envoy, UnitTestElementMetadataIdentifier.GetSkipReason(method));
            consumer(methodElement);
        }
    }
}
