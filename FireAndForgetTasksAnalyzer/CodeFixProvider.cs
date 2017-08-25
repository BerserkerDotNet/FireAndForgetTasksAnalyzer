using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FireAndForgetTasksAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FireAndForgetTasksAnalyzerCodeFixProvider)), Shared]
    public class FireAndForgetTasksAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Await expression";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(FireAndForgetTasksAnalyzerAnalyzer.DiagnosticId); }
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
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => AwaitLocalVariable(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> AwaitLocalVariable(Document document, VariableDeclaratorSyntax variableDeclaration, CancellationToken cancellationToken)
        {
            var oldValue = variableDeclaration.Initializer.Value;
            var awaitExp = SyntaxFactory.AwaitExpression(oldValue);
            var newInitializer = variableDeclaration.Initializer
                .WithValue(awaitExp);
            var methodDecl = variableDeclaration.GetParentOfType<MethodDeclarationSyntax>();
            var newMethodDecl = methodDecl.ReplaceNode(variableDeclaration.Initializer, newInitializer);

            var asyncToken = newMethodDecl.ChildTokens().SingleOrDefault(t => t.IsKind(SyntaxKind.AsyncKeyword));
            if (asyncToken.IsKind(SyntaxKind.None))
            {
                newMethodDecl = newMethodDecl.AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
            }

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(methodDecl, newMethodDecl);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}