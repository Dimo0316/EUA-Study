using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer
{
    /// Helper class to check a MonoBehavior-derived class.
    class MonoBehaviourInfo
    {
        /// all Update methods in MonoBehaviour classes to test.
        private static readonly ImmutableHashSet<string> UpdateMethodNames = ImmutableHashSet.Create(
            "OnGUI",
            "Update",
            "FixedUpdate",
            "LateUpdate");

        /// class declaration of this Mono-derived class.
        private readonly ClassDeclarationSyntax _classDeclaration;

        /// class type representation.
        private readonly INamedTypeSymbol _classSymbol;


        /// Constructor.
        /// <param name="context">context for a symbol action.</param>
        public MonoBehaviourInfo(SyntaxNodeAnalysisContext analysisContext)
        {
            _classDeclaration = analysisContext.Node as ClassDeclarationSyntax;
            _classSymbol = analysisContext.SemanticModel.GetDeclaredSymbol(_classDeclaration) as INamedTypeSymbol;
            if (_classSymbol != null)
            {
                this.ClassName = _classSymbol.Name;
            }
        }


        /// class name of this Mono-derived class.
        public string ClassName { get; private set; }


        /// Find all Update methods in this MonoBehavior-derived class, and invoke the given callback with these functions.
        /// <param name="callback">callback to be invoked with all Update functions in a MonoBehavior-derived class.</param>
        public void ForEachUpdateMethod(Action<MethodDeclarationSyntax> callback)
        {
            // if this is a MonoBehavior-derived class
            if (this.IsMonoBehaviour())
            {
                // as for all Update methods, invoke the passed-in callback
                var methods = _classDeclaration.Members.OfType<MethodDeclarationSyntax>();
                foreach (var method in methods)
                {
                    if (UpdateMethodNames.Contains(method.Identifier.ValueText))
                    {
                        callback(method);
                    }
                }
            }
        }


        public void ForEachUpdateMethodSymbol(SemanticModel sm, Action<IMethodSymbol> callback)
        {
            ForEachUpdateMethod((syntax) =>
            {
                var symbol = sm.GetDeclaredSymbol(syntax) as IMethodSymbol;
                if (symbol != null && callback != null)
                {
                    callback(symbol);
                }
            });
        }


        /// Determine if this class is a MonoBehavior-derived class.
        public bool IsMonoBehaviour()
        {
            return IsMonoBehavior(_classSymbol);
        }


        /// Determine if the given class is a MonoBehavior-derived class.
        /// <param name="classDeclaration">the class to test.</param>
        private static bool IsMonoBehavior(INamedTypeSymbol classDeclaration)
        {
            // base class of the testing one
            if (classDeclaration.BaseType == null)
            {
                return false;
            }

            var baseClass = classDeclaration.BaseType;

            // if this testing class is a Mono-derived eclass
            if (baseClass.ContainingNamespace.Name.Equals("UnityEngine") && baseClass.Name.Equals("MonoBehaviour"))
            {
                return true;
            }

            // determine if the BaseClass extends MonoBehavior
            return IsMonoBehavior(baseClass);
        }
    }
}