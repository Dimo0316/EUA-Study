using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Containers
{
    /// A diagnostic analyzer to check "return container"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotReturnContainerAnalyzer : DiagnosticAnalyzer
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
                        "LinkedList",
                        "Queue",
                        "Stack",
                        "SortedList",
                        "SortedDictionary",
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
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotReturnContainer); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, SyntaxKind.MethodDeclaration);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotReturnContainer))
            {
                return;
            }

            // only checks a method declaration syntax
            var declare = context.Node as MethodDeclarationSyntax;
            if (declare == null)
            {
                return;
            }

            // retrieve the method returnType syntax
            var returnTypeSyntax = declare.ReturnType;
            if (returnTypeSyntax == null)
            {
                return;
            }

            // retrieve the type symbol of the return type
            var returnType = context.SemanticModel.GetTypeInfo(returnTypeSyntax).Type;
            var ns = returnType?.ContainingNamespace?.ToString() ?? string.Empty;

            // checks whether the return type is among all the container types 
            //     or is an array type
            //     or is string type 
            if (ContainerTypes.ContainsKey(ns) && ContainerTypes[ns].Contains(returnType?.Name ?? string.Empty) ||
                returnType?.Kind == SymbolKind.ArrayType ||
                returnType?.Name == "String" && ns?.ToString() == "System")
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotReturnContainer,
                    returnTypeSyntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}