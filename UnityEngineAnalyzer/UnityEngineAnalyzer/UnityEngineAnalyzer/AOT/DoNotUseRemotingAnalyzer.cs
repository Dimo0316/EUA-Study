using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.AOT
{
    /// Do not use "using System.Runtime.Remoting" in project files.
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseRemotingAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseRemoting);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a using directive in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.UsingDirective);
        }


        /// Action to be executed at completion of semantic analysis of a using directive in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseRemoting))
            {
                return;
            }

            // retrieve using syntax node
            var syntax = context.Node as UsingDirectiveSyntax;
            // and check if it is System.Runtime.Remoting
            if (syntax.Name.ToString().Equals("System.Runtime.Remoting"))
            {
                // report a Diagnostic result
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseRemoting, syntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}