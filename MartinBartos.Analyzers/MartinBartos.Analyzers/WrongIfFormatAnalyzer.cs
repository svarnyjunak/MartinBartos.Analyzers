using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace MartinBartos.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WrongIfFormatAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WrongIfFormatAnalyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.WrongIfFormatAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.WrongIfFormatAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.WrongIfFormatAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
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
                foreach (var statement in root.DescendantNodes().OfType<StatementSyntax>())
                {
                    if (statement is IfStatementSyntax)
                    {
                        if (statement.ChildTokens().First().TrailingTrivia.Count == 0)
                        {
                            var diagnostic = Diagnostic.Create(Rule, statement.GetLocation());
                            syntaxTreeContext.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            });
        }
    }
}
