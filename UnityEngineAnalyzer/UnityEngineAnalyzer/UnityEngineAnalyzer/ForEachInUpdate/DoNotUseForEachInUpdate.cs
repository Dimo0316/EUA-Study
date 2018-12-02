using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.ForEachInUpdate
{
    /// A diagnostic analyzer to check "whether any "ForEach" method in "Update" functions".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseForEachInUpdate : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseForEachInUpdate);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeClassSyntax, SyntaxKind.ClassDeclaration);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        public static void AnalyzeClassSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseForEachInUpdate))
            {
                return;
            }

            var monoBehaviourInfo = new MonoBehaviourInfo(context);
            var searched = new Dictionary<IMethodSymbol, bool>();

            /// iterate all Update methods in this MonoBehavior-derived class
            monoBehaviourInfo.ForEachUpdateMethod((updateMethod) =>
            {
                // iterate all "ForEach" methods in this "Update" method
                var forEachStatements = SearchForForEach(context, updateMethod, searched);
                foreach (var forEachStatement in forEachStatements)
                {
                    if (forEachStatement.IsExcluded(DiagnosticIDs.DoNotUseForEachInUpdate))
                    {
                        continue;
                    }

                    Debug.WriteLine("Found a bad call! " + forEachStatement);

                    // report a Diagnostic result
                    var location = forEachStatement.ForEachKeyword.GetLocation();
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseForEachInUpdate, location,
                        monoBehaviourInfo.ClassName, updateMethod.Identifier);
                    context.ReportDiagnostic(diagnostic);
                }
            });
        }


        /// Find all "ForEach" methods in a given method body.
        /// <param name="context">context for a symbol action.</param>
        /// <param name="method">the method body to test</param>
        /// <param name="searched"></param>
        /// <returns>all "ForEach" methods in the given method body.</returns>
        private static IEnumerable<ForEachStatementSyntax> SearchForForEach(SyntaxNodeAnalysisContext context,
            MethodDeclarationSyntax method, IDictionary<IMethodSymbol, bool> searched)
        {
            var invocations = method.DescendantNodes().OfType<ForEachStatementSyntax>();
            foreach (var invocation in invocations)
            {
                yield return invocation;
            }

            //TODO: Keep Searching recurively to other methods...
        }
    }
}