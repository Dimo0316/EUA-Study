using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Memory
{
    /// A diagnostic analyzer to check "unity flag DontUnloadUnusedAsset".
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnityUnusedFlagAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.UnityUnusedFlag);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a method in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.SimpleAssignmentExpression);
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.UnityUnusedFlag))
            {
                return;
            }

            // retrieve the assignment syntax
            var assignment = context.Node as AssignmentExpressionSyntax;
            if (assignment == null)
            {
                return;
            }

            // retrieve the assign type symbol
            var assignmentType = context.SemanticModel.GetTypeInfo(assignment).Type;
            if (assignmentType == null)
            {
                return;
            }

            // check whether the type of this assignment is "HideFlags"
            if (!(assignmentType.Name?.Equals("HideFlags") == true &&
                  assignmentType.ContainingNamespace?.ToString().Equals("UnityEngine") == true))
            {
                return;
            }

            // check whether the left symbol is "UnityEngine.Object.hideFlags"
            var leftSymbol = context.SemanticModel.GetSymbolInfo(assignment.Left).Symbol;
            if (!(leftSymbol?.Name?.Equals("hideFlags") == true &&
                  leftSymbol?.ContainingType?.ToString().Equals("UnityEngine.Object") == true))
            {
                return;
            }

            // check whether the right value contains "HideFlags.DontUnloadUnusedAsset"
            var rightValue = context.SemanticModel.GetConstantValue(assignment.Right);
            if (!rightValue.HasValue)
            {
                return;
            }

            // "HideFlags.DontUnloadUnusedAsset" equals to 32
            if (((int) rightValue.Value & 32) == 0)
            {
                return;
            }

            // record the target object
            ISymbol target = null;
            if (assignment.Left is MemberAccessExpressionSyntax)
            {
                var targetSyntax = (assignment.Left as MemberAccessExpressionSyntax).Expression;
                target = context.SemanticModel.GetSymbolInfoSafe(targetSyntax).Symbol;
            }
            else
            {
                var targetSyntax = assignment.Ancestors().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                target = context.SemanticModel.GetDeclaredSymbolSafe(targetSyntax);
            }

            if (target == null)
            {
                return;
            }

            // check whethr the recorded symbol is "Destroyed" somewhere
            if (assignment.Root().DescendantNodes().OfType<InvocationExpressionSyntax>().Any(invocation =>
            {
                var method = context.SemanticModel.GetSymbolInfo(invocation).Symbol;
                if (method == null || method.Name == null || invocation.ArgumentList == null)
                {
                    return false;
                }

                if (method.Name.StartsWith("Destroy") &&
                    method.ContainingNamespace?.ToString().Equals("UnityEngine") == true)
                {
                    var arg = invocation.ArgumentList.Arguments[0];
                    if (target == context.SemanticModel.GetSymbolInfo(arg.Expression).Symbol)
                    {
                        return true;
                    }
                }

                return false;
            }))
            {
                return;
            }

            // report a diagnostic
            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.UnityUnusedFlag, assignment.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}