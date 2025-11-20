using System;
using System.Collections.Generic;

namespace MiniIT.CORE
{
    /// <summary>
    /// Simple generic object pool.
    /// </summary>
    /// <typeparam name="T">Type implementing IPoolable.</typeparam>
    public class ObjectPool<T> where T : class, IPoolable
    {
        private readonly Func<T> _factory;
        private readonly Stack<T> _items = new Stack<T>();

        /// <summary>Create pool with factory and initial capacity.</summary>
        public ObjectPool(Func<T> factory, int initial = 0)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            for (int i = 0; i < initial; ++i)
            {
                _items.Push(_factory());
            }
        }

        /// <summary>Rent instance from pool.</summary>
        public T Rent()
        {
            T item = _items.Count > 0 ? _items.Pop() : _factory();
            item.OnSpawned();
            return item;
        }

        /// <summary>Return instance to pool.</summary>
        public void Return(T item)
        {
            if (item == null)
            {
                return;
            }

            item.OnDespawned();
            _items.Push(item);
        }
    }
}