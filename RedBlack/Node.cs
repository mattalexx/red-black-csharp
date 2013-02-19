using System;
using System.Collections.Generic;

namespace RedBlack
{
    internal class Node<T> : IComparable where T : ITreeObject
    {
        public static int LEFT = 0;
        public static int RIGHT = 1;
        public static int RED = 0;
        public static int BLACK = 1;
        public static int NRED = -1;
        public static int DBLACK = 2;
        Node<T>[] children = new Node<T>[2];
        public int color = RED;
        public bool Red
        {
            get { return color == RED; }
            set { color = value ? RED : BLACK; }
        }
        public int Color
        {
            get { return color; }
            set { color = value; }
        }
        Tree<T> Tree { get; set; }
        string Key
        {
            get { return Object.GetObjectStorageKey(); }
        }
        public T Object { get; set; }
        public Node<T> Parent { get; set; }
        public Node<T> Left
        {
            get { return children[LEFT]; }
            set { children[LEFT] = value; }
        }
        public Node<T> Right
        {
            get { return children[RIGHT]; }
            set { children[RIGHT] = value; }
        }
        public Node<T> Sibling
        {
            get
            {
                if (Parent == null)
                    return null;
                return Parent.GetChild(1 - ChildDirection);
            } 
        }
        public Node<T> GrandParent
        {
            get
            {
                if (Parent == null)
                    return null;
                return Parent.Parent;
            } 
        }
        public Node<T> Uncle
        {
            get
            {
                if (Parent == null)
                    return null;
                return Parent.Sibling;
            } 
        }

        public int ChildDirection
        {
            get
            {
                if (Parent == null)
                    throw new NullReferenceException();
                return this == Parent.Left ? LEFT : RIGHT;
            }
        }

        public Node(Tree<T> tree, T obj)
        {
            Tree = tree;
            Object = obj;
        }

        public Node<T> GetChild(int dir)
        {
            return children[dir];
        }

        public void Add(Node<T> node)
        {
            int result = CompareTo(node);

            if (result == 0)
                return;

            int dir = result == 1 ? LEFT : RIGHT;
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

            int dir = result == 1 ? LEFT : RIGHT;
            Node<T> child = GetChild(dir);

            if (child != null)
                return child.Find(key);

            return null;
        }

        public List<Node<T>> GetMyselfAndAncestors()
        {
            var nodes = new List<Node<T>>();

            nodes.Add(this);

            if (Parent != null)
                nodes.AddRange(Parent.GetMyselfAndAncestors());

            return nodes;
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

            foreach (int dir in new[]{LEFT, RIGHT})
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
            if (Parent == null || !Parent.Red)
                return;

            if (Uncle != null && Uncle.Red)
            {
                Parent.Red      = false;
                Uncle.Red       = false;
                GrandParent.Red = true;

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
            if (ChildDirection == RIGHT && Parent.ChildDirection == LEFT)
            {
                Parent.Rotate(LEFT);
                return Left;
            }

            if (ChildDirection == LEFT && Parent.ChildDirection == RIGHT)
            {
                Parent.Rotate(RIGHT);
                return Right;
            }

            return this;
        }

        /// <summary>
        /// Called from lowest of the three involved nodes
        /// </summary>
        void DoSecondRotation()
        {
            Parent.Red = false;
            GrandParent.Red = true;

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
            if (Left == null || Right == null)
            {
                Node<T> child = Left != null ? Left : Right;

                FixRemove(this);

                ReplaceWith(child);

                return;
            }

            Node<T> next = Right.GetLeast();
            Object = next.Object;

            FixRemove(next);

            next.ReplaceWith(next.Right);
        }

        void FixRemove(Node<T> node)
        {
            if (node.Red)
                return;

            if (node.Left != null || node.Right != null)
            {
                int dir = node.Left == null ? RIGHT : LEFT;
                node.GetChild(dir).Color = BLACK;
            }
            else
                BubbleUp(node.Parent);
        }

        void BubbleUp(Node<T> parent)
        {
            if (parent == null)
                return;

            parent.Color++;
            parent.Left.Color--;
            parent.Right.Color--;

            Node<T> child = parent.Left;
            if (child.Color == NRED)
            {
                FixNegativeRed(child);
                return;
            }
            else if (child.Color == RED)
            {
                if (child.Left != null && child.Left.Color == RED)
                {
                    FixDoubleRed(child.Left);
                    return;
                }
                if (child.Right != null && child.Right.Color == RED)
                {
                    FixDoubleRed(child.Right);
                    return;
                }
            }

            child = parent.Right;
            if (child.Color == NRED)
            {
                FixNegativeRed(child);
                return;
            }
            else if (child.Color == RED)
            {
                if (child.Left != null && child.Left.Color == RED)
                {
                    FixDoubleRed(child.Left);
                    return;
                }
                if (child.Right != null && child.Right.Color == RED)
                {
                    FixDoubleRed(child.Right);
                    return;
                }
            }

            if (parent.Color == DBLACK)
            {
                if (parent.Parent == null)
                    parent.Color = BLACK;
                else
                    BubbleUp(parent.Parent);
            }
        }

        void FixNegativeRed(Node<T> negRed)
        {
            Node<T> n1, n2, n3, n4, t1, t2, t3, child;
            Node<T> parent = negRed.Parent;

            if (parent.Left == negRed)
            {
                n1 = negRed.Left;
                n2 = negRed;
                n3 = negRed.Right;
                n4 = parent;
                t1 = n3.Left;
                t2 = n3.Right;
                t3 = n4.Right;
                n1.Color = RED;
                n2.Color = BLACK;
                n4.Color = BLACK;
                n2.SetChild(RIGHT, t1);
                T temp = n4.Object;
                n4.Object = n3.Object;
                n3.Object = temp;
                n3.SetChild(LEFT, t2);
                n3.SetChild(RIGHT, t3);
                n4.SetChild(RIGHT, n3);
                child = n1;
            }
            else
            {
                n4 = negRed.Right;
                n3 = negRed;
                n2 = negRed.Left;
                n1 = parent;
                t3 = n2.Right;
                t2 = n2.Left;
                t1 = n1.Left;
                n4.Color = RED;
                n3.Color = BLACK;
                n1.Color = BLACK;
                n3.SetChild(LEFT, t3);
                T temp = n1.Object;
                n1.Object = n2.Object;
                n2.Object = temp;
                n2.SetChild(RIGHT, t2);
                n2.SetChild(LEFT, t1);
                n1.SetChild(LEFT, n2);
                child = n4;
            }

            if (child.Left != null && child.Left.Color == RED)
            {
                FixDoubleRed(child.Left);
                return;
            }
            if (child.Right != null && child.Right.Color == RED)
                FixDoubleRed(child.Right);

        }

        void FixDoubleRed(Node<T> child)
        {
            Node<T> parent = child.Parent;
            Node<T> grandParent = child.GrandParent;

            if (grandParent == null)
            {
                parent.Color = BLACK;
                return;
            }

            Node<T> n1, n2, n3, t1, t2, t3, t4;

            if (parent == grandParent.Left)
            {
                n3 = grandParent;
                t4 = grandParent.Right;
                if (child == parent.Left)
                {
                    n1 = child;
                    n2 = parent;
                    t1 = child.Left;
                    t2 = child.Right;
                    t3 = parent.Right;
                }
                else
                {
                    n1 = parent;
                    n2 = child;
                    t1 = parent.Left;
                    t2 = child.Left;
                    t3 = child.Right;
                }
            }
            else
            {
                n1 = grandParent;
                t1 = grandParent.Left;
                if (child == parent.Left)
                {
                    n2 = child;
                    n3 = parent;
                    t2 = child.Left;
                    t3 = child.Right;
                    t4 = parent.Right;
                }
                else
                {
                    n2 = parent;
                    n3 = child;
                    t2 = parent.Left;
                    t3 = child.Left;
                    t4 = child.Right;
                }
            }

            if (grandParent == Tree.Root)
            {
                Tree.Root = n2;
                n2.Parent = null;
            }
            else
                grandParent.ReplaceWith(n2);

            n1.SetChild(LEFT, t1);
            n1.SetChild(RIGHT, t2);
            n2.SetChild(LEFT, n1);
            n2.SetChild(RIGHT, n3);
            n3.SetChild(LEFT, t3);
            n3.SetChild(RIGHT, t4);
            n2.Color = grandParent.Color - 1;
            n1.Color = BLACK;
            n3.Color = BLACK;

            if (n2 == Tree.Root)
            {
                Tree.Root.Color = BLACK;
            }
            else if (n2.Color == RED && n2.Parent.Color == RED)
            {
                FixDoubleRed(n2);
            }
        }

        Node<T> GetLeast()
        {
            if (Left == null)
                return this;

            return Left.GetLeast();
        }

        Node<T> GetGreatest()
        {
            if (Right == null)
                return this;

            return Right.GetGreatest();
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
                Parent.children[ChildDirection] = null;

            Parent = null;
        }

        void SetChild(int dir, Node<T> node)
        {
            int otherDir = 1 - dir;

            // If the passed node exists as the other child, disconnect it from there
            if (children[otherDir] == node && node != null)
                children[otherDir].DisconnectFromParent();

            // If we have a node in the target spot, disconnect it
            if (children[dir] != null)
                children[dir].DisconnectFromParent();

            // Set the node
            children[dir] = node;

            if (node == null)
                return;

            // Set node's previous parent's child node to null
            if (node.Parent != null)
                node.DisconnectFromParent();

            node.Parent = this;
        }
    }
}