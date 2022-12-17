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
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MultipleLinesAnalyzer.DiagnosticId);

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
                formatedNode = formatedNode.ReplaceToken(token, token.WithLeadingTrivia(GetWithoutDuplicitEmptyLines(token.LeadingTrivia)));
            }

            if (MultipleLinesAnalyzer.ContainsMultipleLines(token.TrailingTrivia))
            {
                formatedNode = formatedNode.ReplaceToken(token, token.WithLeadingTrivia(GetWithoutDuplicitEmptyLines(token.TrailingTrivia)));
            }

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(node, formatedNode);

            return document.WithSyntaxRoot(newRoot);
        }

        private IEnumerable<SyntaxTrivia> GetWithoutDuplicitEmptyLines(SyntaxTriviaList list)
        {
            var previous = list[0];

            for (int i = 1; i < list.Count; i++)
            {
                var current = list[i];

                if (!previous.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    yield return previous;
                }
                else if (!current.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    yield return previous;
                }

                previous = current;
            }

            yield return previous;
        }
    }
}
