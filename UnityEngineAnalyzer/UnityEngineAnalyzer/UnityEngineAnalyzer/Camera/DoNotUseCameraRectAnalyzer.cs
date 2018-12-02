using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Camera
{
    /// A diagnostic analyzer to check "Camera.rect"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseCameraRectAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotUseCameraRect); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, SyntaxKind.SimpleMemberAccessExpression);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseCameraRect))
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

            var propertySymbol = symbolInfo.Symbol as IPropertySymbol;
            if (propertySymbol == null)
            {
                return;
            }

            // check if it equals to "UnityEngine.Camera.rect"
            if (propertySymbol.Name.Equals("rect") && propertySymbol.ContainingType.Name.Equals("Camera") &&
                propertySymbol.ContainingNamespace.Name.Equals("UnityEngine"))
            {
                // check if this property is used in an assignment leftvalue (whether it is a set method)
                if ((syntax.Parent is AssignmentExpressionSyntax) &&
                    syntax == (syntax.Parent as AssignmentExpressionSyntax).Left)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseCameraRect, syntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}