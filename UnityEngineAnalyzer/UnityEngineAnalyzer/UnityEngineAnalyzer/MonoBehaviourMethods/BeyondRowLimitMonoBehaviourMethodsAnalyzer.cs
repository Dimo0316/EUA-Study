using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.MonoBehaviourMethods
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BeyondRowLimitMonoBehaviourMethodsAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ImmutableHashSet<string> MonoBehaviourMethods = ImmutableHashSet.Create(
            "Awake", "Start");
        
        private static int Zero = 0;
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
             ImmutableArray.Create(DiagnosticDescriptors.BeyondRowLimitMonoBehaviourMethod);

        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of syntax of method declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSymbol, SyntaxKind.MethodDeclaration);
            // NOTE: It might be more officient to find classes and then determine if they are a MonoBehaviour rather than look at every method
        }

        /// Action to be executed at completion of syntax of method declaration in the target project.
        /// <param name="context">context for a syntax action.</param>
        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            UEASettingReader uEASettingReader = new UEASettingReader();
            UEASettings uEASettings = uEASettingReader.ReadConfigFile();

            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.EmptyMonoBehaviourMethod))
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

            int rowsCount2ThisMethod = CheckExceedRowMethod(context, methodSymbol, new Dictionary<IMethodSymbol, int>());
            // from the method syntax, check if awake or start method is out off rawlimeted
            if (rowsCount2ThisMethod <= uEASettings.startAndAwakeFunctinMAXRows)
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
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.BeyondRowLimitMonoBehaviourMethod, methodSyntax.GetLocation(), containingClass.Name, methodSymbol.Name, rowsCount2ThisMethod);

                // report a Diagnostic result
                context.ReportDiagnostic(diagnostic);
            }
        }
        //exceeds the rows-limit CheckExceedRowMethod
        /// Check whether a given method symbol is recognized as extended rows limit
        /// <param name="context">context for a symbol action </param>
        /// <param name="methodSymbol">the method symbol to be checked</param>
        /// <param name="searched">a dictionary for each method symbol which is already counted </param>
        /// <returns></returns>
        private static int CheckExceedRowMethod(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol, 
            Dictionary<IMethodSymbol, int> searched)
        {
            // if the symbol is null, we see it as empty
            if (methodSymbol == null)
            {
                return Zero;
            }

            // if it is already counted, return the stored result straightly
            if (searched.ContainsKey(methodSymbol))
            {
                return searched[methodSymbol];
            }

            // we assume that this method is empty by default
            searched.Add(methodSymbol, Zero);

            // if nothing but empty method invocations is found within the method, it is empty
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

                    searched[methodSymbol] += methodSyntax.Body.Statements.Count;

                    // count all the invocations
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
                        
                        // add the callMethodSymbol rows count to this methodSymbol
                        searched[methodSymbol] += CheckExceedRowMethod(context, callMethodSymbol, searched);
                    }
                }
            }
            
            return searched[methodSymbol];
        }
    }
}
