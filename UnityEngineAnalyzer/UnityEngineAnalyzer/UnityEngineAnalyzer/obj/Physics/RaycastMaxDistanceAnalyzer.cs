using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Physics
{
    /// A diagnostic analyzer to check "maxDistance option of Physics.Raycast".
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RaycastMaxDistanceAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///     list of all methods of Physics which contains a "maxDistance" option
        /// </summary>
        private static ImmutableDictionary<string, ImmutableArray<string>> PhysicsRaycastMethods =
            ImmutableDictionary.CreateRange(
                new Dictionary<string, ImmutableArray<string>>
                {
                    {"BoxCast", ImmutableArray.Create("maxDistance")},
                    {"BoxCastAll", ImmutableArray.Create("maxDistance")},
                    {"BoxCastNonAlloc", ImmutableArray.Create("maxDistance")},
                    {"CapsuleCast", ImmutableArray.Create("maxDistance")},
                    {"CapsuleCastAll", ImmutableArray.Create("maxDistance")},
                    {"CapsuleCastNonAlloc", ImmutableArray.Create("maxDistance")},
                    {"Raycast", ImmutableArray.Create("maxDistance")},
                    {"RaycastAll", ImmutableArray.Create("maxDistance")},
                    {"RaycastNonAlloc", ImmutableArray.Create("maxDistance")},
                    {"SphereCast", ImmutableArray.Create("maxDistance")},
                    {"SphereCastAll", ImmutableArray.Create("maxDistance")},
                    {"SphereCastNonAlloc", ImmutableArray.Create("maxDistance")},
                }
            );


        /// <summary>
        ///     list of all methods of Physics2D which contains a "distance"/"minDepth"/"maxDepth" option
        /// </summary>
        private static ImmutableDictionary<string, ImmutableArray<string>> Physics2DRaycastMethods =
            ImmutableDictionary.CreateRange(
                new Dictionary<string, ImmutableArray<string>>
                {
                    {"BoxCast", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"BoxCastAll", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"BoxCastNonAlloc", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"CapsuleCast", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"CapsuleCastAll", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"CapsuleCastNonAlloc", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"CircleCast", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"CircleCastAll", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"CircleCastNonAlloc", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"GetRayIntersection", ImmutableArray.Create("distance")},
                    {"GetRayIntersectionAll", ImmutableArray.Create("distance")},
                    {"GetRayIntersectionNonAlloc", ImmutableArray.Create("distance")},
                    {"Linecast", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"LinecastAll", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"LinecastNonAlloc", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"OverlapArea", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapAreaAll", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapAreaNonAlloc", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapBox", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapBoxAll", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapBoxNonAlloc", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapCapsule", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapCapsuleAll", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapCapsuleNonAlloc", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapCircle", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapCircleAll", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapCircleNonAlloc", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapPoint", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapPointAll", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"OverlapPointNonAlloc", ImmutableArray.Create("minDepth", "maxDepth")},
                    {"Raycast", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"RaycastAll", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                    {"RaycastNonAlloc", ImmutableArray.Create("distance", "minDepth", "maxDepth")},
                }
            );

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.RaycastMaxDistance);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a method in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.RaycastMaxDistance))
            {
                return;
            }

            // retrieve the methos Syntax
            var invocation = context.Node as InvocationExpressionSyntax;

            // retrieve the method symbol
            var method = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            // retrieve the method name and its containing type
            var containingType = method?.ContainingType?.ToString();
            var methodName = method?.Name;
            if (containingType == null || methodName == null)
            {
                return;
            }

            // make sure this method is a Physics method
            var isPhysics = containingType.Equals("UnityEngine.Physics");
            var isPhysics2D = containingType.Equals("UnityEngine.Physics2D");
            if (!(isPhysics && PhysicsRaycastMethods.Keys.Contains(methodName) ||
                  isPhysics2D && Physics2DRaycastMethods.Keys.Contains(methodName)))
            {
                return;
            }

            // If this method passes all the checkings above, it must have the specified parameter/parameters,
            // so the following steps check the parameters of this method.
            // We reserve a flag indicating whether its parameters pass the checkings.
            bool flag = false;

            // it must have some parameters
            var parameters = method.Parameters;
            var requiredParameters =
                isPhysics ? PhysicsRaycastMethods[methodName] : Physics2DRaycastMethods[methodName];
            if (parameters != null)
            {
                // all the required parameter names must be implemented in the parameter list
                if (IsParametersAllImplemented(requiredParameters, parameters))
                {
                    flag = true;
                }
            }

            // if the flag is not true, this invocation doesn't pass the parameter checking
            if (!flag)
            {
                // report a diagnostic
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.RaycastMaxDistance, invocation.GetLocation(),
                    NameListToString(requiredParameters), isPhysics ? "Physics" : "Physics2D" + "." + methodName);
                context.ReportDiagnostic(diagnostic);
            }
        }


        /// checks whether each of the required parameter names is implemented in the parameter list
        private static bool IsParametersAllImplemented(ImmutableArray<string> required,
            ImmutableArray<IParameterSymbol> parameters)
        {
            return required.All(requiredPara => parameters.Any(para => true == para?.Name?.Equals(requiredPara)));
        }


        /// convert a list of string to string
        private static string NameListToString(IEnumerable<string> list)
        {
            return "\"" + string.Join("\"/\"", list) + "\"";
        }
    }
}