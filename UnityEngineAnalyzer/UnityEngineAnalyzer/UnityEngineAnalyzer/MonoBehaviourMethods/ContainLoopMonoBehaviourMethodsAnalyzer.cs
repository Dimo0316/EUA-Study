using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.MonoBehaviourMethods
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ContainLoopMonoBehaviourMethodsAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ImmutableHashSet<string> MonoBehaviourMethods = ImmutableHashSet.Create(
           "Awake", "Start");
        
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
             ImmutableArray.Create(DiagnosticDescriptors.ContainLoopMonoBehaviourMethods);

        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of syntax of method declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSymbol, SyntaxKind.MethodDeclaration);
        }

        /// Action to be executed at completion of syntax of method declaration in the target project.
        /// <param name="context">context for a syntax action.</param>
        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            UEASettingReader uEASettingReader = new UEASettingReader();
            UEASettings uEASettings = uEASettingReader.ReadConfigFile();
            
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.ContainLoopMonoBehaviourMethods))
            {
                return;
            }

            // retrieve method syntax
            var methodSyntax = context.Node as MethodDeclarationSyntax;

            // retrieve method symbol
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax);
            if (methodSymbol == null)
            {
                return;
            }

            // check if method name is a MonoBehaviour method name
            if (!MonoBehaviourMethods.Contains(methodSymbol.Name))
            {
                return;
            }
            
            // from the method syntax, check if awake or start method is out off rawlimeted
            if (CheckLoopNotContainMethod(context, methodSymbol, new Dictionary<IMethodSymbol, bool>()))
            {
                return;
            }
            
            // at this point, we have a method with a MonoBehaviour method name and an empty body
            // finally, check if this method is contained in a class which extends UnityEngine.MonoBehaviour
            var containingClass = methodSymbol.ContainingType;
            var baseClass = containingClass.BaseType;
            if (baseClass?.ContainingNamespace?.Name.Equals("UnityEngine") == true &&
                baseClass?.Name?.Equals("MonoBehaviour") == true)
            {
                // represents a diagnostic, such as a compiler error or a warning, along with the location where it occurred
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ContainLoopMonoBehaviourMethods, methodSyntax.GetLocation(), containingClass.Name, methodSymbol.Name);

                // report a Diagnostic result
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool CheckLoopNotContainMethod(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol, Dictionary<IMethodSymbol, bool> searched)
        {
            // if the symbol is null, we see it as empty
            if (methodSymbol == null)
            {
                return true;
            }

            // if it is already checked, return the stored result straightly
            if (searched.ContainsKey(methodSymbol))
            {
                return searched[methodSymbol];
            }

            // we assume that there is no loop 
            searched.Add(methodSymbol, true);
            

            // if nothing but empty method invocations is found within the method, it is empty
            bool isContainLoop = false;
            foreach (var syntaxReference in methodSymbol.DeclaringSyntaxReferences)
            {
                // retrieve the syntax of each declaring syntax reference
                var methodSyntax = syntaxReference.GetSyntax() as MethodDeclarationSyntax;
                if (methodSyntax != null && methodSyntax.Body != null)
                {
                    // if its body has no statements, it is empty
                    if (methodSyntax.Body.Statements.Count == 0)
                    {
                        continue;
                    }
                    
                    var whileSyntax = methodSyntax.DescendantNodes().OfType<WhileStatementSyntax>();
                    var forSyntax = methodSyntax.DescendantNodes().OfType<ForStatementSyntax>();
                    var foreachSyntax = methodSyntax.DescendantNodes().OfType<ForEachStatementSyntax>();

                    if (whileSyntax.Count() != 0 || forSyntax.Count() != 0 || foreachSyntax.Count() != 0)
                    {
                        isContainLoop = true;
                        break;
                    }

                    // check all the invocations
                    var invocations = methodSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>();
                    foreach (var invocation in invocations)
                    {
                        // retrieve the symbol for the invocation
                        SymbolInfo symbolInfo;
                        if (!context.TryGetSymbolInfo(invocation, out symbolInfo))
                        {
                            continue;
                        }

                        var callMethodSymbol = symbolInfo.Symbol as IMethodSymbol;

                        // if a non-empty invocation is found, we mark this method non-empty
                        if (!CheckLoopNotContainMethod(context, callMethodSymbol, searched))
                        {
                            isContainLoop = true;
                            break;
                        }
                    }
                }
            }

            // if this method is marked as non-empty, we change the flag value in the dictionary
            if (isContainLoop)
            {
                searched[methodSymbol] = false;
            }

            return searched[methodSymbol];
        }
    }
}
