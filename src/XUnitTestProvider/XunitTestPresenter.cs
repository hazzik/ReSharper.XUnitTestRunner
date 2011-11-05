namespace ReSharper.XUnitTestProvider
{
    using JetBrains.CommonControls;
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.TreeModels;
    using JetBrains.UI.TreeView;

    [UnitTestPresenter]
    public class XunitTestPresenter : IUnitTestPresenter
    {
        private static readonly XunitBrowserPresenter Presenter = new XunitBrowserPresenter();

        #region IUnitTestPresenter Members

        public void Present(IUnitTestElement element, IPresentableItem presentableItem, TreeModelNode node, PresentationState state)
        {
            if ((element is XunitTestClassElement) || (element is XunitTestMethodElement))
                Presenter.UpdateItem(element, node, presentableItem, state);
        }

        #endregion
    }
}