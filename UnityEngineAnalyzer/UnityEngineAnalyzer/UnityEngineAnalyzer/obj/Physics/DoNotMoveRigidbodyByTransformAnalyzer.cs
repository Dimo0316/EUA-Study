using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Physics
{
    /// A diagnostic analyzer to check "move rigidbody by Transform API".
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotMoveRigidbodyByTransformAnalyzer : DiagnosticAnalyzer
    {
        /// list of all transform API which affects position and rotation
        private static ImmutableArray<string> TransformAPI = ImmutableArray.Create(
            "eulerAngles",
            "forward",
            "localEulerAngles",
            "localPosition",
            "localRotation",
            "position",
            "right",
            "rotation",
            "up",
            "LookAt",
            "Rotate",
            "RotateAround",
            "RotateAroundLocal",
            "SetPositionAndRotation",
            "Translate"
        );

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotMoveRigidbodyByTransform);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a method in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotMoveRigidbodyByTransform))
            {
                return;
            }

            // retrieve the class declaration syntax
            var classDeclareSyntax = context.Node as ClassDeclarationSyntax;

            // make sure this class is a MonoBehaviour which requires a Rigidbody component
            if (!IsRigidbodyMonoBehaviour(context.SemanticModel, classDeclareSyntax))
            {
                return;
            }

            foreach (var node in context.Node.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Where(access =>
            {
                // check whether this access syntax is a Transform API access on "transform" or "gameObject.transform"
                if (TransformAPI.Contains(access?.Name?.Identifier.Text) &&
                    (true == access?.Expression.ToString().Equals("transform") ||
                     true == access?.Expression.ToString().Equals("gameObject.transform") ||
                     true == access?.Expression.ToString().Equals("this.transform") ||
                     true == access?.Expression.ToString().Equals("this.gameObject.transform")))
                {
                    // if this is a property access, it should be left value of an assignment
                    if (context.SemanticModel.GetSymbolInfo(access).Symbol is IPropertySymbol)
                    {
                        if (access.IsAssignmentLeftValue())
                            return true;
                        else
                            return false;
                    }

                    return true;
                }

                return false;
            }))
            {
                // report a diagnostic
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotMoveRigidbodyByTransform,
                    node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }


        /// Checks whether a class is a monobehavior which requires a rigidbody component
        private static bool IsRigidbodyMonoBehaviour(SemanticModel sm, ClassDeclarationSyntax classDeclare)
        {
            // make sure the data flowing in is not null
            if (classDeclare == null || sm == null)
            {
                return false;
            }

            // check whether this class is MonoBehaviour
            var symbol = sm.GetDeclaredSymbol(classDeclare) as INamedTypeSymbol;
            if (!symbol.IsMonoBehavior())
            {
                return false;
            }

            // retrieve the attribute list of the class
            var attributes = classDeclare.AttributeLists;
            if (attributes != null)
            {
                foreach (var attributeList in attributes)
                {
                    // checks each of the attribute of the class
                    if (true == attributeList?.Attributes.Any(attr =>
                    {
                        bool hasRequireRigidbody = false;

                        // checks whether it is a RequireComponent attribute
                        var attrType = sm.GetTypeInfo(attr).ConvertedType;
                        if (true == attrType?.Name?.Equals("RequireComponent") &&
                            true == attrType?.ContainingNamespace?.ToString().Equals("UnityEngine"))
                        {
                            // if true, check whether this attribute has any "typeof(Rigidbody)" argument
                            if (true == attr?.ArgumentList?.Arguments.Any(arg =>
                            {
                                bool isArgRigidbody = false;

                                // find any TypeOfExpression syntax
                                if (arg?.Expression is TypeOfExpressionSyntax)
                                {
                                    // check whether the type of this TypeOfExpression is UnityEngine.Rigidbody
                                    var typeofSyntax = arg.Expression as TypeOfExpressionSyntax;
                                    var type = sm.GetTypeInfo(typeofSyntax?.Type).Type;
                                    if ((true == type?.Name?.Equals("Rigidbody") ||
                                         true == type?.Name?.Equals("Rigidbody2D")) &&
                                        true == type?.ContainingNamespace?.ToString().Equals("UnityEngine"))
                                    {
                                        isArgRigidbody = true;
                                    }
                                }

                                return isArgRigidbody;
                            }))
                            {
                                hasRequireRigidbody = true;
                            }
                        }

                        return hasRequireRigidbody;
                    }))
                    {
                        // if it has any attribute as "RequireComponent(typeof(Rigidbody))", it is qualified
                        return true;
                    }
                }
            }

            return false;
        }
    }
}