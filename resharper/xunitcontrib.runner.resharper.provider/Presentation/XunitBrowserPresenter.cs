namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Presentation
{
    using System.Drawing;
    using JetBrains.CommonControls;
    using JetBrains.ReSharper.Features.Common.TreePsiBrowser;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.UnitTestExplorer;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.ReSharper.UnitTestFramework.UI;
    using JetBrains.TreeModels;
    using JetBrains.UI.TreeView;

    internal class XunitBrowserPresenter : TreeModelBrowserPresenter
	{
		internal XunitBrowserPresenter()
		{
			Present<XunitTestClassElement>(PresentTestFixture);
			Present<XunitTestMethodElement>(PresentTest);
		}

		protected override bool IsNaturalParent(object parentValue,
		                                        object childValue)
		{
			var @namespace = parentValue as UnitTestNamespace;
			var test = childValue as XunitTestClassElement;

			if(test != null && @namespace != null)
				return @namespace.Equals(test.GetNamespace());

			return base.IsNaturalParent(parentValue, childValue);
		}

		private static void PresentTest(XunitTestMethodElement value,
		                                IPresentableItem item,
		                                TreeModelNode modelNode,
		                                PresentationState state)
		{
			item.RichText = value.Class.TypeName != value.TypeName
			                	? string.Format("{0}.{1}", new ClrTypeName(value.TypeName).ShortName, value.MethodName)
			                	: value.MethodName;

			if(value.Explicit)
				item.RichText.SetForeColor(SystemColors.GrayText);

			var stateImage = UnitTestIconManager.GetStateImage(state);
			var typeImage = UnitTestIconManager.GetStandardImage(UnitTestElementImage.Test);

			if(stateImage != null)
				item.Images.Add(stateImage);
			else if(typeImage != null)
				item.Images.Add(typeImage);
		}

		private void PresentTestFixture(XunitTestClassElement value,
		                                IPresentableItem item,
		                                TreeModelNode modelNode,
		                                PresentationState state)
		{
			var name = new ClrTypeName(value.TypeName);

			if(IsNodeParentNatural(modelNode, value))
				item.RichText = name.ShortName;
			else
				item.RichText = string.IsNullOrEmpty(name.GetNamespaceName())
				                	? name.ShortName
				                	: string.Format("{0}.{1}", name.GetNamespaceName(), name.ShortName);

			var stateImage = UnitTestIconManager.GetStateImage(state);
			var typeImage = UnitTestIconManager.GetStandardImage(UnitTestElementImage.TestContainer);

			if(stateImage != null)
				item.Images.Add(stateImage);
			else if(typeImage != null)
				item.Images.Add(typeImage);

			AppendOccurencesCount(item, modelNode, "test");
		}

		protected override object Unwrap(object value)
		{
			if(value is XunitTestMethodElement || value is XunitTestClassElement)
				value = ((IUnitTestViewElement) value).GetDeclaredElement();

			return base.Unwrap(value);
		}
	}
}