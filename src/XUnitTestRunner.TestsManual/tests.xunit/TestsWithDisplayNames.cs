using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Sdk;
using System;

namespace tests.xunit
{
    namespace ExpectedToFail.NotYetImplemented
    // namespace TestsWithDisplayNames
    {

        public class TestsWithDisplayNames
        {
            // TEST: Name should be reported as attribute value, with no namespaces
#if XUNIT_1_1
            [NamedFact("NameComesFromAttribute")]
#else
            [Fact(Name = "NameComesFromAttribute")]
#endif
            public void ShouldNotSeeThis_NameShouldComeFromAttribute()
            {
                //Assert.Equal(1, 1);
                throw new NotImplementedException();
            }

            // TEST: Name should be reported as attribute value, with no namespaces
#if XUNIT_1_1
            [NamedFact("Name contains spaces")]
#else
            [Fact(Name = "Name contains spaces")]
#endif
            public void ShouldNotSeeThis_NameShouldContainSpaces()
            {
                //Assert.Equal(1, 1);
                throw new NotImplementedException();
            }
        }
    }

#if XUNIT_1_1
    // Unfortunately, xunit 1.1's FactAttribute's Name property can only be
    // set in a derived class. And it's not passed into the test commands either.
    // This is fixed in xunit 1.5
    public class NamedFactAttribute : FactAttribute
    {
        public NamedFactAttribute(string name)
        {
            Name = name;
        }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(MethodInfo method)
        {
            yield return new TestCommand(method, Name);
        }
    }
#endif
}
