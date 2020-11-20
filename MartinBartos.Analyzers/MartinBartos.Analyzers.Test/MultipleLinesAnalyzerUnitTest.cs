using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = MartinBartos.Analyzers.Test.CSharpCodeFixVerifier<
    MartinBartos.Analyzers.MultipleLinesAnalyzer,
    MartinBartos.Analyzers.MultipleLinesAnalyzerCodeFixProvider>;

namespace MartinBartos.Analyzers.Test
{
    [TestClass]
    public class MultipleLinesAnalyzerUnitTest
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
        public async Task DiagnosticAndCodeFixLeadingTriviaExpected()
        {
            var test = @"
    using System;

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

            var fixtest = @"
    using System;

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

            var expected = VerifyCS.Diagnostic(MultipleLinesAnalyzer.DiagnosticId).WithLocation(8, 9);
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task DiagnosticAndCodeFixTrailingTriviaExpected()
        {
            var test = @"
    using System;

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

            var fixtest = @"
    using System;

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

            var expected = VerifyCS.Diagnostic(MultipleLinesAnalyzer.DiagnosticId).WithLocation(17, 5);
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
