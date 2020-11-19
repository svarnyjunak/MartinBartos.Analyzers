using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = MartinBartos.Analyzers.Test.CSharpCodeFixVerifier<
    MartinBartos.Analyzers.WrongIfFormatAnalyzer,
    MartinBartos.Analyzers.WrongIfFormatAnalyzerCodeFixProvider>;

namespace MartinBartos.Analyzers.Test
{
    [TestClass]
    public class WrongIfFormatAnalyzerUnitTests
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task NoDiagnosticsExpected()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task DiagnosticAndCodeFixExpected()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class ClassName
        {   
            void TestMethod1()
            {
                if(true)
                {
                }
            }
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class ClassName
        {   
            void TestMethod1()
            {
                if (true)
                {
                }
            }
        }
    }";

            var expected = VerifyCS.Diagnostic(WrongIfFormatAnalyzer.DiagnosticId).WithLocation(15, 17);
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
