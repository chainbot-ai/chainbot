using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Chainbot.Contracts.CodeAnalysis;
using Chainbot.Cores.ExpressionEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Chainbot.Cores.CodeAnalysis
{
    public class VisualBasicExpressionTokenizer : IExpressionTokenizer
    {
        private bool IsUnaryOperator(SyntaxNode node)
        {
            return node != null && (node.IsKind(SyntaxKind.UnaryMinusExpression) || node.IsKind(SyntaxKind.UnaryPlusExpression));
        }

        public IReadOnlyCollection<FlatExpressionToken> GetIdentifierAndLiteralList(string expression, IReadOnlyDictionary<string, Type> context)
        {
            if (context == null)
            {
                context = new Dictionary<string, Type>();
            }
            List<FlatExpressionToken> list = new List<FlatExpressionToken>();
            IEnumerable<SyntaxNode> rawRoslynSyntaxNodes = VisualBasicExpressionTokenizer.GetRawRoslynSyntaxNodes(expression);
            SyntaxNode syntaxNode = rawRoslynSyntaxNodes.FirstOrDefault((SyntaxNode i) => i is IdentifierNameSyntax);
            foreach (SyntaxNode syntaxNode2 in rawRoslynSyntaxNodes)
            {
                bool? flag;
                if (syntaxNode2 == null)
                {
                    flag = null;
                }
                else
                {
                    SyntaxNode parent = syntaxNode2.Parent;
                    flag = ((parent != null) ? new bool?(parent.IsKind(SyntaxKind.QualifiedName)) : null);
                }
                bool? flag2 = flag;
                if (!flag2.GetValueOrDefault() && !this.IsUnaryOperator((syntaxNode2 != null) ? syntaxNode2.Parent : null))
                {
                    GenericFlatExpressionToken genericFlatExpressionToken = null;
                    IdentifierNameSyntax identifierNameSyntax = syntaxNode2 as IdentifierNameSyntax;
                    if (identifierNameSyntax == null)
                    {
                        GenericNameSyntax genericNameSyntax = syntaxNode2 as GenericNameSyntax;
                        if (genericNameSyntax == null)
                        {
                            PredefinedTypeSyntax predefinedTypeSyntax = syntaxNode2 as PredefinedTypeSyntax;
                            if (predefinedTypeSyntax == null)
                            {
                                QualifiedNameSyntax qualifiedNameSyntax = syntaxNode2 as QualifiedNameSyntax;
                                if (qualifiedNameSyntax == null)
                                {
                                    LiteralExpressionSyntax literalExpressionSyntax = syntaxNode2 as LiteralExpressionSyntax;
                                    if (literalExpressionSyntax == null)
                                    {
                                        UnaryExpressionSyntax unaryExpressionSyntax = syntaxNode2 as UnaryExpressionSyntax;
                                        if (unaryExpressionSyntax == null)
                                        {
                                            ArgumentListSyntax argumentListSyntax = syntaxNode2 as ArgumentListSyntax;
                                            if (argumentListSyntax != null)
                                            {
                                                GenericFlatExpressionToken genericFlatExpressionToken2 = list.LastOrDefault<FlatExpressionToken>() as GenericFlatExpressionToken;
                                                if (genericFlatExpressionToken2 != null)
                                                {
                                                    string[] argumentExpressions = (from n in argumentListSyntax.ChildNodes()
                                                                                    select n.ToFullString()).ToArray<string>();
                                                    genericFlatExpressionToken2.SetArgumentExpressions(argumentExpressions);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            genericFlatExpressionToken = new GenericFlatExpressionToken
                                            {
                                                StringRepresentation = unaryExpressionSyntax.ToString(),
                                                Type = TokenType.LiteralUsage,
                                                DepthLevel = this.GetNestingLevel(unaryExpressionSyntax),
                                                ExpressionType = null
                                            };
                                        }
                                    }
                                    else
                                    {
                                        genericFlatExpressionToken = new GenericFlatExpressionToken
                                        {
                                            StringRepresentation = ((((literalExpressionSyntax != null) ? literalExpressionSyntax.Token.Value : null) as string) ?? literalExpressionSyntax.ToString()),
                                            Type = TokenType.LiteralUsage,
                                            DepthLevel = this.GetNestingLevel(literalExpressionSyntax),
                                            ExpressionType = this.GetLiteralTokenType(literalExpressionSyntax)
                                        };
                                    }
                                }
                                else
                                {
                                    genericFlatExpressionToken = new GenericFlatExpressionToken
                                    {
                                        StringRepresentation = qualifiedNameSyntax.ToString(),
                                        Type = TokenType.IdentifierReference,
                                        DepthLevel = this.GetNestingLevel(qualifiedNameSyntax),
                                        ExpressionType = null
                                    };
                                }
                            }
                            else
                            {
                                genericFlatExpressionToken = new GenericFlatExpressionToken
                                {
                                    StringRepresentation = predefinedTypeSyntax.Keyword.Text,
                                    Type = TokenType.IdentifierReference,
                                    DepthLevel = this.GetNestingLevel(predefinedTypeSyntax),
                                    ExpressionType = null
                                };
                            }
                        }
                        else
                        {
                            genericFlatExpressionToken = new GenericFlatExpressionToken
                            {
                                StringRepresentation = genericNameSyntax.Identifier.Text,
                                Type = TokenType.IdentifierReference,
                                DepthLevel = this.GetNestingLevel(genericNameSyntax),
                                ExpressionType = null
                            };
                        }
                    }
                    else
                    {
                        Type type = null;
                        if (context.ContainsKey(identifierNameSyntax.Identifier.Text))
                        {
                            type = context[identifierNameSyntax.Identifier.Text];
                        }
                        genericFlatExpressionToken = new GenericFlatExpressionToken
                        {
                            StringRepresentation = identifierNameSyntax.Identifier.Text,
                            Type = (this.IsObjectCreationIdentifier(identifierNameSyntax) ? TokenType.ObjectCreation : TokenType.IdentifierReference),
                            DepthLevel = this.GetNestingLevel(identifierNameSyntax),
                            ExpressionType = ((identifierNameSyntax == syntaxNode) ? type : null)
                        };
                    }
                    if (genericFlatExpressionToken != null)
                    {
                        genericFlatExpressionToken.SetStartPos(syntaxNode2.Span.Start - "Dim _temp=".Length);
                        genericFlatExpressionToken.SetEndPos(syntaxNode2.Span.End - "Dim _temp=".Length);
                        list.Add(genericFlatExpressionToken);
                    }
                }
            }
            return list.AsReadOnly();
        }

        public IReadOnlyCollection<string> GetUniqueIdentifiers(string expression)
        {
            HashSet<string> hashSet = new HashSet<string>();
            SyntaxNodeOrToken[] array = VisualBasicExpressionTokenizer.GetRawRoslynSyntaxNodesAndTokens(expression).ToArray<SyntaxNodeOrToken>();
            for (int i = 0; i < array.Length; i++)
            {
                SyntaxNodeOrToken nodeOrToken = array[i];
                if (nodeOrToken.IsKind(SyntaxKind.DotToken))
                {
                    i++;
                }
                else
                {
                    IdentifierNameSyntax identifierNameSyntax = nodeOrToken.AsNode() as IdentifierNameSyntax;
                    if (identifierNameSyntax != null && (i + 2 >= array.Length || !(array[i + 2].AsNode() is ArgumentListSyntax)))
                    {
                        hashSet.Add(identifierNameSyntax.Identifier.Text);
                    }
                }
            }
            return hashSet;
        }


        public string GetExpressionWithReplacedIdentifiers(string expression, string oldIdentifier, string newIdentifier)
        {
            if (string.IsNullOrEmpty(oldIdentifier) || string.IsNullOrEmpty(newIdentifier) || oldIdentifier.Equals(newIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                return expression;
            }
            return this.GetExpressionWithReplacedIdentifiersRecursively(expression, oldIdentifier, newIdentifier, 0);
        }

        public IEnumerable<string> GetMethodNameAndArgumentsList(string expression)
        {
            SyntaxNode syntaxNode = VisualBasicExpressionTokenizer.GetRawRoslynSyntaxNodes(expression).FirstOrDefault<SyntaxNode>();
            InvocationExpressionSyntax invocation = syntaxNode as InvocationExpressionSyntax;
            if (invocation != null)
            {
                yield return invocation.Expression.GetText(null, SourceHashAlgorithm.Sha1).ToString();
                foreach (ArgumentSyntax argumentSyntax in invocation.ArgumentList.Arguments)
                {
                    yield return argumentSyntax.GetText(null, SourceHashAlgorithm.Sha1).ToString();
                }
                SeparatedSyntaxList<ArgumentSyntax>.Enumerator enumerator = default(SeparatedSyntaxList<ArgumentSyntax>.Enumerator);
            }
            yield break;
        }

        private int GetNestingLevel(SyntaxNode node)
        {
            int num = 0;
            while (((node != null) ? node.Parent : null) != null)
            {
                num++;
                node = ((node != null) ? node.Parent : null);
            }
            return num;
        }


        private Type GetLiteralTokenType(LiteralExpressionSyntax literalNode)
        {
            SyntaxKind syntaxKind = literalNode.Kind();
            if (syntaxKind > SyntaxKind.StringLiteralExpression)
            {
                if (syntaxKind == SyntaxKind.FalseKeyword || syntaxKind == SyntaxKind.TrueKeyword)
                {
                    return typeof(bool);
                }
                if (syntaxKind != SyntaxKind.StringLiteralToken)
                {
                    return null;
                }
                else
                {
                    return typeof(string);
                }
            }
            else
            {
                if ((ushort)syntaxKind - (ushort)SyntaxKind.TrueLiteralExpression <= (ushort)SyntaxKind.List)
                {
                    return typeof(bool);
                }
                if (syntaxKind == SyntaxKind.StringLiteralExpression)
                {
                    return typeof(string);
                }
                return null;
            }
        }


        private bool IsObjectCreationIdentifier(IdentifierNameSyntax identifierNode)
        {
            Type left;
            if (identifierNode == null)
            {
                left = null;
            }
            else
            {
                SyntaxNode parent = identifierNode.Parent;
                left = ((parent != null) ? parent.GetType() : null);
            }
            return left == typeof(ObjectCreationExpressionSyntax);
        }


        private static IEnumerable<SyntaxNode> GetRawRoslynSyntaxNodes(string expression)
        {
            IEnumerable<SyntaxNode> result;
            try
            {
                result = VisualBasicExpressionTokenizer.GetFirstEquality(expression).Value.DescendantNodesAndSelf(null, true);
            }
            catch (Exception exception)
            {
                exception.Trace(null);
                result = Enumerable.Empty<SyntaxNode>();
            }
            return result;
        }

        private static IEnumerable<SyntaxNodeOrToken> GetRawRoslynSyntaxNodesAndTokens(string expression)
        {
            IEnumerable<SyntaxNodeOrToken> result;
            try
            {
                result = VisualBasicExpressionTokenizer.GetFirstEquality(expression).Value.DescendantNodesAndTokensAndSelf(null, true);
            }
            catch (Exception exception)
            {
                exception.Trace(null);
                result = Enumerable.Empty<SyntaxNodeOrToken>();
            }
            return result;
        }


        public static EqualsValueSyntax GetFirstEquality(string expression)
        {
            string text = "Dim _temp=" + expression;
            VisualBasicParseOptions options = new VisualBasicParseOptions(LanguageVersion.VisualBasic14, DocumentationMode.Parse, SourceCodeKind.Script, null);
            return ((VisualBasicSyntaxTree)VisualBasicSyntaxTree.ParseText(text, options, "", null, default(CancellationToken))).GetCompilationUnitRoot(default(CancellationToken)).DescendantNodes(null, false).OfType<EqualsValueSyntax>().FirstOrDefault<EqualsValueSyntax>();
        }

        public string EscapeStringLiteral(object rawString)
        {
            if (rawString == null)
            {
                return "Nothing";
            }
            string text = rawString as string;
            if (text == null)
            {
                return rawString.ToString();
            }
            return SyntaxFactory.Literal(text).ToString().Replace("“", "““").Replace("”", "””");
        }

        public string UnescapeStringLiteral(string literal)
        {
            string result;
            try
            {
                object value = VisualBasicExpressionTokenizer.GetRawRoslynSyntaxNodes(literal).ToArray<SyntaxNode>()[0].GetFirstToken(false, false, false, false).Value;
                result = (((value != null) ? value.ToString() : null) ?? literal);
            }
            catch (Exception)
            {
                result = literal;
            }
            return result;
        }


        public bool TryGetStringLiteral(string expression, out string literal)
        {
            literal = null;
            if (expression == "Nothing")
            {
                return true;
            }
            if (expression == "\"\"")
            {
                literal = "";
                return true;
            }
            if (expression != null && expression.Length > 2 && expression.StartsWith("\"") && expression.EndsWith("\""))
            {
                IReadOnlyCollection<FlatExpressionToken> identifierAndLiteralList = this.GetIdentifierAndLiteralList(expression, null);
                FlatExpressionToken flatExpressionToken = identifierAndLiteralList.FirstOrDefault<FlatExpressionToken>();
                if (identifierAndLiteralList.Count == 1 && flatExpressionToken.Type == TokenType.LiteralUsage)
                {
                    literal = flatExpressionToken.StringRepresentation;
                    return true;
                }
            }
            return false;
        }


        public string GetAsObjectCreationExpression(string objectName, IEnumerable<KeyValuePair<string, object>> arguments)
        {
            return string.Concat(new string[]
            {
                "new ",
                objectName,
                "(",
                string.Join(",", (from arg in arguments
                select arg.Key + ":=" + this.EscapeStringLiteral(arg.Value)).ToArray<string>()),
                ")"
            });
        }


        private string GetExpressionWithReplacedIdentifiersRecursively(string expression, string oldIdentifier, string newIdentifier, int tokensToSkipCount = 0)
        {
            SyntaxNodeOrToken[] array = VisualBasicExpressionTokenizer.GetRawRoslynSyntaxNodesAndTokens(expression).Skip(tokensToSkipCount).ToArray<SyntaxNodeOrToken>();
            if (!array.Any<SyntaxNodeOrToken>())
            {
                return expression;
            }
            for (int i = 0; i < array.Length; i++)
            {
                SyntaxNodeOrToken nodeOrToken = array[i];
                if (nodeOrToken.IsKind(SyntaxKind.DotToken))
                {
                    i++;
                }
                else
                {
                    IdentifierNameSyntax identifierNameSyntax = nodeOrToken.AsNode() as IdentifierNameSyntax;
                    if (identifierNameSyntax != null && (i + 2 >= array.Length || !(array[i + 2].AsNode() is ArgumentListSyntax)) && identifierNameSyntax.Identifier.Text.Equals(oldIdentifier, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            SyntaxTree syntaxTree = identifierNameSyntax.SyntaxTree;
                            string text = (syntaxTree != null) ? syntaxTree.GetText(default(CancellationToken)).ToString() : null;
                            if (string.IsNullOrEmpty(text))
                            {
                                return expression;
                            }
                            int num = text.IndexOf(expression);
                            expression = expression.Remove(nodeOrToken.Span.Start - num, nodeOrToken.Span.Length);
                            expression = expression.Insert(nodeOrToken.Span.Start - num, newIdentifier);
                            return this.GetExpressionWithReplacedIdentifiersRecursively(expression, oldIdentifier, newIdentifier, ++i);
                        }
                        catch (Exception exception)
                        {
                            exception.Trace(null);
                        }
                    }
                }
            }
            return expression;
        }

        private const string TempVarInit = "Dim _temp=";
    }
}
