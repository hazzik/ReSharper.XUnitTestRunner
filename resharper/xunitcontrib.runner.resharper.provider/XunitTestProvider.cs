using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider.Properties;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [UnitTestProvider]
    [UsedImplicitly]
    public class XunitTestProvider : XunitTestRunnerProvider, IUnitTestProvider
    {
        private static readonly AssemblyLoader AssemblyLoader = new AssemblyLoader();
        private static readonly UnitTestElementComparer comparer;
        private readonly CacheManager cacheManager;
        private readonly ISolution solution;

        static XunitTestProvider()
        {
            // ReSharper automatically adds all test providers to the list of assemblies it uses
            // to handle the AppDomain.Resolve event

            // The test runner process talks to devenv/resharper via remoting, and so devenv needs
            // to be able to resolve the remote assembly to be able to recreate the serialised types.
            // (Aside: the assembly is already loaded - why does it need to resolve it again?)
            // ReSharper automatically adds all unit test provider assemblies to the list of assemblies
            // it uses to handle the AppDomain.Resolve event. Since we've got a second assembly to
            // live in the remote process, we need to add this to the list.
            AssemblyLoader.RegisterAssembly(typeof (XunitTaskRunner).Assembly);
            comparer = new UnitTestElementComparer(new[] {typeof (XunitTestMethodElement), typeof (XunitTestClassElement)});
        }

        public XunitTestProvider(ISolution solution,
                                 CacheManager cacheManager,
                                 PsiModuleManager psiModuleManager,
                                 UnitTestingCategoriesProvider categoriesProvider)
        {
            this.solution = solution;
            this.cacheManager = cacheManager;
        }

        #region IUnitTestProvider Members

        public Image Icon
        {
            get { return Resources.xunit; }
        }

        public ISolution Solution
        {
            get { return solution; }
        }

        public IUnitTestViewElement DeserializeElement(XmlElement parent, IUnitTestViewElement parentElement)
        {
            return null;
        }

        public bool IsSupported(IHostProvider hostProvider)
        {
            return true;
        }

        public int CompareUnitTestElements(IUnitTestElement x, IUnitTestElement y)
        {
            return comparer.Compare(x, y);
        }

        public void SerializeElement(XmlElement parent, IUnitTestElement element)
        {
        }


        public void ExploreExternal(UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore anything that's not a part of the solution + projects world
        }

        public void ExploreFile(IFile psiFile,
                                UnitTestElementLocationConsumer consumer,
                                CheckForInterrupt interrupted)
        {
            if (psiFile == null)
                throw new ArgumentNullException("psiFile");

            psiFile.ProcessDescendants(new XunitFileExplorer(this, consumer, psiFile, interrupted, cacheManager));
        }

        public void ExploreSolution(ISolution solution, UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore the solution, without going into the projects
        }

        // It's rather useful to put a breakpoint here. When this gets hit, you can then attach
        // to the task runner process

        // Used to discover the type of the element - unknown, test, test container (class) or
        // something else relating to a test element (e.g. parent class of a nested test class)
        // This method is called to get the icon for the completion lists, amongst other things
        public bool IsElementOfKind(IDeclaredElement declaredElement, UnitTestElementKind elementKind)
        {
            switch (elementKind)
            {
                case UnitTestElementKind.Unknown:
                    return !UnitTestElementIdentifier.IsAnyUnitTestElement(declaredElement);

                case UnitTestElementKind.Test:
                    return UnitTestElementIdentifier.IsUnitTest(declaredElement);

                case UnitTestElementKind.TestContainer:
                    return UnitTestElementIdentifier.IsUnitTestContainer(declaredElement);

                case UnitTestElementKind.TestStuff:
                    return UnitTestElementIdentifier.IsUnitTestStuff(declaredElement);
            }

            return false;
        }

        public bool IsElementOfKind(IUnitTestElement element, UnitTestElementKind elementKind)
        {
            switch (elementKind)
            {
                case UnitTestElementKind.Unknown:
                    return !(element is XunitTestElementBase);

                case UnitTestElementKind.Test:
                    return element is XunitTestMethodElement;

                case UnitTestElementKind.TestContainer:
                    return element is XunitTestClassElement;

                case UnitTestElementKind.TestStuff:
                    return element is XunitTestElementBase;
            }

            return false;
        }

        #endregion

        public XunitTestClassElement GetOrCreateClassElement(string id, IProject project)
        {
            IUnitTestElement element = UnitTestManager.GetInstance(Solution).GetElementById(project, id);
            if (element != null)
            {
                return (element as XunitTestClassElement);
            }
            return new XunitTestClassElement(this, project, id, UnitTestManager.GetOutputAssemblyPath(project).FullPath);
        }

        public XunitTestMethodElement GetOrCreateMethodElement(string id, IProject project, XunitRunnerTestClassElement parent)
        {
            IUnitTestElement element = UnitTestManager.GetInstance(Solution).GetElementById(project, id);
            if (element != null)
            {
                return (element as XunitTestMethodElement);
            }
            string[] splitted = id.Split('.');
            string declaringTypeName = splitted.Take((splitted.Length - 1)).Join(".");
            string name = splitted.Last();
            return new XunitTestMethodElement(this, parent, project, declaringTypeName, name);
        }
    }

    public static class StringUtil
    {
        public static string Join(this IEnumerable<string> strings, string separator)
        {
            return string.Join(separator, strings.ToArray());
        }
    }
}
