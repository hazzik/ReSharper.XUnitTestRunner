﻿using Xunit;

namespace tests.xunit
{
    namespace MixedMethodVisibility
    {
        public class PublicClass
        {
			//CONSTRUCTOR: should not be be flagged as 'Can be made private'
            public PublicClass()
            {
                Assert.Equal(1, 1);
            }

			//CONSTRUCTOR: should be flagged as 'Can be made private'
        	public PublicClass(int a)
        	{
        		Assert.Equal(1, 1);
        	}

            // TEST: Method should be flagged and should run as a test
            [Fact]
            public void PublicMethod()
            {
                Assert.Equal(1, 1);
            }

            // TEST: Method should be flagged and should run as a test
            [Fact]
            public static void PublicStaticMethod()
            {
                Assert.Equal(1, 1);
            }

            // xunit 1.1 only allows public methods to be tests. 1.5 allows any method
#if false
            // TEST: Method should be flagged and should run as a test
            [Fact]
            protected void ProtectedMethod()
            {
                Assert.Equal(1, 1);
            }

            // TEST: Method should be flagged and should run as a test
            [Fact]
            protected static void ProtectedStaticMethod()
            {
                Assert.Equal(1, 1);
            }

            // TEST: Method should be flagged and should run as a test
            [Fact]
            internal void InternalMethod()
            {
                Assert.Equal(1, 1);
            }

            // TEST: Method should be flagged and should run as a test
            [Fact]
            internal static void InternalStaticMethod()
            {
                Assert.Equal(1, 1);
            }

            // TEST: Method should be flagged and should run as a test
            [Fact]
            private void PrivateMethod()
            {
                Assert.Equal(1, 1);
            }

            // TEST: Method should be flagged and should run as a test
            [Fact]
            private static void PrivateInternalStaticMethod()
            {
                Assert.Equal(1, 1);
            }

            // TEST: Method should be flagged and should run as a test
            [Fact]
            protected internal void ProtectedInternalMethod()
            {
                Assert.Equal(1, 1);
            }

            // TEST: Method should be flagged and should run as a test
            [Fact]
            protected internal static void ProtectedInternalStaticMethod()
            {
                Assert.Equal(1, 1);
            }
#endif
        }

        class PrivateClass
        {
            // TEST: Method should not be flagged or run as a test
            [Fact]
            public void PublicMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        internal class InternalClass
        {
            // TEST: Method should not be flagged or run as a test
            [Fact]
            public void PublicMethod()
            {
                Assert.Equal(1, 1);
            }
        }
    }
}
