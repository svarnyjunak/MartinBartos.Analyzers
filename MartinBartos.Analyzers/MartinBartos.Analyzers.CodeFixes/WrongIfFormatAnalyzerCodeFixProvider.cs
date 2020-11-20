using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MartinBartos.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(WrongIfFormatAnalyzerCodeFixProvider)), Shared]
    public class WrongIfFormatAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(WrongIfFormatAnalyzer.DiagnosticId); }
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

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<IfStatementSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.WrongIfFormatCodeFixTitle,
                    createChangedDocument: c => AddWhitespaceAsync(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.WrongIfFormatCodeFixTitle)),
                diagnostic);

            declaration.ChildTokens().First().TrailingTrivia.Add(SyntaxFactory.Whitespace(" "));
        }

        private async Task<Document> AddWhitespaceAsync(Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            var firstToken = ifStatement.ChildTokens().First();
            var formatedIfStatement = ifStatement.ReplaceToken(firstToken, firstToken.WithTrailingTrivia(SyntaxFactory.Whitespace(" ")));

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(ifStatement, formatedIfStatement);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
