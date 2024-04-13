using System.Collections;

namespace HNIdesu.Collection
{
    internal sealed class ArrayList<T>:IList<T>
    {
        private T[] _InnerArray;
        public int Capacity => _InnerArray.Length;
        public int Count { get; private set; } = 0;

        public bool IsReadOnly => false;

        public T this[int index] { 
            get
            {
                if(index >=0&&index<Count)
                    return _InnerArray[index];
                throw new IndexOutOfRangeException();
            }
            set
            {
                if(index<0)
                    throw new IndexOutOfRangeException();
                if (index >= Capacity)
                {
                    int newSize = 1;
                    while (newSize <= index)
                        newSize *= 2;
                    Resize(newSize);
                }
                _InnerArray[index] = value;
                if (index > Count)
                    Count = index + 1;
            }
        }

        private void Resize(int newSize)
        {
            if (newSize < Capacity)
                return;
            var temp = new T[newSize];
            CopyTo(temp, 0);
            _InnerArray = temp;
        }
        public ArrayList(int capacity)
        {
            if (capacity > 0)
                _InnerArray = new T[capacity];
            else
                throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        public ArrayList()
        {
            _InnerArray = new T[1];
        }
        

        public Span<T> ToSpan()=> new Span<T>(_InnerArray, 0, Count);

        public int IndexOf(T item)
        {
            for (int i = 0, count = Count; i < count; i++)
                if (_InnerArray[i]!.Equals(item))
                    return i;
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            Count = 0;
        }

        public void Add(T item)
        {
            if (Count == Capacity)
                Resize(Capacity * 2);
            _InnerArray[Count++] = item;
        }

        public bool Contains(T item)=> IndexOf(item) != -1;

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0, count = Count; i < count; i++)
                array[arrayIndex++] = this[i];
        }

        private IEnumerable<T> Enumerate()
        {
            for (int i = 0, count = Count; i < count; i++)
                yield return _InnerArray[i];
        }

        public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()=> Enumerate().GetEnumerator();
    }
}
