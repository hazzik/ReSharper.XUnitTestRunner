﻿namespace ReSharper.XUnitTestProvider
{
    using JetBrains.ReSharper.UnitTestFramework;
    using JetBrains.ReSharper.UnitTestFramework.Strategy;

    public partial class XunitTestFakeElement
    {
        public IUnitTestRunStrategy GetRunStrategy(IHostProvider hostProvider)
        {
            return new DoNothingRunStrategy();
        }
    }
}