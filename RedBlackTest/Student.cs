using RedBlack;

namespace RedBlackTest
{
    public class Student : ITreeObject
    {
        private readonly string _key;

        public Student(string key)
        {
            _key = key;
        }

        public string GetObjectStorageKey()
        {
            return _key;
        }
    }
}