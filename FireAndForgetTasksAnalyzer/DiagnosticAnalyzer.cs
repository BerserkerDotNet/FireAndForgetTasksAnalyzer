using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace FireAndForgetTasksAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FireAndForgetTasksAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FireAndForgetTasksAnalyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Async/Await";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeLocalSymbol, SyntaxKind.VariableDeclarator);
        }

        private void AnalyzeLocalSymbol(SyntaxNodeAnalysisContext context)
        {
            var variableDeclarator = (VariableDeclaratorSyntax)context.Node;
            var variableDeclaration = variableDeclarator.Parent as VariableDeclarationSyntax;
            if (variableDeclaration == null || !variableDeclaration.Type.IsVar || variableDeclarator.Initializer == null)
                return;

            var type = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).ConvertedType;
            if (type.Name != "Task")
                return;

            var awaitExpression  = variableDeclarator.Initializer.Value as AwaitExpressionSyntax;
            if (awaitExpression != null)
                return;

            var methodDeclaration = variableDeclaration.GetParentOfType<MethodDeclarationSyntax>();
            var awaiterExpression = methodDeclaration.DescendantNodes()
                .OfType<AwaitExpressionSyntax>()
                .SingleOrDefault(n => (n.Expression is IdentifierNameSyntax) 
                && ((IdentifierNameSyntax)n.Expression).Identifier.Text == variableDeclarator.Identifier.Text);

            if (awaiterExpression != null)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, variableDeclarator.GetLocation(), variableDeclarator.Identifier.Text));
        }
    }
}
