using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
	internal static class MethodWrapper
	{
		public static IMethodInfo AsMethodInfo(this IMethod method)
		{
			return new PsiMethodWrapper(method);
		}

		public static IMethodInfo AsMethodInfo(this IMetadataMethod method)
		{
			return new MetadataMethodWrapper(method);
		}

		private class MetadataMethodWrapper : IMethodInfo
		{
			private readonly IMetadataMethod metadataMethod;

			public MetadataMethodWrapper(IMetadataMethod metadataMethod)
			{
				this.metadataMethod = metadataMethod;
			}

			public string TypeName
			{
				get { return metadataMethod.DeclaringType.FullyQualifiedName; }
			}

			public bool IsAbstract
			{
				get { return metadataMethod.IsAbstract; }
			}

			public bool IsStatic
			{
				get { return metadataMethod.IsStatic; }
			}

			public MethodInfo MethodInfo
			{
				get { return null; }
			}

			public string Name
			{
				get { return metadataMethod.Name; }
			}

			public string ReturnType
			{
				get { return metadataMethod.ReturnValue.Type.AssemblyQualifiedName; }
			}

			public void Invoke(object testClass, params object[] parameters)
			{
				throw new NotImplementedException();
			}

			public object CreateInstance()
			{
				throw new NotImplementedException();
			}

			// Get any attributes that are of type attributeType, or that are assignable to attributeType
			// (i.e. attributes that are subclasses of attributeType)
			public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
			{
				return from attribute in metadataMethod.CustomAttributes
				       where attributeType.IsAssignableFrom(attribute.UsedConstructor.DeclaringType)
				       select attribute.AsAttributeInfo();
			}

			public bool HasAttribute(Type attributeType)
			{
				return GetCustomAttributes(attributeType).Any();
			}

			public override string ToString()
			{
				return string.Format("{0} {1}({2})",
				                     metadataMethod.ReturnValue.Type.AssemblyQualifiedName,
				                     metadataMethod.Name,
				                     string.Join(", ", metadataMethod.Parameters.Select(param => param.Type.AssemblyQualifiedName).ToArray()));
			}
		}

		private class PsiMethodWrapper : IMethodInfo
		{
			private readonly IMethod method;

			public PsiMethodWrapper(IMethod method)
			{
				this.method = method;
			}

			public string TypeName
			{
				get { return method.GetContainingType().GetClrName().FullName; }
			}

			public bool IsAbstract
			{
				get { return method.IsAbstract; }
			}

			public bool IsStatic
			{
				get { return method.IsStatic; }
			}

			public MethodInfo MethodInfo
			{
				get { return null; }
			}

			public string Name
			{
				get { return method.ShortName; }
			}

			public string ReturnType
			{
				get
				{
					var type = method.ReturnType as IDeclaredType;
					return type == null ? null : type.GetClrName().FullName;
				}
			}

			public void Invoke(object testClass, params object[] parameters)
			{
				throw new NotImplementedException();
			}

			public object CreateInstance()
			{
				throw new NotImplementedException();
			}

			public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
			{
				return from attribute in method.GetAttributeInstances(false)
				       where attributeType.IsAssignableFrom(attribute.AttributeType)
				       select attribute.AsAttributeInfo();
			}

			public bool HasAttribute(Type attributeType)
			{
				return method.GetAttributeInstances(false).Any(attribute => attributeType.IsAssignableFrom(attribute.AttributeType));
			}

			public override string ToString()
			{
				var language = KnownLanguage.ANY;

				return string.Format("{0} {1}({2})",
				                     method.ReturnType.GetLongPresentableName(language),
				                     method.ShortName,
				                     string.Join(", ",
				                                 method.Parameters.Select(param => param.Type.GetLongPresentableName(language)).ToArray()));
			}
		}
	}
}