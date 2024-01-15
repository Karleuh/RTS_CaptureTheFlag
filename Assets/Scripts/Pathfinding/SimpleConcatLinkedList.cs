using System;
using System.Collections;
using System.Collections.Generic;

public class SimpleConcatLinkedList<T> : ICollection<T>
{
	public SimpleConcatLinkedListNode<T> First { get; private set; }
	public SimpleConcatLinkedListNode<T> Last { get => First?.Prev;  }

	public int Count { get; private set; }

	public bool IsReadOnly => false;

	public SimpleConcatLinkedList()
	{
		this.Count = 0;
		this.First = null;
	}

	public SimpleConcatLinkedList(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}

		this.Count = 0;
		SimpleConcatLinkedListNode<T> current = null;

		foreach (T item in collection)
		{
			this.Count += 1;

			if (current == null)
			{
				this.First = new SimpleConcatLinkedListNode<T>(item);
				current = this.First;
			}
			else
			{
				SimpleConcatLinkedListNode<T> node = new SimpleConcatLinkedListNode<T>(item);
				node.Prev = current;
				current = node;
			}
		}
		this.First.Prev = this.Last;
	}

	public void AddLast(SimpleConcatLinkedListNode<T> node)
	{
		if (this.Count == 0)
		{
			this.First = node;
			this.First.Prev = node;
		}
		else
		{
			node.Prev = this.Last;
			this.First.Prev = node;
		}

		this.Count += 1;
	}

	public void AddLast(T item)
	{
		this.AddLast(new SimpleConcatLinkedListNode<T>(item));
	}

	public void AddFirst(SimpleConcatLinkedListNode<T> node)
	{
		if (this.Count == 0)
		{
			this.First = node;
			this.First.Prev = node;
		}
		else
		{
			node.Prev = this.Last;
			this.First.Prev = node;

			this.First = node;
		}
		this.Count += 1;
	}

	public void AddFirst(T item)
	{
		if (this.Count == 0)
		{
			this.First = new SimpleConcatLinkedListNode<T>(item);
			this.First.Prev = this.First;
		}
		else
		{
			this.First.Prev = new SimpleConcatLinkedListNode<T>(item);
			this.First.Prev.Prev = this.Last;

			this.First = this.First.Prev;
		}
		this.Count += 1;
	}

	public T RemoveLast()
	{
		if (this.Count == 0)
			return default;

		SimpleConcatLinkedListNode<T> node = this.Last;
		if(this.Count == 1)
		{
			this.Clear();
			return node.Value;
		}


		this.First.Prev = node.Prev;

		this.Count -= 1;

		return node.Value;
	}

	public void ConcatBefore(SimpleConcatLinkedList<T> list)
	{
		if (list == null || list.Count == 0)
			return;
		if(this.Count == 0)
		{
			this.First = list.First;
			this.Count = list.Count;
			list.Clear();
			return;
		}


		SimpleConcatLinkedListNode<T> last = this.Last;
		this.First.Prev = list.Last;
		list.First.Prev = last;

		this.First = list.First;

		this.Count = this.Count + list.Count;

		list.Clear();

	}

	public void ConcatAfter(SimpleConcatLinkedList<T> list)
	{
		if (list == null || list.Count == 0)
			return;

		if (this.Count == 0)
		{
			this.First = list.First;
			this.Count = list.Count;
			list.Clear();
			return;
		}

		SimpleConcatLinkedListNode<T> last = this.Last;
		this.First.Prev = list.Last;
		list.First.Prev = last;

		this.Count = this.Count + list.Count;

		list.Clear();
	}


	public void Clear()
	{
		this.Count = 0;
		this.First = null;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	public void Add(T item)
	{
		this.AddLast(item);
	}

	public bool Contains(T item)
	{
		bool first = true;
		for (SimpleConcatLinkedListNode<T> current = this.Last; first || current != this.Last; first = false, current = current.Prev)
		{
			if (current.Value.Equals(item))
				return true;
		}
		return false;
	}

	public void CopyTo(T[] array, int index)
	{
		if (array == null)
			throw new ArgumentNullException("array");

		if (index < 0 || index > array.Length)
			throw new ArgumentOutOfRangeException("index out of range");

		if (array.Length - index < this.Count)
		{
			throw new ArgumentException("Argument insufficient space");
		}

		SimpleConcatLinkedListNode<T> node = this.Last;
		if (node != null)
		{
			int i = 0;
			do
			{
				array[index + this.Count - 1 - i++] = node.Value;
				node = node.Prev;
			} while (node != this.Last);
		}
	}

	public bool Remove(T item)
	{
		SimpleConcatLinkedListNode<T> node = this.Last;
		SimpleConcatLinkedListNode<T> next = this.First;
		if (this.Count == 0)
			return false;
		else if(this.Count == 1)
		{
			if(node.Value.Equals(item))
			{
				this.Clear();
				return true;
			}
		}
		else if (node != null)
		{
			do
			{
				if(node.Value.Equals(item))
				{
					next.Prev = node.Prev;
					return true;
				}
				next = node;
				node = node.Prev;
			} while (node != this.Last);
		}

		return false;
	}

	public bool Remove(SimpleConcatLinkedListNode<T> node, SimpleConcatLinkedListNode<T> next)
	{
		if (this.Count == 0)
			return false;
		else if (this.Count == 1)
		{
			if (this.First == node)
			{
				this.Clear();
				return true;
			}
			
		}
		else if (node != null)
		{
			next.Prev = node.Prev;
			if (this.First == node)
				this.First = next;
			this.Count -= 1;
		}

		return false;
	}

	public struct Enumerator : IEnumerator<T>
	{
		private T _current;
		private SimpleConcatLinkedList<T> list;
		private SimpleConcatLinkedListNode<T> node;

		T IEnumerator<T>.Current => _current;

		object IEnumerator.Current => _current;


		public Enumerator(SimpleConcatLinkedList<T> list)
		{
			this.list = list;
			this._current = default;
			this.node = list.Last;
		}


		void IDisposable.Dispose()
		{
		}

		bool IEnumerator.MoveNext()
		{
			if (this.node == null)
				return false;

			this._current = this.node.Value;
			this.node = this.node.Prev;
			if (this.node == this.list.Last)
				this.node = null;

			return true;
		}

		void IEnumerator.Reset()
		{
			this._current = default;
			this.node = list.Last;
		}
	}

}

public class SimpleConcatLinkedListNode<T>
{
	public T Value { get; set; }
	public SimpleConcatLinkedListNode<T> Prev { get; set; }

	public SimpleConcatLinkedListNode(T item)
	{
		this.Value = item;
	}
}
