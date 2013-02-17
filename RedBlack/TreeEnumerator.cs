using System.Collections;
using System.Collections.Generic;

namespace RedBlack
{
    public class TreeEnumerator<T> : IEnumerator<T>
    {
        Stack _stack = new Stack();
        T _current;

        public TreeEnumerator(Node node)
        {
            while (node != null)
            {
                _stack.Push(node);
                node = node.Right;
            }
        }

        // Summary:
        //     Gets the current element in the collection.
        //
        // Returns:
        //     The current element in the collection.
        public T Current
        {
            get { return _current; }
            set { _current = value; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        // Summary:
        //     Advances the enumerator to the next element of the collection.
        //
        // Returns:
        //     true if the enumerator was successfully advanced to the next element; false
        //     if the enumerator has passed the end of the collection.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The collection was modified after the enumerator was created.
        public bool MoveNext()
        {
            return false;
        }

        //
        // Summary:
        //     Sets the enumerator to its initial position, which is before the first element
        //     in the collection.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The collection was modified after the enumerator was created.
        public void Reset()
        {
        }

        public void Dispose()
        {
        }
    }
}
