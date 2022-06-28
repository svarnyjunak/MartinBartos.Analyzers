using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = MartinBartos.Analyzers.Test.CSharpCodeFixVerifier<
    MartinBartos.Analyzers.FileScopedNamespaceAnalyzer, 
    MartinBartos.Analyzers.FileScopedNamespaceAnalyzerCodeFixProvider>;

namespace MartinBartos.Analyzers.Test;

[TestClass]
public class FileScopedNamespaceAnalyzerUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task DiagnosticAndCodeFixExpected()
    {
        var test = @"
using System;

namespace Namespace.AnotherLevel;
class ClassName
{   
    void TestMethod1()
    {
    }
}";

        var fixtest = @"
using System;

namespace Namespace.AnotherLevel;

class ClassName
{   
    void TestMethod1()
    {
    }
}";

        var expected = VerifyCS.Diagnostic(FileScopedNamespaceAnalyzer.DiagnosticId).WithLocation(5, 1);
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task DiagnosticAndCodeFixForClassWithComment()
    {
        var test = @"
using System;

namespace Namespace.AnotherLevel;
/// <summary>
/// Test
/// </summary>
class ClassName
{   
    void TestMethod1()
    {
    }
}";

        var fixtest = @"
using System;

namespace Namespace.AnotherLevel;

/// <summary>
/// Test
/// </summary>
class ClassName
{   
    void TestMethod1()
    {
    }
}";

        var expected = VerifyCS.Diagnostic(FileScopedNamespaceAnalyzer.DiagnosticId).WithLocation(8, 1);
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task DiagnosticAndCodeFixForClassWithAttribute()
    {
        var test = @"
using System;

namespace Namespace.AnotherLevel;
[Serializable]
class ClassName
{   
    void TestMethod1()
    {
    }
}";

        var fixtest = @"
using System;

namespace Namespace.AnotherLevel;

[Serializable]
class ClassName
{   
    void TestMethod1()
    {
    }
}";

        var expected = VerifyCS.Diagnostic(FileScopedNamespaceAnalyzer.DiagnosticId).WithLocation(5, 1);
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
