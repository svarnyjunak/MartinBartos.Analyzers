using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace MartinBartos.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MultipleLinesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MB1010";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MultipleLinesAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MultipleLinesAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MultipleLinesAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Formatting";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxTreeAction(syntaxTreeContext =>
            {
                var root = syntaxTreeContext.Tree.GetRoot(syntaxTreeContext.CancellationToken);
                foreach (var node in root.DescendantNodes())
                {
                    foreach (var token in node.ChildTokens())
                    {
                        if (token.HasLeadingTrivia && ContainsMultipleLines(token.LeadingTrivia))
                        {
                            var diagnostic = Diagnostic.Create(Rule, token.GetLocation());
                            syntaxTreeContext.ReportDiagnostic(diagnostic);
                        }

                        if (token.HasTrailingTrivia && ContainsMultipleLines(token.TrailingTrivia))
                        {
                            var diagnostic = Diagnostic.Create(Rule, token.GetLocation());
                            syntaxTreeContext.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            });
        }

        public static bool ContainsMultipleLines(SyntaxTriviaList triviaList)
        {
            if (triviaList.Any())
            {
                var previous = triviaList.First();

                foreach (var trivia in triviaList.Skip(1))
                {
                    if (previous.IsKind(SyntaxKind.EndOfLineTrivia) &&
                        trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                    {
                        return true;
                    }

                    previous = trivia;
                }
            }

            return false;
        }
    }
}
