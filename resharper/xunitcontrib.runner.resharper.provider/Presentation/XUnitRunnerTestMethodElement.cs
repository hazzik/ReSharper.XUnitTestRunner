namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.ReSharper.TaskRunnerFramework;
    using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
    using RemoteRunner;

    internal class XUnitRunnerTestMethodElement : XUnitTestElementBase, IEquatable<XUnitRunnerTestMethodElement>
    {
        protected XUnitRunnerTestMethodElement(IUnitTestRunnerProvider provider,
                                               XUnitTestClassElement @class,
                                               string typeName,
                                               string methodName)
            : base(provider, @class)
        {
            Class = @class;
            TypeName = typeName;
            MethodName = methodName;
        }

        public XUnitTestClassElement Class { get; private set; }

        public string MethodName { get; private set; }

        public override string ShortName
        {
            get { return MethodName; }
        }

        public override string Id
        {
            get { return (Class.TypeName + "." + MethodName); }
        }

        public string TypeName { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as XUnitRunnerTestMethodElement);
        }

        public override bool Equals(IUnitTestElement other)
        {
            return Equals(other as XUnitRunnerTestMethodElement);
        }

        public bool Equals(XUnitRunnerTestMethodElement other)
        {
            return (other != null && Equals(MethodName, other.MethodName)) && Equals(TypeName, other.TypeName);
        }

        public override int GetHashCode()
        {
            int result = 0;
            result = (result * 397) ^ TypeName.GetHashCode();
            return ((result * 397) ^ MethodName.GetHashCode());
        }

        public override IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            XUnitTestClassElement testClass = Class;

            return new List<UnitTestTask>
                       {
                           new UnitTestTask(null, CreateAssemblyTask(testClass.AssemblyLocation)),
                           new UnitTestTask(testClass, CreateClassTask(testClass, explicitElements)),
                           new UnitTestTask(this, CreateMethodTask(this, explicitElements))
                       };
        }

        private static RemoteTask CreateAssemblyTask(string assemblyLocation)
        {
            return new XunitTestAssemblyTask(assemblyLocation);
        }

        private static RemoteTask CreateClassTask(XUnitRunnerTestClassElement testClass, IEnumerable<IUnitTestElement> explicitElements)
        {
            return new XunitTestClassTask(testClass.AssemblyLocation, testClass.TypeName, explicitElements.Contains(testClass));
        }

        private static RemoteTask CreateMethodTask(XUnitRunnerTestMethodElement testMethod, IEnumerable<IUnitTestElement> explicitElements)
        {
            return new XunitTestMethodTask(testMethod.Class.AssemblyLocation, testMethod.Class.TypeName, testMethod.MethodName, explicitElements.Contains(testMethod));
        }
    }
}
