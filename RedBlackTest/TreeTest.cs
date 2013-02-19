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
        public void TestLinks(Tree<Student> tree)
        {
            Node<Student> root = tree.GetRootNode();

            Assert.Null(root.Parent);
            TestNodeLinks(root);
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
            Tree<Student> tree = GetTree(GetStudentList(values));

            List<string> result = (from student in tree select student.GetObjectStorageKey()).ToList();
            List<string> expected = (from name in values orderby name select name).Distinct().ToList();

            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCaseSource("GetLists")]
        public void TestFind(IEnumerable<string> values)
        {
            IEnumerable<Student> students = GetStudentList(values);
            Tree<Student> tree = GetTree(students);

            foreach (Student student in students)
                Assert.AreSame(student, tree.Find(student.GetObjectStorageKey()));
        }

        [Test]
        [TestCaseSource("GetLists")]
        public void TestFindNonExistent(IEnumerable<string> values)
        {
            IEnumerable<Student> students = GetStudentList(values);
            Tree<Student> tree = GetTree(students);

            string nope = "a";
            while (values.Contains(nope))
                nope += "a";

            Assert.IsNull(tree.Find(nope));
        }

        [Test]
        [TestCaseSource("GetLists")]
        public void TestRemoval(IEnumerable<string> values)
        {
            IEnumerable<Student> students = GetStudentList(values);
            Tree<Student> tree = GetTree(students);
            List<string> expected = (from value in values orderby value select value).Distinct().ToList();
            List<string> removeKeys = RandomizePredictably(expected).ToList();
            List<string> treeList;

            foreach (string removeKey in removeKeys)
            {
                tree.Remove(removeKey);

                expected = expected.Where(value => value != removeKey).ToList();
                treeList = (from student in tree select student.GetObjectStorageKey()).ToList();

                Assert.AreEqual(expected, treeList);
            }
        }

        [Test]
        public void TestRemoval()
        {
            IEnumerable<string> values = new[] { "6", "19", "9", "11", "18", "17", "7", "15", "4", "14", "12", "1", "13", "16", "2", "8", "10", "5", "3" };
            IEnumerable<string> removeKeys = new[] { "6", "19", "9", "11", "18", "17", "7", "15", "4", "14", "12", "1", "13", "16", "2", "8", "10", "5", "3" };
            IEnumerable<Student> students = GetStudentList(values);
            Tree<Student> tree = GetTree(students);
            List<string> expected = (from value in values orderby value select value).Distinct().ToList();
            List<string> treeList;

            foreach (string removeKey in removeKeys)
            {
                tree.Remove(removeKey);

                expected = expected.Where(value => value != removeKey).ToList();
                treeList = (from student in tree select student.GetObjectStorageKey()).ToList();

                Assert.AreEqual(expected, treeList);
            }
        }

        List<IEnumerable<string>> GetLists()
        {
            var lists = new List<IEnumerable<string>>();

            lists.Add(new[]{ "John", "Marie", "Xavier", "Adam", "Betty" });
            lists.Add(new[]{ "04", "01", "03", "02", "06", "05" });
            lists.Add(new[]{ "C", "A", "B" });
            lists.Add(new[]{ "363", "168", "610", "381", "236", "348", "833" });
            lists.Add(new[]{ "08", "07", "06", "05", "04", "03", "02", "01" });
            lists.Add(GetBigList(1000));

            return lists;
        }

        IEnumerable<Tree<Student>> GetTrees()
        {
            var trees = new List<Tree<Student>>();

            foreach (IEnumerable<string> list in GetLists())
                trees.Add(GetTree(GetStudentList(list)));

            return trees;
        }

        void TestNodeLinks(Node<Student> node)
        {
            foreach (int dir in new[] { 0, 1 })
            {
                if (node.GetChild(dir) == null)
                    continue;

                Assert.AreSame(node, node.GetChild(dir).Parent);
                TestNodeLinks(node.GetChild(dir));
            }
        }

        IEnumerable<string> GetBigList(int count)
        {
        return RandomizePredictably(from n in Enumerable.Range(1, count) select n.ToString());
        }

        Tree<Student> GetTree(IEnumerable<Student> students)
        {
            Tree<Student> tree = new Tree<Student>();
            foreach (Student student in students)
                tree.Add(student);

            return tree;
        }

        IEnumerable<Student> GetStudentList(IEnumerable<string> values)
        {
            var students = new List<Student>();
            foreach (string value in values)
                students.Add(new Student(value));

            return students;
        }

        IEnumerable<string> RandomizePredictably(IEnumerable<string> values)
        {
            var src = new List<Tuple<string, string>>();

            foreach (string value in values)
                src.Add(new Tuple<string, string>(value, CalculateMD5Hash(value)));

            return from value in src orderby value.Item2 select value.Item1;
        }

        static string CalculateMD5Hash(string input)
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