using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Containers
{
    /// A diagnostic analyzer to check "new container without size"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotInitContainerWithoutSizeAnalyzer : DiagnosticAnalyzer
    {
        /// list of all Container types
        private static ImmutableDictionary<string, List<string>> ContainerTypes = ImmutableDictionary.CreateRange(
            new Dictionary<string, List<string>>
            {
                {
                    "System.Collections.Generic", new List<string>
                    {
                        "List",
                        "Dictionary",
                        "Queue",
                        "Stack",
                        "SortedList",
                    }
                },
                {
                    "System.Collections", new List<string>
                    {
                        "ArrayList",
                        "Hashtable",
                        "Queue",
                        "Stack",
                        "SortedList",
                    }
                }
            });

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotInitContainerWithoutSize); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, SyntaxKind.ObjectCreationExpression);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotInitContainerWithoutSize))
            {
                return;
            }

            // retrieve the syntax
            var syntax = context.Node as ObjectCreationExpressionSyntax;
            if (syntax == null)
            {
                return;
            }

            // retrive the symbol
            var typeSymbol = context.SemanticModel.GetTypeInfo(syntax).Type as INamedTypeSymbol;
            if (typeSymbol == null)
            {
                return;
            }

            // check if it is a container type
            var ns = typeSymbol.ContainingNamespace?.ToString();
            if (!string.IsNullOrEmpty(ns) && ContainerTypes.ContainsKey(ns))
            {
                if (ContainerTypes[ns].Contains(typeSymbol.Name))
                {
                    // whether this constructor has an argument for capacity
                    bool hasCapacity = false;

                    // whether this constructor has an argument for an existing collection
                    bool hasEnumerable = false;

                    // check its arguments
                    var args = syntax.ArgumentList?.Arguments;
                    if (args != null && args.Value.Count > 0)
                    {
                        foreach (var arg in args)
                        {
                            // retrieve the type symbol for this argument
                            var argType = context.SemanticModel.GetTypeInfo(arg.Expression).ConvertedType;
                            if (argType == null)
                            {
                                continue;
                            }

                            // if it is an Int32, it is a capacity argument
                            if (argType.ContainingNamespace?.ToString().Equals("System") == true &&
                                argType.Name?.Equals("Int32") == true)
                            {
                                hasCapacity = true;
                                break;
                            }

                            // check if it is an IEnumerable
                            if (IsIEnumerable(argType) || argType.AllInterfaces != null &&
                                argType.AllInterfaces.Any((symbol) => { return IsIEnumerable(symbol); }))
                            {
                                hasEnumerable = true;
                                break;
                            }
                        }
                    }

                    if (!hasCapacity && !hasEnumerable)
                    {
                        var diagnostic =
                            Diagnostic.Create(DiagnosticDescriptors.DoNotInitContainerWithoutSize,
                                syntax.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }


        /// check whether a given type is IEnumerable
        private static bool IsIEnumerable(ITypeSymbol symbol)
        {
            var ns = symbol?.ContainingNamespace?.ToString();
            var name = symbol?.Name;
            if (ns == null || name == null)
            {
                return false;
            }

            return (ns.Equals("System.Collections.Generic") || ns.Equals("System.Collections")) &&
                   name.Equals("IEnumerable");
        }
    }
}