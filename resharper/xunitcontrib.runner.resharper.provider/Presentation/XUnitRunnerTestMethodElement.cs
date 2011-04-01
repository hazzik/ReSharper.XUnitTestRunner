using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.ReSharper.TaskRunnerFramework;
    using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;

    public class XunitRunnerTestMethodElement : XunitTestElementBase, IEquatable<XunitRunnerTestMethodElement>
    {
        public XunitRunnerTestMethodElement(IUnitTestRunnerProvider provider,
                                            XunitRunnerTestClassElement @class,
                                            string typeName,
                                            string methodName)
            : base(provider, @class)
        {
            Class = @class;
            TypeName = typeName;
            MethodName = methodName;
        }

        public XunitRunnerTestClassElement Class { get; private set; }

        public string MethodName { get; private set; }

        public override sealed string ShortName
        {
            get { return MethodName; }
        }

        public override sealed string Id
        {
            get { return (Class.TypeName + "." + MethodName); }
        }

        public string TypeName { get; private set; }

        public bool Equals(XunitRunnerTestMethodElement other)
        {
            return (other != null && Equals(MethodName, other.MethodName)) && Equals(TypeName, other.TypeName);
        }

        public override sealed bool Equals(object obj)
        {
            return Equals(obj as XunitRunnerTestMethodElement);
        }

        public override sealed bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitRunnerTestMethodElement);
        }

        public override sealed int GetHashCode()
        {
            int result = 0;
            result = (result*397) ^ TypeName.GetHashCode();
            return ((result*397) ^ MethodName.GetHashCode());
        }

        public override sealed IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            XunitRunnerTestClassElement testClass = Class;

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

        private static RemoteTask CreateClassTask(XunitRunnerTestClassElement testClass, IEnumerable<IUnitTestElement> explicitElements)
        {
            return new XunitTestClassTask(testClass.AssemblyLocation, testClass.TypeName, explicitElements.Contains(testClass));
        }

        private static RemoteTask CreateMethodTask(XunitRunnerTestMethodElement testMethod, IEnumerable<IUnitTestElement> explicitElements)
        {
            return new XunitTestMethodTask(testMethod.Class.AssemblyLocation, testMethod.Class.TypeName, testMethod.MethodName, explicitElements.Contains(testMethod));
        }
    }
}
