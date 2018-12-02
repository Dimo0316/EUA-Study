using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.ReplaceOldGCMethods
{
    /// A diagnostic analyzer to check "old gc methods"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReplaceOldGCMethodsAnalyzer : DiagnosticAnalyzer
    {
        /// list of old GC-causing methods or properties with new non-GC methods for alternative
        private static ImmutableArray<ReplacableMethod> ReplaceMethods = ImmutableArray.Create(
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Input",
                Name = "touches",
                Alter = "Input.GetTouch() and Input.touchCount"
            },
            new ReplacableMethod() {NameSpace = "UnityEngine.Physics", Name = "RaycastAll", Alter = "RaycastNonAlloc"},
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics",
                Name = "SphereCastAll",
                Alter = "SphereCastNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics",
                Name = "BoxCastAll",
                Alter = "BoxCastastNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics",
                Name = "CapsuleCastAll",
                Alter = "CapsuleCastNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics",
                Name = "OverlapBox",
                Alter = "OverlapBoxNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics",
                Name = "OverlapCapsule",
                Alter = "OverlapCapsuleNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics",
                Name = "OverlapSphere",
                Alter = "OverlapSphereNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "RaycastAll",
                Alter = "RaycastNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "CircleCastAll",
                Alter = "CircleCastNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "BoxCastAll",
                Alter = "BoxCastastNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "CapsuleCastAll",
                Alter = "CapsuleCastNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "LineCastAll",
                Alter = "LineCastNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "GetRayIntersectionAll",
                Alter = "GetRayIntersectionNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "OverlapAreaAll",
                Alter = "OverlapAreaNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "OverlapBoxAll",
                Alter = "OverlapBoxNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "OverlapCapsuleAll",
                Alter = "OverlapCapsuleNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "OverlapCircleAll",
                Alter = "OverlapCircleNonAlloc"
            },
            new ReplacableMethod()
            {
                NameSpace = "UnityEngine.Physics2D",
                Name = "OverlapPointAll",
                Alter = "OverlapPointNonAlloc"
            }
        );

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.ReplaceOldGCMethods); }
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
            if (context.IsExcluded(DiagnosticIDs.ReplaceOldGCMethods))
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
            if (symbolInfo.Symbol is IPropertySymbol || symbolInfo.Symbol is IMethodSymbol)
            {
                var ns = symbolInfo.Symbol.ContainingType?.ToString();
                var name = symbolInfo.Symbol.Name;
                var replaceMethod = ReplaceMethods.FirstOrDefault((pair) =>
                {
                    return pair.NameSpace.Equals(ns) && pair.Name.Equals(name);
                });
                if (!string.IsNullOrEmpty(replaceMethod.Alter))
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ReplaceOldGCMethods, syntax.GetLocation(),
                        replaceMethod.NameSpace + "." + replaceMethod.Name, replaceMethod.Alter);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}