using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.StringMethods
{
    /// A diagnostic analyzer to check "whether any Invoke or InvokeRepeating methods are called".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public class InvokeFunctionMissingAnalyzer : DiagnosticAnalyzer
    {
        /// the methods we are looking for.
        private static readonly ImmutableHashSet<string> InvokeMethods =
            ImmutableHashSet.Create("Invoke", "InvokeRepeating");


        /// only methods invoked from this class are ones we are looking for.
        private static readonly string InvokeMethodTypeName = "UnityEngine.MonoBehaviour";


        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.InvokeFunctionMissing);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of an invocation expression in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.InvokeFunctionMissing))
            {
                return;
            }

            // early out
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return;
            }

            // if this invocation expression is the method we are looking for
            var methodName = invocation.MethodName();
            if (InvokeMethods.Contains(methodName))
            {
                // check if the method is the one from UnityEngine
                var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
                var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

                var fullTypeName = methodSymbol?.ContainingType.ToString();
                if (fullTypeName == InvokeMethodTypeName && invocation.ArgumentList.Arguments.Count > 0)
                {
                    // invoked method name
                    var firstArgumentExpression = invocation.ArgumentList.Arguments[0];
                    if (firstArgumentExpression.Kind() != SyntaxKind.StringLiteralExpression)
                    {
                        //only value of a literal string is predicable 
                        return;
                    }

                    var invokedMethodName = firstArgumentExpression.GetArgumentValue<string>();

                    // all user-defined methods defined in the invoked method's containing class
                    var containingClassDeclaration =
                        invocation.Ancestors().FirstOrDefault(a => a is ClassDeclarationSyntax) as
                            ClassDeclarationSyntax;
                    var allMethods = containingClassDeclaration?.Members.OfType<MethodDeclarationSyntax>();

                    // make sure the invoked method is not a user-defined method
                    var invokeEndPoint = allMethods.FirstOrDefault(m => m.Identifier.ValueText == invokedMethodName);
                    if (invokeEndPoint == null)
                    {
                        // report a Diagnostic result
                        var diagnostic = Diagnostic.Create(SupportedDiagnostics.First(),
                            firstArgumentExpression.GetLocation(),
                            methodName, invokedMethodName);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}