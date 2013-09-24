using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RedBlack
{
    public class Tree<T> : ITree<T>, IEnumerable<T> where T : ITreeObject
    {
        internal Node<T> Root { get; set; }

        public void Add(T obj)
        {
            var node = new Node<T>(this, obj);

            if (Root == null)
                Root = node;
            else
                Root.Add(node);

            Root.IsRed = false;
        }

        public T Find(string key)
        {
            if (Root == null)
                return default(T);

            Node<T> node = Root.Find(key);

            if (node != null)
                return node.Object;

            return default(T);
        }

        public void Remove(string key)
        {
            if (Root == null)
                return;

            Node<T> node = Root.Find(key);

            if (node != null)
                node.Remove();
        }

        public T GetTop()
        {
            return Root != null ? Root.Object : default(T);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetNodes().Select(t => t.Object).GetEnumerator();
        }

        public string GetDotCode()
        {
            var lines = new List<string> {"digraph BST {"};

            if (Root != null)
                Root.GetDotCode(ref lines);
            else
                lines.Add("    null0 [shape=point];");

            lines.Add("}");

            string dot = String.Join("\r\n", lines);

            return dot;
        }

        public void PrintAllItems()
        {
            foreach (T t in this)
                Console.WriteLine(t.GetObjectStorageKey());
        }

        internal Node<T> GetRootNode()
        {
            return Root;
        }

        internal IEnumerable<Node<T>> GetNodes()
        {
            return Root == null ? Enumerable.Empty<Node<T>>() : Root.GetMyselfAndDescendants();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}