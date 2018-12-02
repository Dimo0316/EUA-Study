using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.CompareTag
{
    /// A diagnostic analyzer to check：
    /// a. tag.Equals("") or "".Equals(tag);
    /// b. tag == "", tag != "", "" == tag, "" != tag；
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class UseCompareTagAnalyzer : DiagnosticAnalyzer
    {
        /// only "tag" components contained in these symbols are considered as "warning"
        private static readonly ImmutableHashSet<string> ContainingSymbols =
            ImmutableHashSet.Create("UnityEngine.Component", "UnityEngine.GameObject");


        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.UseCompareTag);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of an invocation expression
            // (looking for tag.Equals("") or "".Equals(tag))
            context.RegisterSyntaxNodeActionCatchable(AnalyzeInvocationExpressionNode, SyntaxKind.InvocationExpression);

            // register an action to be executed at completion of semantic analysis of a == expression or a != expression
            // (looking for tag == "", tag != "", "" == tag, "" != tag)
            context.RegisterSyntaxNodeActionCatchable(AnalyzeBinaryExpressionNode, SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);
        }


        /// Looking for an InvocationExpressionSyntax of the form tag.Equals("") or "".Equals(tag).
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeInvocationExpressionNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.UseCompareTag))
            {
                return;
            }

            // a InvocationExpressionSyntax node, represents the syntax node for invocation expression
            // Expression, representing the expression part of the invocation
            var invocationExpression = context.Node as InvocationExpressionSyntax;

            // check that number of arguments is one
            if (1 != invocationExpression.ArgumentList?.Arguments.Count)
            {
                return;
            }

            // retrieve the MemberAccessExpression and check that is it "Equals", check that number of arguments is one
            // (MemberAccessExpressionSyntax, representing the syntax node for member access expression, for example, gameObject.tag;
            //  Name, 
            var equalsMemberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
            if (false == (equalsMemberAccessExpression?.Name.Identifier.Text.Equals("Equals") ?? false))
            {
                return;
            }

            // at this point we have an .Equals member access with one argument, check on both sides if there is a tag member access
            if (true == ProcessPotentialTagMemberAccessExpression(context,
                    equalsMemberAccessExpression.Expression) || // "".Equals(tag)
                true == ProcessPotentialTagMemberAccessExpression(context,
                    invocationExpression.ArgumentList.Arguments[0].Expression)) // tag.Equals("")
            {
                // report a Diagnostic result
                ReportDiagnostic(context, invocationExpression.GetLocation());
            }
        }


        /// Looking for tag == "", tag != "", "" == tag, "" != tag.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeBinaryExpressionNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.UseCompareTag))
            {
                return;
            }

            // a BinaryExpressionSyntax node, represents an expression that has a binary operator
            // left , representing the expression on the left of the binary operator.
            // right, representing the expression on the right of the binary operator.
            // OperatorToken, representing the operator of the binary expression.
            var binaryExpression = context.Node as BinaryExpressionSyntax;
            if (true == ProcessPotentialTagMemberAccessExpression(context, binaryExpression.Left) ||
                true == ProcessPotentialTagMemberAccessExpression(context, binaryExpression.Right))
            {
                // report a Diagnostic result
                ReportDiagnostic(context, binaryExpression.GetLocation());
            }
        }


        /// Check whether the given expression is "UnityEngine.Component.tag", or "UnityEngine.GameObject.tag".
        /// <param name="context">context for a symbol action.</param>
        /// <param name="expression">the expression node to check.</param>
        /// <returns>true, if the expression is a "UnityEngine.Component.tag", or "UnityEngine.GameObject.tag".</returns>
        private static bool ProcessPotentialTagMemberAccessExpression(SyntaxNodeAnalysisContext context,
            ExpressionSyntax expression)
        {
            // MemberAccessExpressionSyntax, representing the syntax node for member access expression, for example, gameObject.tag
            // IdentifierNameSyntax, representing the syntax node for identifier name, for example, "tag" named variable in a Component class

            // check for member access or identifier access
            if (expression is MemberAccessExpressionSyntax || expression is IdentifierNameSyntax)
            {
                // get property symbol for the given expression
                var propertySymbol = context.SemanticModel.GetSymbolInfo(expression).Symbol as IPropertySymbol;

                // check the property accessed is:
                // a. a tag property
                // b. the immediately containing symbol for the tag property is either "UnityEngine.Component" or "UnityEngine.GameObject"
                if (propertySymbol?.Name.Equals("tag") ??
                    false && //TODO: Either fix this statement or remove this check
                    ContainingSymbols.Contains(propertySymbol.ContainingSymbol.ToString()))
                {
                    return true;
                }
            }

            return false;
        }


        /// Report a Diagnostic result.
        /// <param name="context">context for a symbol action.</param>
        /// <param name="location">file location of the diagnostic.</param>
        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, Location location)
        {
            // represents a diagnostic, such as a compiler error or a warning, along with the location where it occurred
            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.UseCompareTag, location);

            // report a Diagnostic result
            context.ReportDiagnostic(diagnostic);
        }
    }
}