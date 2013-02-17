using System;
using RedBlack;
using System.Collections.Generic;
using System.Linq;

namespace RedBlack
{
    internal class Node<T> : IComparable where T : ITreeObject
    {
        public bool Red = true;
        public Tree<T> Tree { get; set; }
        public T Object { get; set; }
        internal string Key { get { return Object.GetObjectStorageKey(); } }
        internal Node<T> Parent { get; set; }
        internal Node<T>[] children = new Node<T>[2];
        internal Node<T> Left { get { return children[0]; } set { children[0] = value; } }
        internal Node<T> Right { get { return children[1]; } set { children[1] = value; } }
        internal Node<T> Sibling
        {
            get
            {
                if (Parent == null)
                    return null;
                if (this == Parent.children[0])
                    return Parent.children[1];
                else
                    return Parent.children[0];
            } 
        }
        internal Node<T> GrandParent
        {
            get
            {
                if (Parent == null)
                    return null;
                return Parent.Parent;
            } 
        }
        internal Node<T> Uncle
        {
            get
            {
                if (Parent == null)
                    return null;
                return Parent.Sibling;
            } 
        }

        internal int ChildDirection
        {
            get
            {
                if (Parent == null)
                    throw new NullReferenceException();

                return this == Parent.children[0] ? 0 : 1;
            }
        }

        public Node(Tree<T> tree, T obj, Node<T> parent = null)
        {
            Tree   = tree;
            Object = obj;
            Parent = parent;
        }

        public void Add(T obj)
        {
            int result = Key.CompareTo(obj.GetObjectStorageKey());

            if (result == 0)
                return;

            int dir = result == 1 ? 0 : 1;
            int otherDir = 1 - dir;
            Node<T> child = children[dir];

            if (child != null)
            {
                child.Add(obj);
                return;
            }

            var node = new Node<T>(Tree, obj);
            SetChild(dir, node);

            node.FixInsert();
        }

        /// <summary>
        /// Called from inserted node or equivalent place in the macro structure
        /// </summary>
        public void FixInsert()
        {
            if (Parent == null || !Parent.Red)
                return;

            // Case 3: Uncle is red so balance by setting colors and checking grandparent
            if (Uncle != null && Uncle.Red)
            {
                Parent.Red      = false;
                Uncle.Red       = false;
                GrandParent.Red = true;

                GrandParent.FixInsert();
            }

            // Case 4: Uncle is black (null is black) so we need to rotate
            else
            {
                Node<T> lowest = this;

                if (ChildDirection == 1 && Parent.ChildDirection == 0)
                {
                    Parent.Rotate(0);
                    lowest = Left;
                }
                else if (ChildDirection == 0 && Parent.ChildDirection == 1)
                {
                    Parent.Rotate(1);
                    lowest = Right;
                }

                lowest.Parent.Red      = false;
                lowest.GrandParent.Red = true;

                lowest.GrandParent.Rotate(1 - lowest.ChildDirection);
            }
        }

        /// <summary>
        /// Called from parent of inserted node or equivalent place in the macro structure
        /// </summary>
        /// <param name="dir"></param>
        public void Rotate(int dir)
        {
            int otherDir       = 1 - dir;
            Node<T> newMe      = children[otherDir];
            Node<T> parent     = Parent;
            Node<T> newMeChild = newMe.children[dir];

            int childDirection = 0;
            if (parent != null)
                childDirection = this.ChildDirection;

            this.Disconnect();
            newMe.Disconnect();

            if (newMeChild != null)
                newMeChild.Disconnect();

            this.SetChild(otherDir, newMeChild);

            if (parent != null)
                parent.SetChild(childDirection, newMe);
            else
            {
                newMe.Parent = null;
                Tree.Root = newMe;
            }

            newMe.SetChild(dir, this);
        }

        public Node<T> Disconnect()
        {
            if (this.Parent != null)
                this.Parent.children[ChildDirection] = null;

            this.Parent = null;

            return this;
        }

        public Node<T> SetChild(int dir, Node<T> node)
        {
            // Set node's previous parent's child node to null
            if (node != null && node.Parent != null)
                throw new Exception("This node already has a parent");

            children[dir] = node;
            if (node != null)
                node.Parent = this;

            return this;
        }

        public List<Node<T>> GetMyselfAndAncestors()
        {
            var ancestors = new List<Node<T>>();

            ancestors.Add(this);

            if (Parent != null)
                ancestors.AddRange(Parent.GetMyselfAndAncestors());

            return ancestors;
        }

        public List<Node<T>> GetMyselfAndDescendants()
        {
            var nodes = new List<Node<T>>();

            if (Left != null)
                nodes.AddRange(Left.GetMyselfAndDescendants());

            nodes.Add(this);

            if (Right != null)
                nodes.AddRange(Right.GetMyselfAndDescendants());

            return nodes;
        }

        public IEnumerable<Node<T>> Traverse()
        {
            foreach (Node<T> node in GetMyselfAndDescendants())
                yield return node;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            var that = obj as Node<T>;

            if (that == null)
                throw new ArgumentException("Only compareable to other Node objects");

            return Key.CompareTo(that.Key);
        }

        public void GetDotCode(ref List<string> lines)
        {
            string color = Red ? "red" : "black";
            lines.Add(String.Format("    {0} [color=\"{1}\" fontcolor=\"{2}\"];", Key, color, color));

            foreach (int dir in new[]{0, 1})
            {
                if (children[dir] != null)
                {
                    lines.Add(String.Format("    {0} -> {1};", Key, children[dir].Key));
                    children[dir].GetDotCode(ref lines);
                }
                else
                {
                    int n = lines.Count;

                    lines.Add(String.Format("    null{0} [shape=point];", n));
                    lines.Add(String.Format("    {0} -> null{1};", Key, n));
                }
            }
        }
    }
}