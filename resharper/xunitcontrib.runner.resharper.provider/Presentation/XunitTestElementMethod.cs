using System;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class XunitTestElementMethod : XunitTestElement
    {
        readonly XunitTestElementClass @class;
        readonly string methodName;
        readonly int order;

        internal XunitTestElementMethod(IUnitTestProvider provider,
                                        XunitTestElementClass @class,
                                        IProject project,
                                        string declaringTypeName,
                                        string methodName,
                                        int order)
            : base(provider, @class, project, declaringTypeName)
        {
            this.@class = @class;
            this.methodName = methodName;
            this.order = order;
        }

        internal XunitTestElementClass Class
        {
            get { return @class; }
        }

        internal string MethodName
        {
            get { return methodName; }
        }

        public override string ShortName
        {
            get { return methodName; }
        }

        public override bool Equals(IUnitTestElement other)
        {
            return Equals(other as object);
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            var declaredType = GetDeclaredType();
            if (declaredType != null)
            {
                return (from member in declaredType.EnumerateMembers(methodName, true)
                        let method = member as IMethod
                        where method != null && !method.IsAbstract && method.TypeParameters.Length <= 0 && method.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC
                        select member).FirstOrDefault();
            }

            return null;
        }

        public override string Kind
        {
            get { return "xUnit.net Test"; }
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1}", @class.GetTitle(), methodName);
        }

        public override bool Equals(IUnitTestViewElement other)
        {
            return Equals(other as object);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                var elementMethod = (XunitTestElementMethod)obj;

                bool returnValue = false;
                if (Equals(@class, elementMethod.@class))
                    returnValue = (methodName == elementMethod.methodName);
                return returnValue;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = base.GetHashCode();
                result = (result * 397) ^ (@class != null ? @class.GetHashCode() : 0);
                result = (result * 397) ^ (methodName != null ? methodName.GetHashCode() : 0);
                result = (result * 397) ^ order;
                return result;
            }
        }

        public override string Id
        {
            get
            {
                return (@class.TypeName + "." + methodName);
            }
        }
    }
}