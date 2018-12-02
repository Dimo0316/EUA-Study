using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Textures
{
    /// A diagnostic analyzer to check "nonReadable option in Texture API"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TextureNonReadableAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///     list of all Texture APIs which contain a parameter as "nonReadable"
        /// </summary>
        private static ImmutableArray<TextureNonReadableAPIItem> TextureAPIs = ImmutableArray.Create(
            new TextureNonReadableAPIItem()
            {
                ClassName = "Texture2D",
                MethodName = "Apply",
                ParameterName = "makeNoLongerReadable"
            },
            new TextureNonReadableAPIItem()
            {
                ClassName = "Texture2D",
                MethodName = "PackTextures",
                ParameterName = "makeNoLongerReadable"
            },
            new TextureNonReadableAPIItem()
            {
                ClassName = "Texture2DArray",
                MethodName = "Apply",
                ParameterName = "makeNoLongerReadable"
            },
            new TextureNonReadableAPIItem()
            {
                ClassName = "Texture3D",
                MethodName = "Apply",
                ParameterName = "makeNoLongerReadable"
            },
            new TextureNonReadableAPIItem()
            {
                ClassName = "Cubemap",
                MethodName = "Apply",
                ParameterName = "makeNoLongerReadable"
            },
            new TextureNonReadableAPIItem()
            {
                ClassName = "CubemapArray",
                MethodName = "Apply",
                ParameterName = "makeNoLongerReadable"
            },
            new TextureNonReadableAPIItem()
            {
                ClassName = "ImageConversion",
                MethodName = "LoadImage",
                ParameterName = "markNonReadable"
            }
        );

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.TextureNonReadable); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.TextureNonReadable))
            {
                return;
            }

            // retrieve the invocation syntax
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return;
            }

            // retrieve the method symbol
            var method = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (method == null)
            {
                return;
            }

            // retrive the containing type symbol of this method
            var containingType = method.ContainingType;
            if (containingType == null)
            {
                return;
            }

            // the method should under "UnityEngine" namespace
            if (containingType.ContainingNamespace?.ToString().StartsWith("UnityEngine") == true)
            {
                // if a matching API is found
                var api = TextureAPIs.FirstOrDefault(item =>
                    item.ClassName.Equals(containingType.Name) && item.MethodName.Equals(method.Name));
                if (!string.IsNullOrEmpty(api.ParameterName))
                {
                    // we reserve a flag here indicating whether this invocation passes the checking
                    var flag = false;

                    // first check whether the method symbol contains the required parameter
                    var index = -1;
                    if (method.Parameters != null)
                    {
                        for (int i = 0; i < method.Parameters.Length; i++)
                        {
                            if (method.Parameters[i]?.Name?.Equals(api.ParameterName) == true)
                            {
                                index = i;
                                break;
                            }
                        }
                    }

                    // second check whether the required parameter is set to "true"
                    if (index >= 0 && invocation.ArgumentList != null)
                    {
                        for (int i = 0; i < invocation.ArgumentList.Arguments.Count; i++)
                        {
                            // find the corresponding argument
                            var arg = invocation.ArgumentList.Arguments[i];
                            if (true == arg?.NameColon?.Name?.Identifier.Text?.Equals(api.ParameterName) ||
                                (arg?.NameColon == null && i == index))
                            {
                                // if this argument is "true", this is a good invocation
                                if (true == arg?.Expression?.ToString().Equals("true"))
                                {
                                    flag = true;
                                }

                                break;
                            }
                        }
                    }

                    if (!flag)
                    {
                        // report a diagnostic
                        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.TextureNonReadable,
                            invocation.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }


        /// <summary>
        ///     struct describing a method which contains a parameter as "nonReadable"
        /// </summary>
        private struct TextureNonReadableAPIItem
        {
            public string ClassName;
            public string MethodName;
            public string ParameterName;
        }
    }
}