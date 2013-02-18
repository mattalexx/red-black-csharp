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
            var node = new Node<T>(this, obj);

            if (Root == null)
                Root = node;
            else
                Root.Add(node);

            Root.Red = false;
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
            if (Root != null)
                return Root.Object;

            return default(T);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (Node<T> t in GetNodes())
                yield return t.Object;
        }

        public string GetDotCode()
        {
            var lines = new List<string>();

            lines.Add("digraph BST {");
            Root.GetDotCode(ref lines);
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
            return Root.GetMyselfAndDescendants();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}