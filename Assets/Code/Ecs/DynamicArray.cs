using System;

namespace Wargon.TinyEcs {
    public sealed class DynamicArray<T> {
        public T[] data;
        public int Count;
        public int Len;
        public DynamicArray(int size) {
            Count = 0;
            Len = size;
            data = new T[Len];
        }

        public T this[int index] {
            get => data[index];
            set => data[index] = value;
        }

        public void Add(T item) {
            if (Len <= Count) {
                Len = Count + 16;
                Array.Resize(ref data, Len);
            }

            data[Count] = item;
            Count++;
        }

        public T Last() {
            return Count == 0 ? default : data[Count - 1];
        }

        public void RemoveLast() {
            Count--;
        }

        public void Clear() {
            Count = 0;
        }
    }
}