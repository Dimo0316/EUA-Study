using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.FindMethodsInUpdate
{
    /// A diagnostic analyzer to check:
    /// a. do not call "Find" methods in any "Update" method body directly.
    /// b. do not call "Find" methods in any first-level child methods in any "Update" method body.
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseFindMethodsInUpdateAnalyzer : DiagnosticAnalyzer
    {
        /// all "Find" calls we are looking for.
        private static readonly ImmutableHashSet<string> FindMethodNames = ImmutableHashSet.Create(
            "Find",
            "FindGameObjectsWithTag",
            "FindGameObjectWithTag",
            "FindWithTag",
            "FindObjectOfType",
            "FindObjectsOfType",
            "FindObjectsOfTypeAll",
            "FindObjectsOfTypeIncludingAssets",
            "FindSceneObjectsOfType",
            "GetComponent",
            "GetComponentInChildren",
            "GetComponentInParent",
            "GetComponents",
            "GetComponentsInChildren",
            "GetComponentsInParent",
            "FindChild");


        /// only "Find" calls after these symbols are the ones we are looking for.
        private static readonly ImmutableHashSet<string> ContainingSymbols = ImmutableHashSet.Create(
            "UnityEngine.GameObject",
            "UnityEngine.Object",
            "UnityEngine.Component",
            "UnityEngine.Transform");


        /// all "Find" methods called in user-defined methods, which are called in root-level "Update" methods.
        /// <the "Find" method, the user-defined method that calls this "Find" method>.
        private Dictionary<ExpressionSyntax, ExpressionSyntax> _indirectCallers;


        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    /// do not call "Find" methods in any "Update" method body directly
                    DiagnosticDescriptors.DoNotUseFindMethodsInUpdate,
                    /// do not call "Find" methods in any first-level child methods in any "Update" method body
                    DiagnosticDescriptors.DoNotUseFindMethodsInUpdateRecursive);
            }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeClassSyntax, SyntaxKind.ClassDeclaration);
        }


        /// Action to be executed at completion of semantic analysis of a class description in the target project.
        /// <param name="context">context for a symbol action.</param>
        public void AnalyzeClassSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseFindMethodsInUpdate))
            {
                return;
            }

            var monoBehaviourInfo = new MonoBehaviourInfo(context);
            var searched = new Dictionary<IMethodSymbol, bool>();
            _indirectCallers = new Dictionary<ExpressionSyntax, ExpressionSyntax>();

            // iterate all "Update" functions in this class, if this class is a MonoBehavior-derived one
            monoBehaviourInfo.ForEachUpdateMethod((updateMethod) =>
            {
                // Debug.WriteLine("Found an update call! " + updateMethod);

                // iterate all "Find" functions in this "Update" function
                var findCalls = SearchForFindCalls(context, updateMethod, searched, true);
                foreach (var findCall in findCalls)
                {
                    // if this call is excluded, skip it
                    if (findCall.IsExcluded(DiagnosticIDs.DoNotUseFindMethodsInUpdate))
                    {
                        continue;
                    }

                    // if this is a "Find" method to be called directly in this method body
                    if (!_indirectCallers.ContainsKey(findCall))
                    {
                        // report a Diagnostic result
                        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseFindMethodsInUpdate,
                            findCall.GetLocation(), findCall, monoBehaviourInfo.ClassName, updateMethod.Identifier);
                        context.ReportDiagnostic(diagnostic);
                    }
                    // if this is a "Find" method to be called within a user-defined method, which is called in this method body
                    // (here, we only care about "Find" methods in the first-level child methods, with all deeper child methods being ignored)
                    else
                    {
                        var endPoint = _indirectCallers[findCall];

                        // report a Diagnostic result
                        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseFindMethodsInUpdateRecursive,
                            findCall.GetLocation(), monoBehaviourInfo.ClassName, updateMethod.Identifier, findCall,
                            endPoint);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            });
        }


        /// Search for all "Find" methods within the given method body.
        /// <param name="context">context for a symbol action.</param>
        /// <param name="method">within which method body to search for "Find" methods.</param>
        /// <param name="searched">
        ///     methods that we have already checked,
        ///     <method symbol, if this method is the "Find" method we are looking for>.
        /// </param>
        /// <param name="isRoot">whether the given method is the root method.</param>
        /// <returns>
        ///     all invocation methods within the given method and all its child methods (methods to be called within the parent
        ///     method body),
        ///     that are considered as "Find" methods we are looking for.
        /// </returns>
        private IEnumerable<ExpressionSyntax> SearchForFindCalls(SyntaxNodeAnalysisContext context,
            MethodDeclarationSyntax method,
            IDictionary<IMethodSymbol, bool> searched, bool isRoot)
        {
            // find all invocation expressions in the given method body
            var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();

            // iterate with all invocation expressions
            foreach (var invocation in invocations)
            {
                // try to get the simbol info for the invoking expression
                SymbolInfo symbolInfo;
                if (!context.TryGetSymbolInfo(invocation, out symbolInfo))
                {
                    continue;
                }

                // if the invoking expression is a method
                var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
                if (methodSymbol != null)
                {
                    // if this method was already considered as "Find" method, return it
                    if (searched.ContainsKey(methodSymbol))
                    {
                        if (searched[methodSymbol])
                        {
                            yield return invocation;
                        }
                    }
                    else
                    {
                        // if this is a method we are looking for (Find method), return it
                        if (FindMethodNames.Contains(methodSymbol.Name) &&
                            ContainingSymbols.Contains(methodSymbol.ContainingSymbol.ToString()))
                        {
                            searched.Add(methodSymbol, true);
                            yield return invocation;
                        }
                        // this method is not the "Find" method we are looking for, but it might be a user-defined function, 
                        // and it might contain child-methods which have "Find" methods inside
                        else
                        {
                            var methodDeclarations = methodSymbol.DeclaringSyntaxReferences;

                            // let's assume this method does not contain "Find" calls within its body
                            searched.Add(methodSymbol, false);

                            // check all invocation expressions within this method
                            foreach (var methodDeclaration in methodDeclarations)
                            {
                                var theMethodSyntax = methodDeclaration.GetSyntax() as MethodDeclarationSyntax;
                                if (theMethodSyntax != null)
                                {
                                    // check whether this child method has any "Find" invocation inside
                                    var childFindCallers =
                                        SearchForFindCalls(context, theMethodSyntax, searched, false);
                                    if (childFindCallers != null && childFindCallers.Any())
                                    {
                                        // update the searched directionary with new info
                                        // (although the parent method does not contain any "Find" method we are looking for, 
                                        //  one of its child method has at least one "Find" method inside)
                                        searched[methodSymbol] = true;

                                        // cache all first-level method calls that have "Find" calls we are looking for
                                        // (here, we only care about "Find" methods in the first-level child methods, with all deeper child methods being ignored)
                                        if (isRoot)
                                        {
                                            _indirectCallers.Add(invocation, childFindCallers.First());
                                        }

                                        // this is the method we are interested in
                                        yield return invocation;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}