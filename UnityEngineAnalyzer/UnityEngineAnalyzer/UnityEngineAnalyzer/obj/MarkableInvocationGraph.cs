using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer
{
    public delegate SyntaxNode PredicateDelegate(SemanticModel sm, MethodDeclarationSyntax methodDeclare);

    class MarkableInvocationGraph
    {
        // cached info of searching recursion depth, in purpose of avoiding StackOverFlow
        int _depth = 0;
        Stack<ISymbol> _searchStack = new Stack<ISymbol>();
        Dictionary<int, MarkInfo> markDict = new Dictionary<int, MarkInfo>();

        PredicateDelegate predicate;

        public MarkableInvocationGraph(PredicateDelegate predicate)
        {
            this.predicate = predicate;
        }

        private void Pop()
        {
            _depth--;
            _searchStack.Pop();
        }

        /// <summary>
        ///     Search recursively a given method to find whether it or any child call of it meets the specified predication
        /// </summary>
        public bool SearchMethod(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax declare,
            Stack<SyntaxNode> callHierarchy, MarkInfo parent = null)
        {
            // =========================================
            // 1. some previous work before checking 
            // =========================================

            // retrieve the symbol for the method declaration
            var method = RolsynExtensions.SemanticModelFor(context.SemanticModel, declare)?.GetDeclaredSymbol(declare);
            if (method == null)
            {
                return false;
            }

            // if parent equals null, this is a starting node of a searching chain, so we reset the cached infomation about the searching stack
            if (parent == null)
            {
                _depth = 1;
                _searchStack.Clear();
                _searchStack.Push(method);
            }
            else
            {
                _depth++;
                _searchStack.Push(method);

                // we do this just in purpose of avoiding the potential probability of endless searching nesting in case of any accidential error occurring, 
                // which could throw a StackOverflowException and shut down the application as a severe result.
                if (_depth > 32)
                {
                    Pop();
                    CommonAnalyzer.CommonReport("[UnityEngineAnalyzer] The Stack is ignored for its heavy depth: \n" +
                                                _searchStack.SymbolStackToString());
                    return false;
                    //throw new Exception("[UnityEngineAnalyzer] The Stack is ignored for its heavy depth: \n" + _searchStack.SymbolStackToString());
                }
            }

            // check declaration exclusion
            if (declare.IsExcluded())
            {
                Pop();
                return false;
            }

            // make sure the predication is not null
            if (predicate == null)
            {
                Pop();
                return false;
            }

            // get the hashcode of this method (and check method validation)
            int methodCode = GetMethodHashCode(method);
            if (methodCode == 0)
            {
                Pop();
                return false;
            }

            // make sure the call hierarchy is not null
            if (callHierarchy == null)
            {
                callHierarchy = new Stack<SyntaxNode>();
            }

            // ==========================================================
            // 2. check the cache dictionary to find out 
            //    whether this method is ever checked 
            //    and whether the marked infomation is still validate
            // ==========================================================

            // if we have already marked this method, check its syntaxTree to find whether the stored data is out-of-date
            // if not, just return the recorded value
            if (markDict.ContainsKey(methodCode))
            {
                var markInfo = markDict[methodCode];

                if (parent != null && !markInfo.parents.Contains(parent))
                {
                    markInfo.parents.Add(parent);
                }

                // if the stored SyntaxTree doesn't equal to the actual SyntaxTree, 
                // we should update the stored data with the newest, setting all its parents as dirty
                if (!MethodHasSameSyntaxTree(method, markInfo.method))
                {
                    //CommonAnalyzer.CommonReport("update: " + method.ToString() + ", " + markInfo.method.GetHashCode() + " -> " + method.GetHashCode());
                    markInfo.Update(method);
                }
                // if the stored data is dirty, which means some of its children has been rebuilt,
                // we should not use the stored data as result directly and should reset this node 
                // ( but not update it since it itself is still validate. 
                //   if we still update it and set all its parents dirty, we may get stuck in a loop )
                else if (markInfo.dirty)
                {
                    //CommonAnalyzer.CommonReport("dirty: " + method.ToString());
                    markInfo.ResetMark();
                }
                // otherwise we use the stored data as result
                else
                {
                    if (markInfo.mark_value)
                    {
                        foreach (var call in markInfo.callstack)
                        {
                            callHierarchy.Push(call);
                        }
                    }

                    Pop();
                    return markInfo.mark_value;
                }
            }
            else
            {
                // otherwise we mark this method with a default "false" value, since we might refer back to this method in codes below
                markDict[methodCode] = new MarkInfo(method);
                if (parent != null)
                {
                    markDict[methodCode].parents.Add(parent);
                }
            }


            // ==========================================================
            // 3. if the method is not marked in the cache dictionary, 
            //    we check over the expressions within the declaration
            // ==========================================================

            // flag indicating whether we found any qualified node within this method
            SyntaxNode hitNode = null;

            // if the declaration meets the predication, we record it
            var declareSM = RolsynExtensions.SemanticModelFor(context.SemanticModel, declare);
            if (declareSM != null)
            {
                hitNode = predicate(declareSM, declare);
            }


            // ============================================================
            // 4. The last step is to check the declarations recursively
            //    of all the child invocations
            // ============================================================

            if (hitNode == null)
            {
                // if the iteration doesn't break above, meaning this declaration itself doesn't match the predication,
                // so we will check its child invocations.
                var childcalls = declare.DescendantNodes().OfType<InvocationExpressionSyntax>();
                if (childcalls != null)
                {
                    foreach (var childcall in childcalls)
                    {
                        if (childcall.IsExcluded())
                        {
                            continue;
                        }

                        // make sure the invocation is a valid method
                        var childMethodSymbol = declareSM?.GetSymbolInfo(childcall).Symbol as IMethodSymbol;
                        if (childMethodSymbol == null)
                        {
                            continue;
                        }

                        var childDeclare = childMethodSymbol.GetMethodDeclaration();
                        if (childDeclare == null)
                        {
                            continue;
                        }

                        // check the state this child invocation is marked as
                        var ret = SearchMethod(context, childDeclare, callHierarchy, markDict[methodCode]);

                        // if this child invocation is marked as "true", then record it and break.
                        if (ret)
                        {
                            hitNode = childcall;
                            break;
                        }
                    }
                }
            }

            // if this method is qualified, update the hierarchy stack and return
            if (hitNode != null)
            {
                callHierarchy.Push(hitNode);
                markDict[methodCode].Mark(callHierarchy);
            }

            Pop();
            return hitNode != null;
        }


        /// <summary>
        ///     Given a method symbol, return its hashcode
        /// </summary>
        int GetMethodHashCode(IMethodSymbol method)
        {
            return method?.OriginalDefinition?.ToString().GetHashCode() ?? 0;
        }


        /// <summary>
        ///     check whether the given two method symbol share the same SyntaxTree
        /// </summary>
        bool MethodHasSameSyntaxTree(IMethodSymbol m1, IMethodSymbol m2)
        {
            var t1 = m1.DeclaringSyntaxReferences.Length > 0 ? m1.DeclaringSyntaxReferences[0].SyntaxTree : null;
            var t2 = m2.DeclaringSyntaxReferences.Length > 0 ? m2.DeclaringSyntaxReferences[0].SyntaxTree : null;

            return ReferenceEquals(t1, t2);
        }


        public void CheckClassDirty(SyntaxNodeAnalysisContext context)
        {
            foreach (var methodDeclare in context.Node.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var method = context.SemanticModel.GetDeclaredSymbol(methodDeclare) as IMethodSymbol;
                var methodCode = GetMethodHashCode(method);
                if (markDict.ContainsKey(methodCode))
                {
                    if (!MethodHasSameSyntaxTree(method, markDict[methodCode].method))
                    {
                        markDict[methodCode].Update(method);
                        markDict[methodCode].dirty = true;
                    }
                }
            }
        }

        public class MarkInfo
        {
            public Stack<SyntaxNode> callstack = new Stack<SyntaxNode>();
            public bool dirty = false;
            public bool mark_value = false;
            public IMethodSymbol method;
            public List<MarkInfo> parents = new List<MarkInfo>();

            public MarkInfo(IMethodSymbol method)
            {
                this.mark_value = false;
                this.method = method;
            }

            public void Update(IMethodSymbol method)
            {
                this.mark_value = false;
                this.method = method;
                this.dirty = false;
                this.callstack.Clear();
                SetParentDirty();
            }

            public void Mark(Stack<SyntaxNode> callstack)
            {
                this.mark_value = true;
                this.callstack.Clear();
                foreach (var call in callstack)
                {
                    this.callstack.Push(call);
                }
            }

            public void ResetMark()
            {
                this.mark_value = false;
                this.dirty = false;
                this.callstack.Clear();
            }

            void SetParentDirty()
            {
                foreach (var parent in parents)
                {
                    if (!parent.dirty && parent != this)
                    {
                        parent.dirty = true;
                        parent.SetParentDirty();
                    }
                }
            }
        }
    }
}