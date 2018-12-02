using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer
{
    /// a struct to describe a replacable method
    struct ReplacableMethod
    {
        public string NameSpace;
        public string Name;
        public string Alter;
    }

    public static class RolsynExtensions
    {
        /// cached dictionary for each file path indicating whether it is a generated file.
        private static Dictionary<string, bool> _cachedGeneratedFileCheck = new Dictionary<string, bool>();
        

        /// cached dictionary for each file storing all the preprocessor nodes within it.
        private static Dictionary<int, KeyValuePair<SyntaxTree, List<SyntaxNode>>> _cachedPreprocessors =
            new Dictionary<int, KeyValuePair<SyntaxTree, List<SyntaxNode>>>();


        public static bool IsCLI = false;

        public static bool TryGetSymbolInfo(this SyntaxNodeAnalysisContext context, SyntaxNode node,
            out SymbolInfo symbolInfo)
        {
            try
            {
                //NOTE: The Call below fixes many issues where the symbol cannot be found - but there are still cases where an argumentexception is thrown
                // which seems to resemble this issue: https://github.com/dotnet/roslyn/issues/11193

                var semanticModel = SemanticModelFor(context.SemanticModel, node);

                symbolInfo = semanticModel.GetSymbolInfo(node); //context.SemanticModel.GetSymbolInfo(node);
                return true;
            }
            catch (Exception generalException)
            {
                Debug.WriteLine("Unable to find Symbol: " + node);
                Debug.WriteLine(generalException);
            }

            symbolInfo = default(SymbolInfo);
            return false;
        }

        internal static SemanticModel SemanticModelFor(SemanticModel semanticModel, SyntaxNode expression)
        {
            if (ReferenceEquals(semanticModel.SyntaxTree, expression.SyntaxTree))
            {
                return semanticModel;
            }

            //NOTE: there may be a performance boost if we cache some of the semantic models
            return semanticModel.Compilation.GetSemanticModel(expression.SyntaxTree);
        }

        public static bool IsDerived(this ClassDeclarationSyntax classDeclaration)
        {
            return (classDeclaration.BaseList != null && classDeclaration.BaseList.Types.Count > 0);
        }

        public static bool IsSealed(this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Modifiers.Any(m => m.Kind() == SyntaxKind.SealedKeyword);
        }

        public static bool IsSealed(this MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.Modifiers.Any(m => m.Kind() == SyntaxKind.SealedKeyword);
        }

        public static bool IsOverriden(this MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.Modifiers.Any(m => m.Kind() == SyntaxKind.OverrideKeyword);
        }

        /// Determine if the given class is a MonoBehavior-derived class.
        /// <param name="classDeclaration">the class to test.</param>
        public static bool IsMonoBehavior(this INamedTypeSymbol classDeclaration)
        {
            // base class of the testing one
            if (classDeclaration?.BaseType == null)
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

        public static string MethodName(this InvocationExpressionSyntax invocation)
        {
            string name = string.Empty;

            if (invocation.Expression is MemberAccessExpressionSyntax)
            {
                name = ((MemberAccessExpressionSyntax) invocation.Expression).Name.Identifier.ToString();
            }
            else if (invocation.Expression is IdentifierNameSyntax)
            {
                name = ((IdentifierNameSyntax) invocation.Expression).ToString();
            }
            else if (invocation.Expression is GenericNameSyntax)
            {
                name = ((GenericNameSyntax) invocation.Expression).Identifier.ToString();
            }
            else if (invocation.Expression is InvocationExpressionSyntax)
            {
                name = MethodName(invocation.Expression as InvocationExpressionSyntax);
            }
            else
            {
                //throw new ArgumentException("Unable to determine name of method: " + invocation.GetText());
            }

            return name;
        }

        public static T GetArgumentValue<T>(this ArgumentSyntax argument)
        {
            //NOTE: Possibly add support for constant parameters in the future

            if (argument?.Expression is LiteralExpressionSyntax)
            {
                var argumentValue = ((LiteralExpressionSyntax) argument.Expression).Token.Value;
                return (T) argumentValue;
            }

            return default(T);
        }


        /// Check whether a given node is under a specified kind of Syntax node (recursively)
        public static bool IsUnderSyntaxOfKind(this SyntaxNode node, SyntaxKind kind)
        {
            if (node == null)
            {
                return false;
            }
            else if (node?.Kind() == kind)
            {
                return true;
            }
            else return IsUnderSyntaxOfKind(node?.Parent, kind);
        }


        public static bool IsAssignmentLeftValue(this SyntaxNode node)
        {
            var assign = node.Ancestors().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
            if (assign?.Left == node)
            {
                return true;
            }

            return false;
        }


        public static bool IsInDebugInvocation(this SyntaxNode node, SemanticModel sm)
        {
            return node.Ancestors().Any((parent) =>
            {
                if (parent is ThrowStatementSyntax)
                {
                    return true;
                }

                if (parent is InvocationExpressionSyntax)
                {
                    var method = sm.GetSymbolInfo(parent).Symbol as IMethodSymbol;
                    var type = method?.ContainingType;
                    if ((type?.Name?.Equals("Debug") == true &&
                         type?.ContainingNamespace?.ToString().Equals("UnityEngine") == true) ||
                        (type?.Name?.Equals("ILogSystem") == true &&
                         type?.ContainingNamespace?.ToString().Equals("X2Interface") == true))
                    {
                        return true;
                    }
                }

                return false;
            });
        }


        public static bool IsReviewed(this SyntaxNode node)
        {
            return node.DescendantTrivia().Any((trivia) =>
            {
                return trivia.ToString().Equals("/*UnityEngineAnalyzer reviewed*/");
            });
        }


        /// White list check for a given syntax node
        public static bool IsExcluded(this SyntaxNode node, string diagnosticID)
        {
            if (IsUnderSyntaxOfKind(node, SyntaxKind.Attribute)) return true;
            if (node.IsReviewed()) return true;

            var filepath = node?.SyntaxTree?.FilePath ?? string.Empty;
            bool isGeneratedFile;
            if (_cachedGeneratedFileCheck.ContainsKey(filepath))
            {
                isGeneratedFile = _cachedGeneratedFileCheck[filepath];
            }
            else
            {
                isGeneratedFile =
                    // lua wrap file
                    System.Text.RegularExpressions.Regex.IsMatch(filepath, @".*XLua[\\\/]+Gen.*Wrap\.cs") ||
                    // proto file
                    System.Text.RegularExpressions.Regex.IsMatch(filepath, @".*\.proto\.cs");
                _cachedGeneratedFileCheck[filepath] = isGeneratedFile;
            }

            switch (diagnosticID)
            {
                case DiagnosticIDs.DoNotInitContainerWithoutSize:
                    if (isGeneratedFile)
                    {
                        return true;
                    }

                    break;
                case DiagnosticIDs.DoNotUseCameraMain:
                    if (isGeneratedFile)
                    {
                        return true;
                    }

                    break;
                case DiagnosticIDs.DoNotUseCameraRect:
                    if (isGeneratedFile)
                    {
                        return true;
                    }

                    break;
                case DiagnosticIDs.DoNotUseStringMethods:
                    if (isGeneratedFile)
                    {
                        return true;
                    }

                    break;
                case DiagnosticIDs.DoNotUseCoroutines:
                    if (isGeneratedFile)
                    {
                        return true;
                    }

                    break;
                case DiagnosticIDs.Boxing:
                    if (isGeneratedFile)
                    {
                        return true;
                    }

                    break;
                case DiagnosticIDs.DoNotUseDefaultDebug:
                    if (isGeneratedFile)
                    {
                        return true;
                    }

                    break;
                default:
                    break;
            }

            if (IsInUnityEditorProcessor(node)) return true;

            return false;
        }


        /// White list check for a given syntax context (check its node)
        public static bool IsExcluded(this SyntaxNodeAnalysisContext context, string diagnosticID)
        {
            UEASettingReader uEASettingReader = new UEASettingReader();
            uEASettingReader.ReadConfigFile();
            string assembly = context.SemanticModel.Compilation.AssemblyName;
            if (assembly != "Assembly-CSharp.dll" &&
                assembly != "Assembly-CSharp" &&
                assembly != "com.unity.postprocessing.Runtime" &&
                assembly != "com.unity.postprocessing.Runtime.dll") return true;
            if (IsInDebugInvocation(context.Node, context.SemanticModel)) return true;
            if (CheckWhiteList(context, uEASettingReader)) return true;
            

            return IsExcluded(context.Node, diagnosticID);
        }

        public static bool CheckWhiteList(SyntaxNodeAnalysisContext context, UEASettingReader uEASettingReader)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot() as CompilationUnitSyntax;
            if (root == null)
            {
                return true;
            }

            // check if in UnityEditor directory
            var filePath = root.GetLocation().ToString();
            UEASettings uEASettings = uEASettingReader.ReadConfigFile();
            foreach (var regexStatement in uEASettings.ignoreFoldeRegexStrings)
            {
                var regex = new System.Text.RegularExpressions.Regex(regexStatement);
                if (regex.IsMatch(filePath))
                {
                    return true;
                }
            }
            return false;
        }

        /// White list check for a given syntax node
        public static bool IsExcluded(this SyntaxNode node)
        {
            return IsExcluded(node, string.Empty);
        }


        /// Check whether a given syntax node is within a UNITY_EDITOR block
        public static bool IsInUnityEditorProcessor(this SyntaxNode syntax)
        {
            // if this code is executed from CLI, we skip this check 
            // since UNITY_EDITOR is already detached from parsing in CLI program.
            if (IsCLI)
            {
                return false;
            }

            if (syntax == null)
            {
                return false;
            }

            var tree = syntax.SyntaxTree;
            if (tree == null)
            {
                return false;
            }

            var path = tree.FilePath.GetHashCode();

            List<SyntaxNode> preprocessors = null;
            if (_cachedPreprocessors.ContainsKey(path))
            {
                preprocessors = _cachedPreprocessors[path].Value ?? new List<SyntaxNode>();
                if (_cachedPreprocessors[path].Key != tree)
                {
                    preprocessors.Clear();
                    GetDirectiveTriviaRecursively(tree.GetRoot(), preprocessors);

                    _cachedPreprocessors[path] = new KeyValuePair<SyntaxTree, List<SyntaxNode>>(tree, preprocessors);
                }
            }
            else
            {
                preprocessors = new List<SyntaxNode>();
                GetDirectiveTriviaRecursively(tree.GetRoot(), preprocessors);

                _cachedPreprocessors[path] = new KeyValuePair<SyntaxTree, List<SyntaxNode>>(tree, preprocessors);
            }

            return CheckNodeUnderPreprocessScope(preprocessors, syntax, "UNITY_EDITOR");
        }


        private static void GetDirectiveTriviaRecursively(SyntaxNodeOrToken node, List<SyntaxNode> list)
        {
            if (node == null)
            {
                return;
            }

            if (list == null)
            {
                list = new List<SyntaxNode>();
            }

            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.ContainsDirectives)
                {
                    if (child.IsToken)
                    {
                        foreach (var trivia in ((SyntaxToken) child).GetAllTrivia())
                        {
                            if (trivia.IsDirective && trivia.HasStructure)
                            {
                                var trivianode = trivia.GetStructure();
                                switch (trivianode.Kind())
                                {
                                    case SyntaxKind.IfDirectiveTrivia:
                                    case SyntaxKind.ElseDirectiveTrivia:
                                    case SyntaxKind.ElifDirectiveTrivia:
                                    case SyntaxKind.EndIfDirectiveTrivia:
                                        list.Add(trivianode);
                                        break;
                                    default:
                                        GetDirectiveTriviaRecursively(trivianode, list);
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        switch (child.Kind())
                        {
                            case SyntaxKind.IfDirectiveTrivia:
                            case SyntaxKind.ElseDirectiveTrivia:
                            case SyntaxKind.ElifDirectiveTrivia:
                            case SyntaxKind.EndIfDirectiveTrivia:
                                list.Add((SyntaxNode) child);
                                break;
                            default:
                                GetDirectiveTriviaRecursively(child, list);
                                break;
                        }
                    }
                }
            }
        }


        private static bool CheckNodeUnderPreprocessScope(List<SyntaxNode> directiveList, SyntaxNode nodeToCheck,
            string preprocessor)
        {
            if (directiveList == null || nodeToCheck == null || preprocessor == null)
            {
                return false;
            }

            var nodeStart = nodeToCheck.GetLocation().SourceSpan.Start;
            var nodeEnd = nodeToCheck.GetLocation().SourceSpan.End;
            int depth = 0;
            int firstPreprocessorDepth = 0;
            bool flag = false;
            foreach (var node in directiveList)
            {
                switch (node.Kind())
                {
                    case SyntaxKind.IfDirectiveTrivia:
                        depth++;
                        if (FoundPreprocessorInThisNode(node as ConditionalDirectiveTriviaSyntax, preprocessor))
                        {
                            if (firstPreprocessorDepth == 0 && node.GetLocation().SourceSpan.Start < nodeStart)
                            {
                                firstPreprocessorDepth = depth;
                            }

                        }
                        break;
                    case SyntaxKind.ElifDirectiveTrivia:
                        if (FoundPreprocessorInThisNode(node as ConditionalDirectiveTriviaSyntax, preprocessor))
                        {
                            if (firstPreprocessorDepth == 0 && node.GetLocation().SourceSpan.Start < nodeStart)
                            {
                                firstPreprocessorDepth = depth;
                            }
                        }
                        break;
                    case SyntaxKind.ElseDirectiveTrivia:
                        //curIf = !curIf;
                        //if (curIf)
                        //{
                        //    if (firstPreprocessorDepth == 0)
                        //    {
                        //        firstPreprocessorDepth = depth;
                        //    }
                        //}
                        break;
                    case SyntaxKind.EndIfDirectiveTrivia:
                        
                        if (firstPreprocessorDepth != 0 && depth >= firstPreprocessorDepth && node.GetLocation().SourceSpan.Start > nodeEnd)
                        {
                            flag = true;
                        }

                        depth--;

                        if (depth < firstPreprocessorDepth)
                        {
                            firstPreprocessorDepth = 0;
                        }

                        break;
                    default:
                        break;
                }

                if (flag)
                {
                    break;
                }
            }

            return flag;
        }


        private static bool FoundPreprocessorInThisNode(ConditionalDirectiveTriviaSyntax node, string preprocessor)
        {
            if (node == null || preprocessor == null)
            {
                return false;
            }

            var children = node.Condition.DescendantNodesAndSelf();
            return children.Where((n) =>
            {
                if (n.Kind() == SyntaxKind.IdentifierName
                    && n.Parent?.Kind() != SyntaxKind.LogicalNotExpression
                    && (n as IdentifierNameSyntax).Identifier.ValueText.Equals(preprocessor))
                {
                    return true;
                }

                return false;
            }).Count() > 0;
        }


        public static string SyntaxStackToString(this Stack<SyntaxNode> callHierarchy)
        {
            string ret = string.Empty;
            if (callHierarchy != null)
            {
                foreach (var call in callHierarchy)
                {
                    ret += "--->" + call.ToString() + "\n";
                }
            }

            return ret;
        }

        public static string SymbolStackToString(this Stack<ISymbol> callHierarchy)
        {
            string ret = string.Empty;
            if (callHierarchy != null)
            {
                foreach (var call in callHierarchy)
                {
                    ret += "--->" + call.ToString() + "\n";
                }
            }

            return ret;
        }

        public static void RegisterSyntaxNodeActionCatchable<TLanguageKindEnum>(this AnalysisContext context,
            Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds)
            where TLanguageKindEnum : struct
        {
            context.RegisterSyntaxNodeAction((c) =>
            {
                try
                {
                    action(c);
                }
                catch (Exception e)
                {
                    CommonAnalyzer.CommonReport(e.ToString());
                }
            }, syntaxKinds);
        }


        public static MethodDeclarationSyntax GetMethodDeclaration(this IMethodSymbol method)
        {
            var declares = method?.DeclaringSyntaxReferences;
            if (declares == null || declares.Value.Length == 0)
            {
                return null;
            }

            return declares.Value[0].GetSyntax() as MethodDeclarationSyntax;
        }


        public static void ForEachMonoBehaviourUpdateMethod(this SyntaxNodeAnalysisContext context,
            Action<MethodDeclarationSyntax> callback)
        {
            var mono = new MonoBehaviourInfo(context);
            mono.ForEachUpdateMethod(callback);
        }


        public static SyntaxNode FindClosestCommonParent<T>(this IList<T> nodes) where T : SyntaxNode
        {
            if (nodes == null)
            {
                return null;
            }

            SyntaxNode firstNotNull = null;
            foreach (var node in nodes)
            {
                if (node != null)
                {
                    firstNotNull = node;
                    break;
                }
            }

            if (firstNotNull == null)
            {
                return null;
            }

            SyntaxNode curParent = null;
            foreach (var parent in firstNotNull.Ancestors())
            {
                curParent = parent;
                bool flag = true;
                foreach (var node in nodes)
                {
                    if (node != firstNotNull && node != null)
                    {
                        if (!node.Ancestors().Contains(parent))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (flag)
                {
                    break;
                }
            }

            return curParent;
        }


        public static SyntaxNode FindClosestIfBlock(this SyntaxNode node)
        {
            if (node == null) return null;
            SyntaxNode parent = node.Parent;
            while (parent != null)
            {
                if (parent.Parent is IfStatementSyntax)
                {
                    return parent;
                }

                parent = parent.Parent;
            }

            return null;
        }


        public static SyntaxNode Root(this SyntaxNode node)
        {
            if (node.Parent == null)
            {
                return node;
            }

            return Root(node.Parent);
        }


        public static SymbolInfo GetSymbolInfoSafe(this SemanticModel sm, SyntaxNode node)
        {
            if (node == null)
            {
                return default(SymbolInfo);
            }

            return sm.GetSymbolInfo(node);
        }

        public static TypeInfo GetTypeInfoSafe(this SemanticModel sm, SyntaxNode node)
        {
            if (node == null)
            {
                return default(TypeInfo);
            }

            return sm.GetTypeInfo(node);
        }

        public static ISymbol GetDeclaredSymbolSafe(this SemanticModel sm, SyntaxNode node)
        {
            if (node == null)
            {
                return null;
            }

            return sm.GetDeclaredSymbol(node);
        }
    }
}