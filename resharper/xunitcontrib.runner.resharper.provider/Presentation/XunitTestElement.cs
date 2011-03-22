using System;
using System.Linq;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
	internal abstract class XunitTestElement : UnitTestElement, IUnitTestViewElement
	{
		public readonly string TypeName;
		private readonly IProject project;
		private readonly IProjectModelElementPointer projectPointer;

		protected XunitTestElement(IUnitTestProvider provider,
		                           UnitTestElement parent,
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

		protected ITypeElement GetDeclaredType()
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

		public virtual UnitTestNamespace GetNamespace()
		{
			return new UnitTestNamespace(new ClrTypeName(TypeName).GetNamespaceName());
		}

		public override IProject GetProject()
		{
			return project;
		}

		public virtual IProjectModelElementPointer GetProjectPointer()
		{
			return projectPointer;
		}

		public string GetTypeClrName()
		{
			return TypeName;
		}

		public override bool Equals(object obj)
		{
			if(base.Equals(obj))
			{
				var element = (XunitTestElement) obj;

				if(Equals(element.project, project))
					return (element.TypeName == TypeName);
			}

			return false;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = base.GetHashCode();
				result = (result*397) ^ (project != null ? project.GetHashCode() : 0);
				result = (result*397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
				return result;
			}
		}
	}
}