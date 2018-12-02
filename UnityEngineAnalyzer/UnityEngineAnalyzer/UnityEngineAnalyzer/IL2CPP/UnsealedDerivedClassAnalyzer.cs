using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.IL2CPP
{
    /// Do not use overriden but not sealed methods in derived but not sealed classes.
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public class UnsealedDerivedClassAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.UnsealedDerivedClass);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            //context.RegisterSyntaxNodeActionCatchable (AnalyzeClassSyntax, SyntaxKind.ClassDeclaration);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private void AnalyzeClassSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.UnsealedDerivedClass))
            {
                return;
            }

            // if this class is inherited by other derived classes, this class should not be considered as sealed,
            // so we skip this class
            var classDeclaration = (ClassDeclarationSyntax) context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
            if (IsBaseClass(context, classSymbol))
            {
                return;
            }

            // if this is a derived class, but not sealed
            if (classDeclaration.IsDerived() && !classDeclaration.IsSealed())
            {
                // iterate all methods in this class
                var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
                foreach (var method in methods)
                {
                    // if this method is overriden but not sealed
                    if (method.IsOverriden() && !method.IsSealed())
                    {
                        // report a Diagnostic result
                        var diagnostic = Diagnostic.Create(SupportedDiagnostics.First(), method.GetLocation(),
                            method.Identifier.ToString(), classDeclaration.Identifier.ToString());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }


        /// Check if a class is base class
        private bool IsBaseClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
        {
            return false;
        }
    }
}