using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.UnityThread
{
    /// A diagnostic analyzer to check "GameObject in destructor".
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class GameObjectInDestructorAnalzyer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.GameObjectInDestructor); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.DestructorDeclaration);
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            //if (context.IsExcluded(DiagnosticIDs.GameObjectInDestructor)) { return; }

            //var diagnostic = Diagnostic.Create(DiagnosticDescriptors.GameObjectInDestructor, context.Node.GetLocation());
            //context.ReportDiagnostic(diagnostic);
        }
    }
}