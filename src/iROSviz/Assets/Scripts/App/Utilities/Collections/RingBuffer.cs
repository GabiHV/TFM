using System.Collections.Generic;

namespace App.Utilities.Collections
{
    public class RingBuffer<T>
    {
        private T[] _buffer;
        private int _head = 0; 
        private int _count = 0;

        public RingBuffer(int capacity = 10)
        {
            _buffer = new T[capacity];
        }

        public void Push(T item)
        {
            _buffer[_head] = item;
            _head = (_head + 1) % _buffer.Length;
            if(_count < _buffer.Length) _count++;
        }

        public T Pop()
        {
            if(_count == 0) return default(T);

            T item = _buffer[_head];

            _head = (_head + 1) % _buffer.Length;
            _count--;

            return item;
        }

        public T Peek() =>
            _buffer[_head];

        public int Count() =>
            _count;

        public IEnumerable<T> GetAll()
        {
            if(_count == 0) yield break;

            int start = (_head - _count + _buffer.Length) % _buffer.Length;
            for(int i = 0; i < _count; i++)
            {
                yield return _buffer[(start + i) % _buffer.Length];
            }
        }
        
    }
}

