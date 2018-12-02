using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Transform
{
    /// A diagnostic analyzer to check "update transform too many times in one Update".
    /// rule:
    /// 1. Do not set transform properties in loop
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TransformUpdateAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.TransformUpdate); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNodeInLoop, SyntaxKind.WhileStatement,
                SyntaxKind.ForStatement,
                SyntaxKind.ForEachStatement, SyntaxKind.DoStatement);
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNodeInLoop(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcluded(DiagnosticIDs.TransformUpdate))
            {
                return;
            }

            // get all the calls which set Transform values
            var properties = context.Node.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                .Where(node => IsTransformSetProperty(context.SemanticModel, node));

            // we do some data flow analysis in order to exclude some cases
            var loopLocals = context.SemanticModel.AnalyzeDataFlow(context.Node).VariablesDeclared;

            // if this loop contains some yield statement, we see the loop as common 
            if (context.Node.DescendantNodes().OfType<YieldStatementSyntax>().Any())
            {
                var dict = new Dictionary<string, List<MemberAccessExpressionSyntax>>();
                foreach (var property in properties)
                {
                    // make a record of how many times a call is used on the same target expression
                    if (!(property?.Expression is InvocationExpressionSyntax))
                    {
                        var expression = property?.Expression?.ToString();
                        if (expression != null)
                        {
                            if (!dict.ContainsKey(expression))
                            {
                                dict[expression] = new List<MemberAccessExpressionSyntax>();
                            }

                            dict[expression].Add(property);
                        }
                    }
                }

                foreach (var v in dict.Values)
                {
                    // if such a call is used more than once and all in different branches of "if-else" statements,
                    // we report the diagnostic
                    if (v.Count > 1 && !AllUnderDifferentBranches(v, context.Node))
                    {
                        var diagnostic =
                            Diagnostic.Create(DiagnosticDescriptors.TransformUpdate, v.Last().GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
            // when this loop doesn't contain any yield statement, we check whether the transform object is declared within the loop
            else
            {
                foreach (var property in properties)
                {
                    // check whether this property call contains any local variables declared within the loop
                    if (property?.Expression?.DescendantNodesAndSelf().Any((node) =>
                    {
                        var symbol = context.SemanticModel.GetSymbolInfo(node).Symbol;
                        if (symbol != null)
                        {
                            if (loopLocals.Contains(symbol))
                            {
                                return true;
                            }
                        }

                        return false;
                    }) == false)
                    {
                        if (!property.IsExcluded(DiagnosticIDs.TransformUpdate))
                        {
                            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.TransformUpdate,
                                property.GetLocation());
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcluded(DiagnosticIDs.TransformUpdate))
            {
                return;
            }

            // get all the calls which set Transform values
            var properties = context.Node.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                .Where(node => IsTransformSetProperty(context.SemanticModel, node));

            // statistics on how many call times for each property access
            var dict = new Dictionary<string, List<MemberAccessExpressionSyntax>>();
            foreach (var property in properties)
            {
                // make a record of how many times a call is used on the same target expression
                if (!(property?.Expression is InvocationExpressionSyntax))
                {
                    var expression = property?.Expression?.ToString();
                    if (expression != null)
                    {
                        if (!dict.ContainsKey(expression))
                        {
                            dict[expression] = new List<MemberAccessExpressionSyntax>();
                        }

                        dict[expression].Add(property);
                    }
                }
            }

            foreach (var v in dict.Values)
            {
                // if such a call is used more than once and all in different branches of "if-else" statements,
                // we report the diagnostic
                if (v.Count > 1 && !AllUnderDifferentBranches(v, context.Node))
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.TransformUpdate, v.Last().GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }


        /// <summary>
        ///     Checks whether a given syntax node is a MemberAccessExpressionSyntax
        ///     which sets the value of the position or rotation of a Transform object
        /// </summary>
        static bool IsTransformSetProperty(SemanticModel sm, SyntaxNode node)
        {
            if (node is MemberAccessExpressionSyntax)
            {
                var memberAccess = node as MemberAccessExpressionSyntax;
                if (memberAccess.IsAssignmentLeftValue())
                {
                    var property = sm.GetSymbolInfo(memberAccess).Symbol as IPropertySymbol;
                    if (property?.ContainingType?.ToString().Equals("UnityEngine.Transform") == true &&
                        (property?.Name?.Equals("position") == true ||
                         property?.Name?.Equals("rotation") == true ||
                         property?.Name?.Equals("eulerAngles") == true ||
                         property?.Name?.Equals("forward") == true ||
                         property?.Name?.Equals("localEulerAngles") == true ||
                         property?.Name?.Equals("localPosition") == true ||
                         property?.Name?.Equals("localRotation") == true ||
                         property?.Name?.Equals("up") == true ||
                         property?.Name?.Equals("right") == true
                        ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        ///     Checks whether a list of nodes is all under different "if" branches(except the root method declaration node)
        /// </summary>
        static bool AllUnderDifferentBranches(List<MemberAccessExpressionSyntax> nodes, SyntaxNode root)
        {
            foreach (var node in nodes)
            {
                if (!node.IsUnderSyntaxOfKind(SyntaxKind.IfStatement))
                {
                    return false;
                }
            }

            List<SyntaxNode> blocks = new List<SyntaxNode>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                blocks.Add(nodes[i].FindClosestIfBlock());
            }

            foreach (var block in blocks)
            {
                foreach (var other in blocks)
                {
                    if (other != block && other.Ancestors().Contains(block))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}