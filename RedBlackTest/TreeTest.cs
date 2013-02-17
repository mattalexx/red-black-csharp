using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RedBlack;
using System.Security.Cryptography;

namespace RedBlackTest
{
    [TestFixture]
    public class TreeTest
    {
        [Test]
        [TestCaseSource("GetTrees")]
        public void TestOrdering(Tree<Student> tree)
        {
            IEnumerable<Node<Student>> nodes =
                from node in tree.GetNodes()
                where (node.Left != null && node.CompareTo(node.Left.GetMyselfAndDescendants().Max()) != 1)
                    || (node.Right != null && node.CompareTo(node.Right.GetMyselfAndDescendants().Min()) != -1)
                select node;

            Assert.IsEmpty(nodes, "Nodes left of a node should be lesser and nodes right of a node should be greater");
        }

        [Test]
        [TestCaseSource("GetTrees")]
        public void TestNoDuplicates(Tree<Student> tree)
        {
        }

        [Test]
        [TestCaseSource("GetTrees")]
        public void TestLinks(Tree<Student> tree)
        {
            Node<Student> root = tree.GetRootNode();

            Assert.Null(root.Parent);
            TestNodeLinks(root);
        }

        void TestNodeLinks(Node<Student> node)
        {
            foreach (int dir in new[] { 0, 1 })
            {
                if (node.children[dir] == null)
                    continue;

                Assert.AreSame(node, node.children[dir].Parent);
                TestNodeLinks(node.children[dir]);
            }
        }

        [Test]
        [TestCaseSource("GetTrees")]
        public void TestRootBlack(Tree<Student> tree)
        {
            Assert.False(tree.GetRootNode().Red, "Root node must be black");
        }

        [Test]
        [TestCaseSource("GetTrees")]
        public void TestNoRedChildrenOnRedNodes(Tree<Student> tree)
        {
            IEnumerable<Node<Student>> nodes = 
                from node in tree.GetNodes()
                where node.Red && ((node.Left != null && node.Left.Red) || (node.Right != null && node.Right.Red))
                select node;

            Assert.IsEmpty(nodes, "Red nodes cannot have red children");
        }

        [Test]
        [TestCaseSource("GetTrees")]
        public void TestBlackDepthOfLeaves(Tree<Student> tree)
        {
            IEnumerable<int> depths =
                from node in tree.GetNodes()
                where node.Left == null || node.Right == null
                select node.GetMyselfAndAncestors().Where(n => !n.Red).Count();

            Assert.NotNull(depths);
            int distinctDepthCount = depths.Distinct().Count();

            Assert.AreEqual(1, distinctDepthCount, "Every leaf must have the same black depth");
        }

        [Test]
        [TestCaseSource("GetLists")]
        public void TestEnumeration(IEnumerable<string> values)
        {
            Tree<Student> tree = new Tree<Student>();
            foreach (string value in values)
                tree.Add(new Student(value));

            List<string> result   = (from student in tree select student.GetObjectStorageKey()).ToList();
            List<string> expected = (from name in values orderby name select name).Distinct().ToList();

            Assert.AreEqual(expected, result);
        }

        protected IEnumerable<Tree<Student>> GetTrees()
        {
            var trees = new List<Tree<Student>>();

            foreach (IEnumerable<string> list in GetLists())
                trees.Add(GetTree(list));

            return trees;
        }

        protected List<IEnumerable<string>> GetLists()
        {
            var lists = new List<IEnumerable<string>>();

            lists.Add(new[]{ "John", "Marie", "Xavier", "Adam", "Betty" });
            lists.Add(new[]{ "04", "01", "03", "02", "06", "05" });
            lists.Add(new[]{ "C", "A", "B" });
            lists.Add(new[]{ "363", "168", "610", "381", "236", "348", "833" });
            lists.Add(new[]{ "08", "07", "06", "05", "04", "03", "02", "01" });

            var list = GetBigList().ToList();
            //int keep = 2;
            //list.RemoveRange(keep, list.Count() - 1);
            lists.Add(list);

            return lists;
        }

        protected IEnumerable<string> GetBigList()
        {
            var src = new List<Tuple<string, string>>();

            foreach (int n in from n in Enumerable.Range(1, 1000) select n)
                src.Add(new Tuple<string, string>(n.ToString(), CalculateMD5Hash(n.ToString())));

            return from item in src orderby item.Item2 select item.Item1;
        }

        protected Tree<Student> GetTree(IEnumerable<string> values)
        {
            Tree<Student> tree = new Tree<Student>();
            foreach (string value in values)
                tree.Add(new Student(value));

            return tree;
        }

        public static string CalculateMD5Hash(string input)
        {
            var md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString();
        }
    }
}