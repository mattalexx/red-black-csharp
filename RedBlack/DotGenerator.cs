using System;
using System.Collections.Generic;

namespace RedBlack
{
    class DotGenerator
    {
        Tree<T> tree;

        public DotGenerator(Tree<T> tree)
        {
            this.tree = tree;
        }

        // ------ DOT methods --------------- //

        public string GetDot()
        {
            List<string> lines = new List<string>();

            lines.Add("digraph BST {");
            Root.GetDot(ref lines);
            lines.Add("}");

            string dot = String.Join("\n", lines);

            return dot;
        }

        // ------ DOT methods --------------- //

        public void GetNodeDot(ref List<string> lines)
        {
            string color = Red ? "red" : "black";
            lines.Add(String.Format("    {0} [color=\"{1}\" fontcolor=\"{2}\"];", Object.GetObjectStorageKey(), color, color));

            if (Left != null)
            {
                lines.Add(String.Format("    {0} -> {1};", Object.GetObjectStorageKey(), Left.Object.GetObjectStorageKey()));
                Left.GetDot(ref lines);
            }
            else
                GetNullDot(ref lines);

            if (Right != null)
            {
                lines.Add(String.Format("    {0} -> {1};", Object.GetObjectStorageKey(), Right.Object.GetObjectStorageKey()));
                Right.GetDot(ref lines);
            }
            else
                GetNullDot(ref lines);
        }

        public void GetNullDot(ref List<string> lines)
        {
            int n = lines.Count;

            lines.Add(String.Format("    null{0} [shape=point];", n));
            lines.Add(String.Format("    {0} -> null{1};", Object.GetObjectStorageKey(), n));
        }
    }
}
