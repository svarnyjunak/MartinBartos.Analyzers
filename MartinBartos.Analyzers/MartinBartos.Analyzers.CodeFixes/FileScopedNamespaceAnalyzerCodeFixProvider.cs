using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace MartinBartos.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileScopedNamespaceAnalyzerCodeFixProvider)), Shared]
    public class FileScopedNamespaceAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(FileScopedNamespaceAnalyzer.DiagnosticId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                var token = root.FindToken(diagnosticSpan.Start);

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: CodeFixResources.FileScopedNamespaceCodeFixTitle,
                        createChangedDocument: c => AddEmptyLineAsync(context.Document, token, c),
                        equivalenceKey: nameof(CodeFixResources.FileScopedNamespaceCodeFixTitle)),
                    diagnostic);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        private async Task<Document> AddEmptyLineAsync(Document document, SyntaxToken token, CancellationToken cancellationToken)
        {
            var node = token.Parent;
            var formatedNode = node;

            var trivias = new List<SyntaxTrivia>();
            trivias.Add(SyntaxFactory.CarriageReturnLineFeed);
            trivias.AddRange(token.LeadingTrivia);
            formatedNode = formatedNode.ReplaceToken(token, token.WithLeadingTrivia(trivias));

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(node, formatedNode);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
