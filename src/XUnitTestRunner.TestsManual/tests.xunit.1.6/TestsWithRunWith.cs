using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace tests.xunit
{
    public class TestsWithRunWith
    {
        [RunWith(typeof(TestClassCommand))]
        public class MyClass
        {
            [Fact]
            public void A()
            {
            }
        }

        //WHAT WE SHOULD DO?
        //THIS IS TOOOOO HARD!
        //I THINK THAT IT SHOULD BE COMPILLED FIRST?
        public class MyTestClassCommand : TestClassCommand, ITestClassCommand
        {
            bool ITestClassCommand.IsTestMethod(IMethodInfo testMethod)
            {
                return true;
            }
        }
    }
}
