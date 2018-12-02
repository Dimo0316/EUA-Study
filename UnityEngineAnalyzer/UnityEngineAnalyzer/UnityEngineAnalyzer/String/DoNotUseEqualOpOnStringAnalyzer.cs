using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.String
{
    /// A diagnostic analyzer to check "catenating strings with "==" operator".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseEqualOpOnStringAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseEqualOpOnString);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of an invocation expression in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseEqualOpOnString))
            {
                return;
            }

            // retrieve add expression
            var expression = context.Node as BinaryExpressionSyntax;
            if (expression == null)
            {
                return;
            }

            // check whether this expression is operating on string
            if (IsString(context, expression.Left) && IsLiteralEmptyString(context, expression.Right)
                || IsString(context, expression.Right) && IsLiteralEmptyString(context, expression.Left))
            {
                // report a Diagnostic result
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseEqualOpOnString,
                    expression.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }


        /// Check whether a given syntax is a string
        private static bool IsString(SyntaxNodeAnalysisContext context, ExpressionSyntax syntax)
        {
            if (syntax == null)
            {
                return false;
            }

            var type = context.SemanticModel.GetTypeInfo(syntax).ConvertedType;
            if (type?.ContainingNamespace?.ToString().Equals("System") == true && type?.Name?.Equals("String") == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// Check whether a given syntax is ""
        private static bool IsLiteralEmptyString(SyntaxNodeAnalysisContext context, ExpressionSyntax syntax)
        {
            if (syntax == null)
            {
                return false;
            }

            if (syntax is LiteralExpressionSyntax)
            {
                if ((syntax as LiteralExpressionSyntax).Token.Text == "\"\"")
                {
                    return true;
                }
            }

            return false;
        }
    }
}