using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class MinHeap<T> : IEnumerable<T>
{
	private T[] nodes;
	Comparer<T> comparer = Comparer<T>.Default;

	public int Count { get; private set; }
	public int Capacity { get => this.nodes.Length; }
	public T Peek { get => this.nodes[0]; }


	private const int TREE_ARITY = 2;
	private const int LOG_TREE_ARITY = 1;
	private const int GROW_FACTOR = 2;
	private const int MIN_GROW = 4;
	private const int MAX_ARRAY_CAPACITY = Int32.MaxValue;

	public MinHeap(Comparer<T> comp = null)
	{
		this.nodes = new T[0];

		if (comp != null)
			this.comparer = comp;
	}

	public MinHeap(int capacity, Comparer<T> comp = null)
	{
		this.nodes = new T[capacity];

		if (comp != null)
			this.comparer = comp;
	}

	public MinHeap(IEnumerable<T> items, Comparer<T> comp = null)
	{
		this.nodes = items.ToArray();
		this.Count = this.nodes.Length;

		if (comp != null)
			this.comparer = comp;

		if (this.Count > 1)
		{
			Heapify();
		}
	}

	public int EnsureCapacity(int minCapacity)
	{
		this.Grow(minCapacity);

		return this.Capacity;
	}

	public void TrimExcess()
	{
		Array.Resize(ref this.nodes, this.Count);
	}

	private void Grow(int minCapacity)
	{
		if (minCapacity < this.Capacity)
			return;

		int newCapacity = Math.Max(MinHeap<T>.GROW_FACTOR * this.Capacity, this.Capacity + MinHeap<T>.MIN_GROW);

		if (newCapacity > MinHeap<T>.MAX_ARRAY_CAPACITY)
			newCapacity = MinHeap<T>.MAX_ARRAY_CAPACITY;

		Array.Resize(ref this.nodes, newCapacity);
	}

	public void Clear()
	{
		Array.Clear(this.nodes, 0, this.Capacity);
		this.Count = 0;
	}


	public bool IsEmpty()
	{
		return this.Count == 0;
	}


	private void Heapify()
	{
		int lastParentWithChildren = MinHeap<T>.GetParentOf(this.Count - 1);

		for (int index = lastParentWithChildren; index >= 0; --index)
		{
			MoveDown(this.nodes[index], index);
		}
	}

	private void MoveUp(T value, int index)
	{
		while (index > 0)
		{
			int parentIndex = MinHeap<T>.GetParentOf(index);
			T parent = this.nodes[parentIndex];

			if (this.comparer.Compare(value, parent) < 0)
			{
				this.nodes[index] = parent;
				index = parentIndex;
			}
			else
			{
				break;
			}
		}

		this.nodes[index] = value;
	}


	private void MoveDown(T value, int currentIndex)
	{
		int firstChildIndex = MinHeap<T>.GetFirstChildOf(currentIndex);


		while (firstChildIndex < this.Count)
		{
			T minChild = this.nodes[firstChildIndex];
			int minChildIndex = firstChildIndex;

			for (int i = firstChildIndex + 1; i < Math.Min(firstChildIndex + MinHeap<T>.TREE_ARITY, this.Count); i++)
			{
				if (this.comparer.Compare(this.nodes[i], minChild) < 0)
				{
					minChild = this.nodes[i];
					minChildIndex = i;
				}
			}

			if (this.comparer.Compare(value, minChild) <= 0)
			{
				break;
			}

			this.nodes[currentIndex] = minChild;
			currentIndex = minChildIndex;

			firstChildIndex = MinHeap<T>.GetFirstChildOf(currentIndex);
		}

		this.nodes[currentIndex] = value;
	}

	public void Enqueue(T value)
	{
		int index = this.Count;
		this.Count += 1;

		if (this.Count > this.Capacity)
			this.Grow(this.Count);

		this.MoveUp(value, index);

	}

	public T Dequeue()
	{
		if (this.Count == 0)
			throw new ArgumentOutOfRangeException("MinHeap is Empty");

		T ret = this.nodes[0];
		T value = this.nodes[--this.Count];

		if(this.Count > 0)
			this.MoveDown(value, 0);

		return ret;
	}

	public T DequeueEnqueue(T value)
	{
		T root = this.nodes[0];

		if (this.comparer.Compare(value, root) > 0)
			MoveDown(value, 0);
		else
			this.nodes[0] = value;

		return root;
	}

	public T EnqueueDequeue(T value)
	{
		if (this.Count != 0)
		{
			T root = this.nodes[0];


			if (this.comparer.Compare(value, root) > 0)
			{
				MoveDown(value, 0);
				return root;
			}

		}

		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetFirstChildOf(int parent)
	{
		return (parent << MinHeap<T>.LOG_TREE_ARITY) + 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetParentOf(int child)
	{
		return (child - 1) >> MinHeap<T>.LOG_TREE_ARITY;
	}




	public IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)this.nodes).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)this.nodes).GetEnumerator();
	}
}
