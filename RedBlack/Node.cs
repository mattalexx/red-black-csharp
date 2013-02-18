using System;
using System.Collections.Generic;

namespace RedBlack
{
    internal class Node<T> : IComparable where T : ITreeObject
    {
        public static int LEFT = 0;
        public static int RIGHT = 1;
        Node<T>[] children = new Node<T>[2];
        public bool Red = true;
        Tree<T> Tree { get; set; }
        string Key { get { return Object.GetObjectStorageKey(); } }
        public T Object { get; set; }
        public Node<T> Parent { get; set; }
        public Node<T> Left { get { return children[LEFT]; } set { children[LEFT] = value; } }
        public Node<T> Right { get { return children[RIGHT]; } set { children[RIGHT] = value; } }
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

        public void Remove()
        {
            if (Left != null && Right != null)
            {
                Node<T> replacement = Right.GetLeast();
                Object = replacement.Object;
                replacement.Remove();
                return;
            }

            int childDirection = (Parent != null) ? ChildDirection : -1;
            Node<T> parent = Parent;
            Node<T> newMe = Left != null ? Left : Right;

            if (parent != null)
                DisconnectFromParent();
            else
                Tree.Root = null;

            if (newMe != null)
            {
                newMe.DisconnectFromParent();
                if (parent != null)
                    parent.SetChild(childDirection, newMe);
            }

            if (!Red)
            {
                if (newMe != null && newMe.Red)
                    newMe.Red = false;
                else if (newMe == null || !newMe.Red)
                    newMe.RemoveCase1();
            }
        }

        void RemoveCase1()
        {
            if (Parent != null)
                RemoveCase2();
        }

        void RemoveCase2()
        {
            if (Sibling.Red)
            {
                Parent.Red = true;
                Sibling.Red = false;
                Rotate(ChildDirection);
            }

            RemoveCase3();
        }

        void RemoveCase3()
        {
            if (!Parent.Red && !Sibling.Red && !Sibling.Left.Red && !Sibling.Right.Red)
            {
                Sibling.Red = true;
                Parent.RemoveCase1();
            }
            else
                RemoveCase4();
        }

        void RemoveCase4()
        {
            if (Parent.Red && !Sibling.Red && !Sibling.Left.Red && !Sibling.Right.Red)
            {
                Sibling.Red = true;
                Parent.Red = false;
            }
            else
                RemoveCase5();
        }

        void RemoveCase5()
        {
            if (!Sibling.Red)
            {
                int dir = ChildDirection;
                int otherDir = 1 - dir;

                if (!Sibling.GetChild(otherDir).Red && Sibling.GetChild(dir).Red)
                {
                    Sibling.Red = true;
                    Sibling.GetChild(dir).Red = false;
                    Rotate(otherDir);
                }
            }

            RemoveCase6();
        }

        void RemoveCase6()
        {
            Sibling.Red = Parent.Red;
            Parent.Red = false;

            int dir = ChildDirection;
            int otherDir = 1 - dir;

            Sibling.GetChild(otherDir).Red = false;
            Parent.Rotate(dir);
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

            // Set children
            SetChild(otherDir, newMeChild);
            newMe.SetChild(dir, this);
            if (parent != null)
                parent.SetChild(childDirection, newMe);
            else
                Tree.Root = newMe;
        }

        void DisconnectFromParent()
        {
            if (Parent != null)
                Parent.children[ChildDirection] = null;

            Parent = null;
        }

        void SetChild(int dir, Node<T> node)
        {
            // Set node's previous parent's child node to null
            if (node != null && node.Parent != null)
                throw new Exception("This node already has a parent");

            children[dir] = node;
            if (node != null)
                node.Parent = this;
        }
    }
}