using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public class CommonPool<T> where T : Object
	{
		private readonly Queue<T> _pool;
		public CommonPool() => _pool = new Queue<T>();
		public void AddToPool(T poolObject) => _pool.Enqueue(poolObject);
		public T GetFromPool() => _pool.Count == 0 ? default : _pool.Dequeue();
		public Queue<T> GetPool() => _pool;
		public void Clear() => _pool.Clear();
	}
}
