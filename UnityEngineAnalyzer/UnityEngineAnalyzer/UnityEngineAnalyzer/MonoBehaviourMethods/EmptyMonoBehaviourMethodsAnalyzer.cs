using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.MonoBehaviourMethods
{
    /// A diagnostic analyzer to check "whether any empty MonoBehavior methods existed".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class EmptyMonoBehaviourMethodsAnalyzer : DiagnosticAnalyzer
    {
        /// all MonoBehavior functions to check.
        private static readonly ImmutableHashSet<string> MonoBehaviourMethods = ImmutableHashSet.Create(
            "Awake",
            "FixedUpdate",
            "LateUpdate",
            "OnAnimatorIK",
            "OnAnimatorMove",
            "OnApplicationFocus",
            "OnApplicationPause",
            "OnApplicationQuit",
            "OnAudioFilterRead",
            "OnBecameInvisible",
            "OnBecameVisible",
            "OnCollisionEnter",
            "OnCollisionEnter2D",
            "OnCollisionExit",
            "OnCollisionExit2D",
            "OnCollisionStay",
            "OnCollisionStay2D",
            "OnConnectedToServer",
            "OnControllerColliderHit",
            "OnDestroy",
            "OnDisable",
            "OnDisconnectedFromServer",
            "OnDrawGizmos",
            "OnDrawGizmosSelected",
            "OnEnable",
            "OnFailedToConnect",
            "OnFailedToConnectToMasterServer",
            "OnGUI",
            "OnJointBreak",
            "OnLevelWasLoaded",
            "OnMasterServerEvent",
            "OnMouseDown",
            "OnMouseDrag",
            "OnMouseEnter",
            "OnMouseExit",
            "OnMouseOver",
            "OnMouseUp",
            "OnMouseUpAsButton",
            "OnNetworkInstantiate",
            "OnParticleCollision",
            "OnPlayerConnected",
            "OnPlayerDisconnected",
            "OnPostRender",
            "OnPreCull",
            "OnPreRender",
            "OnRenderImage",
            "OnRenderObject",
            "OnSerializeNetworkView",
            "OnServerInitialized",
            "OnTransformChildrenChanged",
            "OnTransformParentChanged",
            "OnTriggerEnter",
            "OnTriggerEnter2D",
            "OnTriggerExit",
            "OnTriggerExit2D",
            "OnTriggerStay",
            "OnTriggerStay2D",
            "OnValidate",
            "OnWillRenderObject",
            "Reset",
            "Start",
            "Update");

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.EmptyMonoBehaviourMethod);


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
            

            // from the method syntax, check if there is a body and if there are statements in it
            if (!CheckEmptyMethod(context, methodSymbol, new Dictionary<IMethodSymbol, bool>()))
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
                //var diagnostic = Diagnostic.Create(DiagnosticDescriptors.EmptyMonoBehaviourMethod, methodSyntax.GetLocation(), containingClass.Name, methodSymbol.Name);

                // report a Diagnostic result
                //context.ReportDiagnostic(diagnostic);
            }
        }


        /// Check whether a given method symbol is recognized as empty
        /// <param name="context">context for a symbol action </param>
        /// <param name="methodSymbol">the method symbol to be checked</param>
        /// <param name="searched">a dictionary for each method symbol which is already checked </param>
        /// <returns></returns>
        private static bool CheckEmptyMethod(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol,
            IDictionary<IMethodSymbol, bool> searched)
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

            // we assume that this method is empty by default
            searched.Add(methodSymbol, true);

            // if nothing but empty method invocations is found within the method, it is empty
            bool isNonEmpty = false;
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

                    // an empty invocation should contain no more than InvocationExpressionSyntax nodes and TypeSyntax nodes of ExpressionSyntax nodes
                    // if an ExpressionSyntax node of another type is found, we mark this method non-empty
                    var expressions = methodSyntax.DescendantNodes().OfType<ExpressionSyntax>();
                    if (expressions.Any((expression) =>
                    {
                        return !(expression is InvocationExpressionSyntax) && !(expression is TypeSyntax);
                    }))
                    {
                        isNonEmpty = true;
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
                        if (!CheckEmptyMethod(context, callMethodSymbol, searched))
                        {
                            isNonEmpty = true;
                            break;
                        }
                    }
                }
            }

            // if this method is marked as non-empty, we change the flag value in the dictionary
            if (isNonEmpty)
            {
                searched[methodSymbol] = false;
            }

            return searched[methodSymbol];
        }
    }
}