using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Extensions;

public class FailingTests
{
   // TEST: Should fail
   // TEST: Should display "Failed: Exception: this test should fail" as tree view status
   // TEST: Should display System.Exception: this test should fail + stack trace as main view
   // BUG: Does not display shortname in status view
   [Fact]
   public void FailsDueToThrownException()
   {
       throw new Exception("this test should fail");
   }
}
