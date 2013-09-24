using NUnit.Framework;
using RedBlack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RedBlackTest
{
    [TestFixture]
    public class TreeTest
    {
        [Test]
        [TestCaseSource("GetTrees")]
        public void TestOrdering(Tree<Student> tree)
        {
            IEnumerable<Node<Student>> nodes = tree.GetNodes()
                .Where(node => (node.LeftNode != null && node.CompareTo(node.LeftNode.GetMyselfAndDescendants().Max()) != 1)
                    || (node.RightNode != null && node.CompareTo(node.RightNode.GetMyselfAndDescendants().Min()) != -1));

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
            Assert.False(tree.GetRootNode().IsRed, "Root node must be black");
        }

        [Test]
        [TestCaseSource("GetTrees")]
        public void TestNoRedChildrenOnRedNodes(Tree<Student> tree)
        {
            IEnumerable<Node<Student>> nodes = tree.GetNodes()
                .Where(node => node.IsRed && ((node.LeftNode != null && node.LeftNode.IsRed) || (node.RightNode != null && node.RightNode.IsRed)));

            Assert.IsEmpty(nodes, "Red nodes cannot have red children");
        }

        [Test]
        [TestCaseSource("GetTrees")]
        public void TestBlackDepthOfLeaves(Tree<Student> tree)
        {
            IEnumerable<int> depths = tree.GetNodes()
                .Where(node => node.LeftNode == null || node.RightNode == null)
                .Select(node => node.GetMyselfAndAncestors().Count(n => !n.IsRed));

            Assert.NotNull(depths);

            int distinctDepthCount = depths.Distinct().Count();

            Assert.AreEqual(1, distinctDepthCount, "Every leaf must have the same black depth");
        }

        [Test]
        [TestCaseSource("GetLists")]
        public void TestEnumeration(IEnumerable<string> values)
        {
            Tree<Student> tree = GetTree(GetStudentList(values));

            List<string> result = tree.Select(student => student.GetObjectStorageKey()).ToList();
            List<string> expected = values.OrderBy(name => name).Distinct().ToList();

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
            var valuesList = values as IList<string> ?? values.ToList();
            IEnumerable<Student> students = GetStudentList(valuesList);
            Tree<Student> tree = GetTree(students);
            List<string> expected = valuesList.OrderBy(value => value).Distinct().ToList();
            List<string> removeKeys = ShufflePredictably(expected).ToList();

            foreach (string removeKey in removeKeys)
            {
                tree.Remove(removeKey);

                expected = expected.Where(value => value != removeKey).ToList();
                List<string> treeList = tree.Select(student => student.GetObjectStorageKey()).ToList();

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
            List<string> expected = values.OrderBy(value => value).Distinct().ToList();

            foreach (string removeKey in removeKeys)
            {
                tree.Remove(removeKey);

                expected = expected.Where(value => value != removeKey).ToList();
                List<string> treeList = tree.Select(student => student.GetObjectStorageKey()).ToList();

                Assert.AreEqual(expected, treeList);
            }
        }

        List<IEnumerable<string>> GetLists()
        {
            var lists = new List<IEnumerable<string>>
            {
                new[] {"John", "Marie", "Xavier", "Adam", "Betty"},
                new[] {"04", "01", "03", "02", "06", "05"},
                new[] {"C", "A", "B"},
                new[] {"363", "168", "610", "381", "236", "348", "833"},
                new[] {"08", "07", "06", "05", "04", "03", "02", "01"},
                GetBigList(1000)
            };

            return lists;
        }

        IEnumerable<Tree<Student>> GetTrees()
        {
            return GetLists().Select(list => GetTree(GetStudentList(list))).ToList();
        }

        void TestNodeLinks(Node<Student> node)
        {
            foreach (int dir in new[] { 0, 1 }.Where(dir => node.GetChild(dir) != null))
            {
                Assert.AreSame(node, node.GetChild(dir).Parent);
                TestNodeLinks(node.GetChild(dir));
            }
        }

        IEnumerable<string> GetBigList(int count)
        {
            return ShufflePredictably(Enumerable.Range(1, count).Select(n => n.ToString()));
        }

        Tree<Student> GetTree(IEnumerable<Student> students)
        {
            var tree = new Tree<Student>();
            foreach (Student student in students)
                tree.Add(student);

            return tree;
        }

        IEnumerable<Student> GetStudentList(IEnumerable<string> values)
        {
            return values.Select(value => new Student(value)).ToList();
        }

        IEnumerable<string> ShufflePredictably(IEnumerable<string> values)
        {
            var src = values.Select(value => new Tuple<string, string>(value, CalculateMD5Hash(value))).ToList();

            return src.OrderBy(value => value.Item2).Select(value => value.Item1);
        }

        static string CalculateMD5Hash(string input)
        {
            var md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString();
        }
    }
}