using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.CodeCompletion
{
    using System.Collections.Generic;


    public class ExpressionNode
    {
        public ExpressionNode()
        {
            this.Nodes = new SortedSet<ExpressionNode>(new ExpressionNodeComparer());
        }


        public string Description { get; set; }


        public string ItemType { get; set; }

        public string Name { get; set; }

        public ExpressionNode Parent { get; set; }

        public SortedSet<ExpressionNode> Nodes { get; private set; }

        public string Path
        {
            get
            {
                return (this.Parent == null) ? this.Name : this.Parent.Path + "." + this.Name;
            }
        }

        public void Add(ExpressionNode node)
        {
            this.Nodes.Add(node);
        }


        public ExpressionNode SearchForNode(string path)
        {
            return ExpressionNode.SearchForNode(this, path);
        }

        public static ExpressionNode SearchForNode(ExpressionNode target, string path)
        {
            ExpressionNode match = SearchForNode(target, path, false, false);
            return match ?? target;
        }

        public static ExpressionNode SearchForNode(ExpressionNode target, string path,
            bool isNamespace, bool createIfMissing)
        {
            string[] names = path.Split('.');
            string subPath = "";

            if (names.Length > 0 && path.Length > names[0].Length)
            {
                subPath = path.Substring(names[0].Length, path.Length - names[0].Length);
            }

            if (subPath.StartsWith("."))
            {
                subPath = subPath.Substring(1);
            }

            List<ExpressionNode> matches = (from x in target.Nodes
                                            where
x.Name.Equals(names[0], StringComparison.OrdinalIgnoreCase)
                                            select x).ToList();

            if (matches.Count == 0)
            {
                if (!createIfMissing) return null;

                ExpressionNode subNode = new ExpressionNode
                {
                    Name = names[0],
                    ItemType = isNamespace || names.Length > 1 ? "namespace" : "class",
                    Parent = target,
                    Description = isNamespace || names.Length > 1 ? string.Format("namespace {0}",
                        names[0]) : string.Format("class {0}", names[0])
                };

                target.Nodes.Add(subNode);

                if (subPath.Trim() != "")
                {
                    return SearchForNode(subNode, subPath, isNamespace, true);
                }

                return subNode;
            }

            if (subPath.Trim() != "")
            {
                return SearchForNode(matches[0], subPath, isNamespace, createIfMissing);
            }

            return matches[0];
        }

        public static List<ExpressionNode> SubsetAutoCompletionList(ExpressionNode rootNode,
            string filter)
        {
            string parentPath = "";
            string searchTerm = filter;

            if (filter.Contains("."))
            {
                parentPath = filter.Substring(0, filter.LastIndexOf("."));
                searchTerm = filter.Substring(parentPath.Length + 1);
            }

            ExpressionNode targetNode =
                parentPath != "" ? SearchForNode(rootNode, parentPath) : rootNode;
            List<ExpressionNode> matches = new List<ExpressionNode>();

            foreach (ExpressionNode subNode in targetNode.Nodes)
            {
                if (subNode.Name.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(subNode);
                }
            }

            return matches;
        }
    }
}
