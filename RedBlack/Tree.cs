using System;
using System.Collections;
using System.Collections.Generic;

namespace RedBlack
{
    public class Tree<T> : ITree<T>, IEnumerable<T> where T : ITreeObject
    {
        internal Node<T> Root { get; set; }

        public void Add(T obj)
        {
            if (Root != null)
                Root.Add(obj);
            else
                Root = new Node<T>(this, obj);

            Root.Red = false;
        }

        public T GetTop()
        {
            if (Root != null)
                return Root.Object;

            return default(T);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (Node<T> t in Root.GetMyselfAndDescendants())
                yield return t.Object;
        }

        internal Node<T> GetRootNode()
        {
            return Root;
        }

        internal IEnumerable<Node<T>> GetNodes()
        {
            return Root.GetMyselfAndDescendants();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string GetDotCode()
        {
            var lines = new List<string>();

            lines.Add("digraph BST {");
            Root.GetDotCode(ref lines);
            lines.Add("}");

            string dot = String.Join(" ", lines);

            return dot;
        }
    }
}