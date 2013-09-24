using System;
using System.Collections.Generic;

namespace RedBlack
{
    internal class Node<T> : IComparable where T : ITreeObject
    {
        const int Left = 0;
        const int Right = 1;
        const int Red = 0;
        const int Black = 1;
        const int Nred = -1;
        const int Dblack = 2;
        readonly Node<T>[] _children = new Node<T>[2];
        int _color = Red;
        public bool IsRed
        {
            get { return _color == Red; }
            set { _color = value ? Red : Black; }
        }
        public int Color
        {
            get { return _color; }
            set { _color = value; }
        }
        Tree<T> Tree { get; set; }
        string Key
        {
            get { return Object.GetObjectStorageKey(); }
        }
        public T Object { get; set; }
        public Node<T> Parent { get; set; }
        public Node<T> LeftNode
        {
            get { return _children[Left]; }
            set { _children[Left] = value; }
        }
        public Node<T> RightNode
        {
            get { return _children[Right]; }
            set { _children[Right] = value; }
        }
        public Node<T> Sibling
        {
            get
            {
                return Parent == null ? null : Parent.GetChild(1 - ChildDirection);
            }
        }
        public Node<T> GrandParent
        {
            get
            {
                return Parent == null ? null : Parent.Parent;
            }
        }
        public Node<T> Uncle
        {
            get
            {
                return Parent == null ? null : Parent.Sibling;
            }
        }

        public int ChildDirection
        {
            get
            {
                if (Parent == null)
                    throw new NullReferenceException();
                return this == Parent.LeftNode ? Left : Right;
            }
        }

        public Node(Tree<T> tree, T obj)
        {
            Tree = tree;
            Object = obj;
        }

        public Node<T> GetChild(int dir)
        {
            return _children[dir];
        }

        public void Add(Node<T> node)
        {
            int result = CompareTo(node);

            if (result == 0)
                return;

            int dir = result == 1 ? Left : Right;
            Node<T> child = GetChild(dir);

            if (child != null)
            {
                child.Add(node);
                return;
            }

            SetChild(dir, node);

            node.FixInsert();
        }

        public Node<T> Find(string key)
        {
            int result = Key.CompareTo(key);

            if (result == 0)
                return this;

            int dir = result == 1 ? Left : Right;
            Node<T> child = GetChild(dir);

            return child != null ? child.Find(key) : null;
        }

        public List<Node<T>> GetMyselfAndAncestors()
        {
            var nodes = new List<Node<T>> {this};

            if (Parent != null)
                nodes.AddRange(Parent.GetMyselfAndAncestors());

            return nodes;
        }

        public List<Node<T>> GetMyselfAndDescendants()
        {
            var nodes = new List<Node<T>>();

            if (LeftNode != null)
                nodes.AddRange(LeftNode.GetMyselfAndDescendants());

            nodes.Add(this);

            if (RightNode != null)
                nodes.AddRange(RightNode.GetMyselfAndDescendants());

            return nodes;
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
            string color = IsRed ? "red" : "black";
            lines.Add(String.Format("    {0} [color=\"{1}\" fontcolor=\"{2}\"];", Key, color, color));

            foreach (int dir in new[]{Left, Right})
            {
                if (GetChild(dir) != null)
                {
                    lines.Add(String.Format("    {0} -> {1};", Key, GetChild(dir).Key));
                    GetChild(dir).GetDotCode(ref lines);
                }
                else
                {
                    int n = lines.Count;

                    lines.Add(String.Format("    null{0} [shape=point];", n));
                    lines.Add(String.Format("    {0} -> null{1};", Key, n));
                }
            }
        }

        void FixInsert()
        {
            if (Parent == null || !Parent.IsRed)
                return;

            if (Uncle != null && Uncle.IsRed)
            {
                Parent.IsRed      = false;
                Uncle.IsRed       = false;
                GrandParent.IsRed = true;

                GrandParent.FixInsert();
            }
            else
                DoFirstRotation().DoSecondRotation();
        }

        /// <summary>
        /// Called from lowest of the three involved nodes
        /// </summary>
        /// <returns></returns>
        Node<T> DoFirstRotation()
        {
            if (ChildDirection == Right && Parent.ChildDirection == Left)
            {
                Parent.Rotate(Left);
                return LeftNode;
            }

            if (ChildDirection == Left && Parent.ChildDirection == Right)
            {
                Parent.Rotate(Right);
                return RightNode;
            }

            return this;
        }

        /// <summary>
        /// Called from lowest of the three involved nodes
        /// </summary>
        void DoSecondRotation()
        {
            Parent.IsRed = false;
            GrandParent.IsRed = true;

            GrandParent.Rotate(1 - ChildDirection);
        }

        /// <summary>
        /// Called from node to be usurped
        /// </summary>
        /// <param name="dir"></param>
        void Rotate(int dir)
        {
            int otherDir = 1 - dir;
            int childDirection = (Parent != null) ? ChildDirection : -1;
            Node<T> parent = Parent;
            Node<T> newMe = GetChild(otherDir);
            Node<T> newMeChild = newMe.GetChild(dir);

            // Disconnect nodes from their parents
            DisconnectFromParent();
            newMe.DisconnectFromParent();
            if (newMeChild != null)
                newMeChild.DisconnectFromParent();

            // Set new children
            SetChild(otherDir, newMeChild);
            newMe.SetChild(dir, this);
            if (parent != null)
                parent.SetChild(childDirection, newMe);
            else
                Tree.Root = newMe;
        }

        public void Remove()
        {
            if (LeftNode == null || RightNode == null)
            {
                Node<T> child = LeftNode ?? RightNode;

                FixRemove(this);

                ReplaceWith(child);

                return;
            }

            Node<T> next = RightNode.GetLeast();
            Object = next.Object;

            FixRemove(next);

            next.ReplaceWith(next.RightNode);
        }

        void FixRemove(Node<T> node)
        {
            if (node.IsRed)
                return;

            if (node.LeftNode != null || node.RightNode != null)
            {
                int dir = node.LeftNode == null ? Right : Left;
                node.GetChild(dir).Color = Black;
            }
            else
                BubbleUp(node.Parent);
        }

        void BubbleUp(Node<T> parent)
        {
            if (parent == null)
                return;

            parent.Color++;
            parent.LeftNode.Color--;
            parent.RightNode.Color--;

            Node<T> child = parent.LeftNode;
            if (child.Color == Nred)
            {
                FixNegativeRed(child);
                return;
            }
            if (child.Color == Red)
            {
                if (child.LeftNode != null && child.LeftNode.Color == Red)
                {
                    FixDoubleRed(child.LeftNode);
                    return;
                }
                if (child.RightNode != null && child.RightNode.Color == Red)
                {
                    FixDoubleRed(child.RightNode);
                    return;
                }
            }

            child = parent.RightNode;
            if (child.Color == Nred)
            {
                FixNegativeRed(child);
                return;
            }
            if (child.Color == Red)
            {
                if (child.LeftNode != null && child.LeftNode.Color == Red)
                {
                    FixDoubleRed(child.LeftNode);
                    return;
                }
                if (child.RightNode != null && child.RightNode.Color == Red)
                {
                    FixDoubleRed(child.RightNode);
                    return;
                }
            }

            if (parent.Color == Dblack)
            {
                if (parent.Parent == null)
                    parent.Color = Black;
                else
                    BubbleUp(parent.Parent);
            }
        }

        void FixNegativeRed(Node<T> negRed)
        {
            Node<T> n1, n2, n3, n4, t1, t2, t3, child;
            Node<T> parent = negRed.Parent;

            if (parent.LeftNode == negRed)
            {
                n1 = negRed.LeftNode;
                n2 = negRed;
                n3 = negRed.RightNode;
                n4 = parent;
                t1 = n3.LeftNode;
                t2 = n3.RightNode;
                t3 = n4.RightNode;
                n1.Color = Red;
                n2.Color = Black;
                n4.Color = Black;
                n2.SetChild(Right, t1);
                T temp = n4.Object;
                n4.Object = n3.Object;
                n3.Object = temp;
                n3.SetChild(Left, t2);
                n3.SetChild(Right, t3);
                n4.SetChild(Right, n3);
                child = n1;
            }
            else
            {
                n4 = negRed.RightNode;
                n3 = negRed;
                n2 = negRed.LeftNode;
                n1 = parent;
                t3 = n2.RightNode;
                t2 = n2.LeftNode;
                t1 = n1.LeftNode;
                n4.Color = Red;
                n3.Color = Black;
                n1.Color = Black;
                n3.SetChild(Left, t3);
                T temp = n1.Object;
                n1.Object = n2.Object;
                n2.Object = temp;
                n2.SetChild(Right, t2);
                n2.SetChild(Left, t1);
                n1.SetChild(Left, n2);
                child = n4;
            }

            if (child.LeftNode != null && child.LeftNode.Color == Red)
            {
                FixDoubleRed(child.LeftNode);
                return;
            }
            if (child.RightNode != null && child.RightNode.Color == Red)
                FixDoubleRed(child.RightNode);
        }

        void FixDoubleRed(Node<T> child)
        {
            Node<T> parent = child.Parent;
            Node<T> grandParent = child.GrandParent;

            if (grandParent == null)
            {
                parent.Color = Black;
                return;
            }

            Node<T> n1, n2, n3, t1, t2, t3, t4;

            if (parent == grandParent.LeftNode)
            {
                n3 = grandParent;
                t4 = grandParent.RightNode;
                if (child == parent.LeftNode)
                {
                    n1 = child;
                    n2 = parent;
                    t1 = child.LeftNode;
                    t2 = child.RightNode;
                    t3 = parent.RightNode;
                }
                else
                {
                    n1 = parent;
                    n2 = child;
                    t1 = parent.LeftNode;
                    t2 = child.LeftNode;
                    t3 = child.RightNode;
                }
            }
            else
            {
                n1 = grandParent;
                t1 = grandParent.LeftNode;
                if (child == parent.LeftNode)
                {
                    n2 = child;
                    n3 = parent;
                    t2 = child.LeftNode;
                    t3 = child.RightNode;
                    t4 = parent.RightNode;
                }
                else
                {
                    n2 = parent;
                    n3 = child;
                    t2 = parent.LeftNode;
                    t3 = child.LeftNode;
                    t4 = child.RightNode;
                }
            }

            if (grandParent == Tree.Root)
            {
                Tree.Root = n2;
                n2.Parent = null;
            }
            else
                grandParent.ReplaceWith(n2);

            n1.SetChild(Left, t1);
            n1.SetChild(Right, t2);
            n2.SetChild(Left, n1);
            n2.SetChild(Right, n3);
            n3.SetChild(Left, t3);
            n3.SetChild(Right, t4);
            n2.Color = grandParent.Color - 1;
            n1.Color = Black;
            n3.Color = Black;

            if (n2 == Tree.Root)
                Tree.Root.Color = Black;
            else if (n2.Color == Red && n2.Parent.Color == Red)
                FixDoubleRed(n2);
        }

        Node<T> GetLeast()
        {
            return LeftNode == null ? this : LeftNode.GetLeast();
        }

        void ReplaceWith(Node<T> node)
        {
            int childDirection = (Parent != null) ? ChildDirection : -1;
            Node<T> parent = Parent;

            // Disconnect nodes from their parents
            if (node != null)
                node.DisconnectFromParent();
            if (parent != null)
                DisconnectFromParent();
            else
                Tree.Root = null;

            // Set new children
            if (parent != null)
                parent.SetChild(childDirection, node);
            else
                Tree.Root = node;
        }

        void DisconnectFromParent()
        {
            if (Parent != null)
                Parent._children[ChildDirection] = null;

            Parent = null;
        }

        void SetChild(int dir, Node<T> node)
        {
            int otherDir = 1 - dir;

            // If the passed node exists as the other child, disconnect it from there
            if (_children[otherDir] == node && node != null)
                _children[otherDir].DisconnectFromParent();

            // If we have a node in the target spot, disconnect it
            if (_children[dir] != null)
                _children[dir].DisconnectFromParent();

            // Set the node
            _children[dir] = node;

            if (node == null)
                return;

            // Set node's previous parent's child node to null
            if (node.Parent != null)
                node.DisconnectFromParent();

            node.Parent = this;
        }
    }
}