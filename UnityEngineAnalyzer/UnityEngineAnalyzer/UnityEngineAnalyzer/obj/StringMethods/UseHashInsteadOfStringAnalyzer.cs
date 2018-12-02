using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.StringMethods
{
    /// A diagnostic analyzer to check "use hashcode as arguments instead of strings"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseHashInsteadOfStringAnalyzer : DiagnosticAnalyzer
    {
        /// these methods should use hashcode as arguments instead of strings
        private static Dictionary<string, List<string>> InterestedMethods = new Dictionary<string, List<string>>()
        {
            {
                "UnityEngine.Animator", new List<string>()
                {
                    "GetBool",
                    "SetBool",
                    "GetFloat",
                    "SetFloat",
                    "GetInteger",
                    "SetInteger",
                    "CrossFade",
                    "CrossFadeInFixedTime",
                    "IsParameterControlledByCurve",
                    "Play",
                    "PlayInFixedTime",
                    "SetTrigger",
                    "ResetTrigger",
                }
            },
            {
                "UnityEngine.Material", new List<string>()
                {
                    "GetColor",
                    "SetColor",
                    "GetColorArray",
                    "SetColorArray",
                    "GetFloat",
                    "SetFloat",
                    "GetFloatArray",
                    "SetFloatArray",
                    "GetInt",
                    "SetInt",
                    "GetMatrix",
                    "SetMatrix",
                    "GetMatrixArray",
                    "SetMatrixArray",
                    "GetTexture",
                    "SetTexture",
                    "GetTextureOffset",
                    "SetTextureOffset",
                    "GetTextureScale",
                    "SetTextureScale",
                    "GetVector",
                    "SetVector",
                    "GetVectorArray",
                    "SetVectorArray",
                    "HasProperty",
                    "SetBuffer",
                }
            },
            {
                "UnityEngine.Shader", new List<string>()
                {
                    "GetGlobalColor",
                    "SetGlobalColor",
                    "GetGlobalFloat",
                    "SetGlobalFloat",
                    "GetGlobalFloatArray",
                    "SetGlobalFloatArray",
                    "GetGlobalInt",
                    "SetGlobalInt",
                    "GetGlobalMatrix",
                    "SetGlobalMatrix",
                    "GetGlobalMatrixArray",
                    "SetGlobalMatrixArray",
                    "GetGlobalTexture",
                    "SetGlobalTexture",
                    "GetGlobalVector",
                    "SetGlobalVector",
                    "GetGlobalVectorArray",
                    "SetGlobalVectorArray",
                    "SetGlobalBuffer",
                }
            },
        };


        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.UseHashInsteadOfString); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.UseHashInsteadOfString))
            {
                return;
            }

            // retrieve the syntax
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return;
            }

            // retrive the symbol
            SymbolInfo symbolInfo;
            if (!context.TryGetSymbolInfo(invocation, out symbolInfo))
            {
                return;
            }

            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            // check if it is among the namespaces we are interested in
            string fullNameSpace = methodSymbol.ContainingType.ToString();
            if (InterestedMethods.Keys.Contains(fullNameSpace))
            {
                // check if it is exactly a method of a name we are interested in
                var methodNames = InterestedMethods[fullNameSpace];
                if (methodNames.Contains(methodSymbol.Name))
                {
                    // check its arguments
                    var args = invocation.ArgumentList.Arguments;
                    if (args != null && args.Count > 0)
                    {
                        // check whether its first argument is a literal string
                        if (args[0]?.Expression?.Kind() == SyntaxKind.StringLiteralExpression)
                        {
                            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.UseHashInsteadOfString,
                                args[0].Expression.GetLocation());
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}