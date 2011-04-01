namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using System;
    using System.Collections.Generic;
    using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
    using JetBrains.Util;

    public class XunitRunnerTestClassElement : XunitTestElementBase, IEquatable<XunitRunnerTestClassElement>
    {
        public XunitRunnerTestClassElement(IUnitTestRunnerProvider provider, string typeName, string assemblyLocation)
            : base(provider, null)
        {
            TypeName = typeName;
            AssemblyLocation = assemblyLocation;
        }

        public override string Id
        {
            get { return TypeName; }
        }

        public override string ShortName
        {
            get
            {
//                return new ClrTypeName(TypeName).ShortName;
                string[] splitted = TypeName.Split('.');
                return splitted[splitted.Length - 1];
            }
        }

        public string TypeName { get; private set; }

        public string AssemblyLocation { get; private set; }

        public bool Equals(XunitRunnerTestClassElement other)
        {
            return ((other != null) && Equals(TypeName, other.TypeName));
        }

        public override sealed bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitRunnerTestClassElement);
        }

        public override sealed bool Equals(object obj)
        {
            return Equals(obj as XunitRunnerTestClassElement);
        }

        public override sealed int GetHashCode()
        {
            return TypeName.GetHashCode();
        }

        public override sealed IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            // We don't have to do anything explicit for a test class, because when a class is run
            // we get called for each method, and each method already adds everything we need (loading
            // the assembly and the class)
            return EmptyArray<UnitTestTask>.Instance;
        }
    }
}
