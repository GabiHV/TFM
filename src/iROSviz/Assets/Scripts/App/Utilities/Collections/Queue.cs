using System.Collections;

namespace App.Utilities.Collections
{
    public class Queue<T>
    {
        private T[] _buff;
        private int _count = 0;

        public Queue(int maxCapacity)
        {
            _buff = new T[maxCapacity];
        }

        public void Enqueue(T element)
        {
            if(QueueFull()) return;
            _buff[_count++] = element;
        }

        public T Dequeue()
        {
            if(QueueEmpty()) return default(T);
            return _buff[--_count];
        }

        public T Peek() =>
            QueueEmpty() ? default(T) : _buff[_count - 1];

        public void Clean() =>
            _count = 0;

        public bool QueueFull() =>
            _count == _buff.Length;
        
        public bool QueueEmpty() =>
            _count == 0;

        public IEnumerator GetEnumerator()
        {
            for(int i = 0; i < _count; i++)
            {
                yield return _buff[i];
            }
        }

        public T[] ToArray()
        {
            T[] array = new T[_count];
            for(int i = 0; i < _count; i++)
            {
                array[i] = _buff[i];
            }
            return array;
        }
    }    
}

