using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;
using System.Reflection;

namespace tests.xunit
{
    namespace CustomAttributes
    {
        public class MyFactAttribute : FactAttribute
        {
            // Do nothing
        }

        public class CustomFactAttribute
        {
            // TEST: Should be flagged as a test class
            [MyFact]
            public void MyTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }
#if XUNIT_1_1
        public class MyTheoryAttribute : TheoryAttribute
        {
            public int Repeat { get; set; }

            protected override IEnumerable<ITestCommand> EnumerateTestCommands(MethodInfo method)
            {
                for (int i = 0; i < Repeat; i++)
                    yield return new TestCommand(method, Name);
            }
        }
#endif
#if XUNIT_1_5
        public class MyTheoryAttribute : TheoryAttribute
        {
            public int Repeat { get; set; }

            protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
            {
                for (int i = 0; i < Repeat; i++)
                    yield return new TestCommand(method, DisplayName);
            }
        }
#endif
#if XUNIT_1_6 || XUNIT_1_6_1
        public class MyTheoryAttribute : TheoryAttribute
        {
            public int Repeat { get; set; }

            protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
            {
                for (int i = 0; i < Repeat; i++)
                    yield return new FactCommand(method);
            }
        }
#endif
        public class CustomTheoryAttribute
        {
            // TEST: Should be flagged as a test
            // TEST: Should output CustomTheoryAttribute.MyTestMethod 5 times
            [MyTheory(Repeat = 5)]
            public void MyTestMethod()
            {
                Console.WriteLine("CustomTheoryAttribute.MyTestMethod");
            }
        }
    }
}