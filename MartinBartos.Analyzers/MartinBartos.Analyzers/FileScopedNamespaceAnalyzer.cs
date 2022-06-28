using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;

namespace MartinBartos.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FileScopedNamespaceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MB1000";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FileScopedNamespaceAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.FileScopedNamespaceAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.FileScopedNamespaceAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        private const string Category = "Formatting";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxTreeAction(syntaxTreeContext =>
            {
                var checkLeadingEmptyLine = false;
                var root = syntaxTreeContext.Tree.GetRoot(syntaxTreeContext.CancellationToken);
                foreach (var node in root.DescendantNodes())
                {
                    if (node is FileScopedNamespaceDeclarationSyntax)
                    {
                        var tokens = node.ChildTokens();
                        var lastToken = tokens.Last();

                        // If there is file scoped namespace declaration,
                        // we should check if there is leading empty line.
                        checkLeadingEmptyLine = true;
                    }
                    else if (checkLeadingEmptyLine)
                    {
                        if (node is TypeDeclarationSyntax typeDeclaration)
                        {
                            var tokens = typeDeclaration.ChildTokens();
                            var firstToken = tokens.First();
                            var diagnosticStart = firstToken;

                            // If type declaration contains attribute,
                            // we should check if there is empty line beore first attribute.
                            if (typeDeclaration.AttributeLists.Any())
                            {
                                firstToken = typeDeclaration.AttributeLists.First().ChildTokens().First();
                            }

                            if (!ContainsLine(firstToken.LeadingTrivia))
                            {
                                var identifierToken = tokens.FirstOrDefault(t => t.IsKind(SyntaxKind.IdentifierToken));

                                var span = new TextSpan(firstToken.SpanStart, identifierToken.Span.End - firstToken.SpanStart);
                                var location = Location.Create(syntaxTreeContext.Tree, span);
                                var diagnostic = Diagnostic.Create(Rule, location);
                                syntaxTreeContext.ReportDiagnostic(diagnostic);
                            }

                            checkLeadingEmptyLine = false;
                        }
                    }
                }
            });
        }

        public static bool ContainsLine(SyntaxTriviaList triviaList)
        {
            if (triviaList.Any())
            {
                var firstTrivia = triviaList.First();
                return firstTrivia.IsKind(SyntaxKind.EndOfLineTrivia);
            }

            return false;
        }
    }
}
