using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Closure
{
    /// A diagnostic analyzer to check "lambda in generic methods".
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseLambdaInGenericMethod : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotUseLambdaInGenericMethod); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.SimpleLambdaExpression, SyntaxKind.AnonymousMethodExpression);
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcluded(DiagnosticIDs.DoNotUseLambdaInGenericMethod))
            {
                return;
            }

            var methodSyntax = context.Node as AnonymousFunctionExpressionSyntax;
            if (methodSyntax == null)
            {
                return;
            }

            // checks whether this anonymous function is within a generic method body
            var declare = methodSyntax.Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(node =>
                    (context.SemanticModel.GetDeclaredSymbol(node) as IMethodSymbol)?.IsGenericMethod ?? false);
            if (declare != null)
            {
                var parameters = declare.TypeParameterList?.Parameters;

                // check if there is any type parameter in this anonymous function
                // if true, this anonymous function would be skipped.
                var flag = false;
                foreach (var identifier in methodSyntax.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    var type = context.SemanticModel.GetTypeInfo(identifier).Type;
                    if (type != null)
                    {
                        foreach (var para in parameters)
                        {
                            if (type.OriginalDefinition == context.SemanticModel.GetDeclaredSymbol(para))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }

                    if (flag)
                    {
                        break;
                    }
                }

                if (!flag)
                {
                    // represents a diagnostic, such as a compiler error or a warning, along with the location where it occurred
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseLambdaInGenericMethod,
                        methodSyntax.GetLocation());

                    // report a Diagnostic result
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}