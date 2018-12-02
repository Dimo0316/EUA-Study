using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.ValueType
{
    /// A diagnostic analyzer to check "boxing"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BoxingAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.Boxing); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax,
                SyntaxKind.IdentifierName,
                SyntaxKind.NumericLiteralExpression,
                SyntaxKind.AddExpression,
                SyntaxKind.SubtractExpression,
                SyntaxKind.MultiplyExpression,
                SyntaxKind.DivideExpression,
                SyntaxKind.UnaryMinusExpression);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.Boxing))
            {
                return;
            }

            // retrieve the origin type symbol and the converted type symbol
            var typeInfo = context.SemanticModel.GetTypeInfo(context.Node);
            var type = typeInfo.Type;
            var convertedType = typeInfo.ConvertedType;

            // check whether the origin type is value type and the converted type is "System.Object"
            if (type != null &&
                convertedType != null &&
                type.IsValueType &&
                convertedType.Name?.Equals("Object") == true &&
                convertedType.ContainingNamespace?.ToString().Equals("System") == true)
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.Boxing, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}