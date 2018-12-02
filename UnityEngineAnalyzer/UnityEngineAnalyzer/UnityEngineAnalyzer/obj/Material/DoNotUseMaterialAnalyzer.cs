using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Material
{
    /// A diagnostic analyzer to check "use sharedMaterial instead of material".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    class DoNotUseMaterialAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///     list of infomation of all the replacable methods
        /// </summary>
        private static ImmutableArray<ReplacableMethod> ReplaceMethods = ImmutableArray.Create(
            new ReplacableMethod() {NameSpace = "UnityEngine.Renderer", Name = "material", Alter = "sharedMaterial"},
            new ReplacableMethod() {NameSpace = "UnityEngine.Renderer", Name = "materials", Alter = "sharedMaterials"},
            new ReplacableMethod() {NameSpace = "UnityEngine.Collider", Name = "material", Alter = "sharedMaterial"},
            new ReplacableMethod() {NameSpace = "UnityEngine.Collider2D", Name = "material", Alter = "sharedMaterial"},
            new ReplacableMethod() {NameSpace = "UnityEngine.MeshFilter", Name = "mesh", Alter = "sharedMesh"}
        );

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseMaterial);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a method in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSymbol, SyntaxKind.SimpleMemberAccessExpression);
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseMaterial))
            {
                return;
            }

            // retrieve the syntax
            var syntax = context.Node as MemberAccessExpressionSyntax;
            if (syntax == null)
            {
                return;
            }

            // retrive the symbol
            SymbolInfo symbolInfo;
            if (!context.TryGetSymbolInfo(syntax, out symbolInfo))
            {
                return;
            }

            // retrieve the full name of property or method
            if (symbolInfo.Symbol is IPropertySymbol)
            {
                var ns = symbolInfo.Symbol.ContainingType?.ToString();
                var name = symbolInfo.Symbol.Name;
                var replaceMethod = ReplaceMethods.FirstOrDefault((pair) =>
                {
                    return pair.NameSpace.Equals(ns) && pair.Name.Equals(name);
                });
                if (!string.IsNullOrEmpty(replaceMethod.Alter))
                {
                    // represents a diagnostic, such as a compiler error or a warning, along with the location where it occurred
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseMaterial, syntax.GetLocation(),
                        replaceMethod.Alter, replaceMethod.Name);

                    // report a Diagnostic result
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}