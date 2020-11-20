using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MartinBartos.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MultipleLinesAnalyzerCodeFixProvider)), Shared]
    public class MultipleLinesAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(MultipleLinesAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var token = root.FindToken(diagnosticSpan.Start);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.MultipleLinesCodeFixTitle,
                    createChangedDocument: c => RemoveMultipleEmptyLinesAsync(context.Document, token, c),
                    equivalenceKey: nameof(CodeFixResources.MultipleLinesCodeFixTitle)),
                diagnostic);
        }

        private async Task<Document> RemoveMultipleEmptyLinesAsync(Document document, SyntaxToken token, CancellationToken cancellationToken)
        {
            var node = token.Parent;
            var formatedNode = node;
            
            if (MultipleLinesAnalyzer.ContainsMultipleLines(token.LeadingTrivia))
            {
                formatedNode = formatedNode.ReplaceToken(token, token.WithLeadingTrivia(token.LeadingTrivia.Where(t => !t.IsKind(SyntaxKind.EndOfLineTrivia))));
            }

            if (MultipleLinesAnalyzer.ContainsMultipleLines(token.TrailingTrivia))
            {
                formatedNode = formatedNode.ReplaceToken(token, token.WithTrailingTrivia(token.TrailingTrivia.Where(t => !t.IsKind(SyntaxKind.EndOfLineTrivia))));
            }

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(node, formatedNode);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
