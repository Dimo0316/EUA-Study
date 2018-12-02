using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Containers
{
    /// A diagnostic analyzer to check "use enum or struct as key to dictionary"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseEnumOrStructAsKeyAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotUseEnumOrStructAsKey); }
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
            if (context.IsExcluded(DiagnosticIDs.DoNotUseEnumOrStructAsKey))
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

            // check if it is a dictionary
            if (typeSymbol.ContainingNamespace?.ToString().Equals("System.Collections.Generic") == true &&
                typeSymbol.Name?.Equals("Dictionary") == true)
            {
                // retrieve the arguments for generic type
                var typeArgs = typeSymbol.TypeArguments;
                if (typeArgs == null || typeArgs.Length != 2)
                {
                    return;
                }

                // the first arg is key
                var key = typeArgs[0];
                if (key == null)
                {
                    return;
                }

                // check if the key is enum or struct (struct not implementing IEquatable)
                if (key.TypeKind == TypeKind.Enum || key.TypeKind == TypeKind.Struct && !IsIEquatable(key))
                {
                    // check arguments of the constructor
                    var args = syntax.ArgumentList?.Arguments;
                    if (args == null || args.Value.Count == 0)
                    {
                        return;
                    }

                    // check if there is a custom comparer among the argument list
                    bool hasComparer = false;
                    foreach (var arg in args)
                    {
                        // retrieve the symbol for the arg
                        var argType = context.SemanticModel.GetTypeInfo(arg.Expression).ConvertedType;

                        // check if this arg implements IEqualityComparer
                        if (argType?.Name?.Equals("IEqualityComparer") == true &&
                            argType?.ContainingNamespace?.ToString().Equals("System.Collections.Generic") == true)
                        {
                            hasComparer = true;
                            break;
                        }
                    }

                    // if no comparer found, it is a bad dictionary
                    if (!hasComparer)
                    {
                        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseEnumOrStructAsKey,
                            syntax.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }


        /// Check whether a type implements the interface "IEquatable"
        private static bool IsIEquatable(ITypeSymbol symbol)
        {
            if (symbol == null)
            {
                return false;
            }

            var interfaces = symbol.AllInterfaces;
            if (interfaces == null || interfaces.Length == 0)
            {
                return false;
            }

            return interfaces.Any((itf) =>
            {
                return itf.Name?.Equals("IEquatable") == true &&
                       itf.ContainingNamespace?.ToString().Equals("System") == true;
            });
        }
    }
}