namespace ReSharper.XUnitTestProvider
{
    using System.Collections.Generic;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Search;

    internal class XunitFileExplorerInheritorsConsumer : IFindResultConsumer<ITypeElement>
    {
        private const int MaxInheritors = 50;

        private readonly HashSet<ITypeElement> foundElements = new HashSet<ITypeElement>();

        public IEnumerable<ITypeElement> FoundElements
        {
            get { return foundElements; }
        }

        #region IFindResultConsumer<ITypeElement> Members

        public ITypeElement Build(FindResult result)
        {
            var inheritedResult = result as FindResultInheritedElement;
            if (inheritedResult == null)
            {
                return null;
            }
            return (ITypeElement) inheritedResult.DeclaredElement;
        }

        public FindExecution Merge(ITypeElement data)
        {
            foundElements.Add(data);
            return foundElements.Count < MaxInheritors
                       ? FindExecution.Continue
                       : FindExecution.Stop;
        }

        #endregion
    }
}