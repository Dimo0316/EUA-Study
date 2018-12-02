using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Particles
{
    /// A diagnostic analyzer to check "withChildren option of ParticleSystem API".
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParticleApiWithChildrenAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///     list of all methods of ParticleSystem which contains a "withChildren" option
        /// </summary>
        private static ImmutableArray<string> ParticleMethods = ImmutableArray.Create(
            "Clear",
            "IsAlive",
            "Pause",
            "Play",
            "Simulate",
            "Stop"
        );

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.ParticleApiWithChildren);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a method in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.ParticleApiWithChildren))
            {
                return;
            }

            // retrieve the methos Syntax
            var invocation = context.Node as InvocationExpressionSyntax;

            // retrieve the method symbol
            var method = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            // make sure this method is a ParticleSystem method
            if (true != method?.ContainingType?.ToString().Equals("UnityEngine.ParticleSystem"))
            {
                return;
            }

            // make sure this method is among the methods we are interested in
            if (!ParticleMethods.Contains(method?.Name))
            {
                return;
            }

            // If this method passes all the checkings above, it must has a parameter as "withChildren" set to "false",
            // so the following steps check the parameters of this method.
            // We reserve a flag indicating whether its parameters pass the checkings.
            bool flag = false;

            // it must contain some parameters
            if (method?.Parameters != null)
            {
                // the index of the "withChildren" parameter
                int index = -1;

                // check all the parameters of this method to find the "withChildren" parameter and its index
                int cnt = 0;
                foreach (var parameter in method.Parameters)
                {
                    if (true == parameter?.Name?.Equals("withChildren"))
                    {
                        index = cnt;
                        break;
                    }

                    cnt++;
                }

                // there should be a parameter named "withChildren"
                if (index >= 0)
                {
                    // retrieve the arguments syntax of this method
                    var arguments = (invocation?.ArgumentList?.Arguments).Value;

                    for (int i = 0; i < arguments.Count; i++)
                    {
                        var arg = arguments[i];

                        // retrieve the "withChildren" argument by the index found above
                        if (true == arg?.NameColon?.Name?.Identifier.Text?.Equals("withChildren") ||
                            (arg?.NameColon == null && i == index))
                        {
                            // if this argument is "false", this is a good invocation
                            if (true == arg?.Expression?.ToString().Equals("false"))
                            {
                                flag = true;
                            }

                            break;
                        }
                    }
                }
            }

            // if the flag is not true, this invocation doesn't pass the parameter checking
            if (!flag)
            {
                // report a diagnostic
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ParticleApiWithChildren,
                    invocation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}