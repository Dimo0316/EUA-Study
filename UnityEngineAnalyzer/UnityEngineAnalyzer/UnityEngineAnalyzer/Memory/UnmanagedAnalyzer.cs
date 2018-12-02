using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Memory
{
    /// A diagnostic analyzer to check "unmanaged resources".
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnmanagedAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.Unmanaged);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a method in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.ObjectCreationExpression);
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.Unmanaged))
            {
                return;
            }

            // retrieve the object creation syntax
            var objectCreationSyntax = context.Node as ObjectCreationExpressionSyntax;
            if (objectCreationSyntax == null)
            {
                return;
            }

            // retrieve the type symbol
            var type = context.SemanticModel.GetTypeInfo(objectCreationSyntax).Type;
            if (type == null)
            {
                return;
            }

            // check whether this type implements "System.IDisposable"
            if (type.AllInterfaces.Any(i =>
                true == i?.Name?.Equals("IDisposable") && true == i?.ContainingNamespace?.ToString().Equals("System")))
            {
                // if it is under a "using" statement, it is OK
                if (objectCreationSyntax.IsUnderSyntaxOfKind(SyntaxKind.UsingStatement))
                {
                    return;
                }

                // we assume it as a bad call first, and check for "Dispose" calls on it
                var flag = false;

                // it must appear in a method declaration body
                var methodDeclare = objectCreationSyntax.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (methodDeclare != null)
                {
                    // it must appear in an assignment expression or a variable declaration, so that it can be disposed later
                    var assignment = objectCreationSyntax.Ancestors().OfType<AssignmentExpressionSyntax>()
                        .FirstOrDefault();
                    var varDeclare = objectCreationSyntax.Ancestors().OfType<VariableDeclaratorSyntax>()
                        .FirstOrDefault();

                    // retrieve the symbol of the target identifier
                    ISymbol target = null;
                    if (assignment != null)
                    {
                        target = context.SemanticModel.GetSymbolInfo(assignment.Left).Symbol;
                    }
                    else if (varDeclare != null)
                    {
                        target = context.SemanticModel.GetDeclaredSymbol(varDeclare);
                    }

                    if (target != null)
                    {
                        // checks whether there is any invocation to "Dispose" on the same target in the method declaration body
                        if (objectCreationSyntax.Root().DescendantNodes().OfType<InvocationExpressionSyntax>().Any(
                            invocation =>
                                invocation.MethodName().Equals("Dispose") &&
                                invocation.Expression is MemberAccessExpressionSyntax &&
                                target == context.SemanticModel
                                    .GetSymbolInfo((invocation.Expression as MemberAccessExpressionSyntax).Expression)
                                    .Symbol))
                        {
                            flag = true;
                        }
                    }
                }

                if (!flag)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.Unmanaged,
                        objectCreationSyntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}