using System;
using JetBrains.CommonControls;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.TreeModels;
using JetBrains.UI.TreeView;
using XunitContrib.Runner.ReSharper.UnitTestProvider.Presentation;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [UnitTestPresenter]
    public class XunitTestPresenter : IUnitTestPresenter
    {
        private static readonly XunitBrowserPresenter presenter = new XunitBrowserPresenter();

        #region IUnitTestPresenter Members

        public void Present(IUnitTestViewElement element, IPresentableItem presentableItem, TreeModelNode node, PresentationState state)
        {
            if ((element is XunitTestClassElement) || (element is XunitTestMethodElement))
                presenter.UpdateItem(element, node, presentableItem, state);
        }

        #endregion
    }
}