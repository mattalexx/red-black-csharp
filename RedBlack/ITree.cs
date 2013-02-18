namespace RedBlack
{
    public interface ITree<T>
    {
        void Add(T obj);
        void Remove(string key);
        T GetTop();
        T Find(string key);
        void PrintAllItems();
    }
}