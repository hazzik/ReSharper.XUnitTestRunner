using System;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using System.Collections.Generic;
    using JetBrains.Application;
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.ReSharper.TaskRunnerFramework;
    using RemoteRunner;

    internal class XunitTestElementMethod : XUnitTestElementBase, IUnitTestViewElement
    {
        readonly XUnitTestClassElement @class;
        readonly string methodName;
        readonly int order;

        internal XunitTestElementMethod(IUnitTestRunnerProvider provider,
                                        XUnitTestClassElement @class,
                                        IProject project,
                                        string declaringTypeName,
                                        string methodName,
                                        int order)
            : this(provider, @class, project, declaringTypeName)
        {
            this.@class = @class;
            this.methodName = methodName;
            this.order = order;
        }

        internal XUnitTestClassElement Class
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

        public virtual IDeclaredElement GetDeclaredElement()
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

        public virtual string Kind
        {
            get { return "xUnit.net Test"; }
        }

        public virtual string GetTitle()
        {
            return string.Format("{0}.{1}", @class.GetTitle(), methodName);
        }

        public virtual bool Equals(IUnitTestViewElement other)
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

        public virtual UnitTestElementDisposition GetDisposition()
        {
            var element = GetDeclaredElement();
            if(element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            var locations = from declaration in element.GetDeclarations()
                            let file = declaration.GetContainingFile()
                            where file != null
                            select
                                new UnitTestElementLocation(file.GetSourceFile().ToProjectFile(),
                                                            declaration.GetNameDocumentRange().TextRange,
                                                            declaration.GetDocumentRange().TextRange);

            return new UnitTestElementDisposition(locations.ToList(), this);
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

        private static RemoteTask CreateClassTask(XUnitTestClassElement testClass, IEnumerable<IUnitTestElement> explicitElements)
        {
            return new XunitTestClassTask(testClass.AssemblyLocation, testClass.TypeName, explicitElements.Contains(testClass));
        }

        private static RemoteTask CreateMethodTask(XunitTestElementMethod testMethod, IEnumerable<IUnitTestElement> explicitElements)
        {
            return new XunitTestMethodTask(testMethod.Class.AssemblyLocation, testMethod.Class.TypeName, testMethod.MethodName, explicitElements.Contains(testMethod));
        }

        public virtual IProject GetProject()
        {
            return project;
        }

        public virtual IProjectModelElementPointer GetProjectPointer()
        {
            return projectPointer;
        }

        public virtual string GetTypeClrName()
        {
            return TypeName;
        }

        protected virtual ITypeElement GetDeclaredType()
        {
            IProject project = GetProject();
            if(project == null)
            {
                return null;
            }
            PsiManager manager = PsiManager.GetInstance(project.GetSolution());
            using (ReadLockCookie.Create())
            {
                return CacheManager.GetInstance(manager.Solution)
                    .GetDeclarationsCache(PsiModuleManager.GetInstance(project.GetSolution()).GetPrimaryPsiModule(project), true, true)
                    .GetTypeElementByCLRName(TypeName);
            }
        }

        public virtual UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(new ClrTypeName(TypeName).GetNamespaceName());
        }

        private string TypeName { get; set; }
        private readonly IProject project;
        private readonly IProjectModelElementPointer projectPointer;

        private XunitTestElementMethod(IUnitTestRunnerProvider provider,
		                           XUnitTestElementBase parent,
		                           IProject project,
		                           string typeName)
			: base(provider, parent)
		{
			if(project == null)
				throw new ArgumentNullException("project");

			if(typeName == null)
				throw new ArgumentNullException("typeName");

			this.project = project;
			TypeName = typeName;
			projectPointer = project.CreatePointer();
		}

//        public override bool Equals(object obj)
//		{
//			if(base.Equals(obj))
//			{
//				var element = (XunitTestElement) obj;
//
//				if(Equals(element.project, project))
//					return (element.TypeName == TypeName);
//			}
//
//			return false;
//		}
//
//		public override int GetHashCode()
//		{
//			unchecked
//			{
//				var result = base.GetHashCode();
//				result = (result*397) ^ (project != null ? project.GetHashCode() : 0);
//				result = (result*397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
//				return result;
//			}
//		}

//        public override bool Equals(object obj)
//        {
//            return (ReferenceEquals(this, obj) ||
//                    ((obj.GetType() == GetType()) && (Provider == ((UnitTestElement) obj).Provider)));
//        }

//        public override int GetHashCode()
//        {
//            return Provider.ID.GetHashCode();
//        }
    }
}