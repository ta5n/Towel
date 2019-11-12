﻿using System;
using System.Collections.Generic;
using static Towel.Syntax;

namespace Towel.DataStructures
{
	/// <summary>Stores items based on priorities and allows access to the highest priority item.</summary>
	/// <typeparam name="T">The generic type to be stored within the heap.</typeparam>
	public interface IHeap<T> : IDataStructure<T>,
		// Structure Properties
		DataStructure.ICountable,
		DataStructure.IClearable,
		DataStructure.IComparing<T>
	{
		#region Methods

		/// <summary>Enqueues an item into the heap.</summary>
		/// <param name="addition"></param>
		void Enqueue(T addition);
		/// <summary>Removes and returns the highest priority item.</summary>
		/// <returns>The highest priority item from the queue.</returns>
		T Dequeue();
		/// <summary>Returns the highest priority item.</summary>
		/// <returns>The highest priority item in the queue.</returns>
		T Peek();

		#endregion
	}

	/// <summary>A heap with static priorities implemented as a array.</summary>
	/// <typeparam name="T">The type of item to be stored in this priority heap.</typeparam>
	/// <citation>
	/// This heap imlpementation was originally developed by 
	/// Rodney Howell of Kansas State University. However, it has 
	/// been modified since its addition into the Towel framework.
	/// </citation>
	public class HeapArray<T> : IHeap<T>
	{
		internal readonly Compare<T> _compare;
		internal T[] _heap;
		internal int _minimumCapacity;
		internal int _count;
		internal const int _root = 1; // The root index of the heap.

		#region Constructors

		/// <summary>Generates a priority queue with a capacity of the parameter. Runtime O(1).</summary>
		/// <param name="compare">Delegate determining the comparison technique used for sorting.</param>
		/// <runtime>θ(1)</runtime>
		public HeapArray(Compare<T> compare)
		{
			_compare = compare;
			_heap = new T[2];
			_minimumCapacity = 1;
			_count = 0;
		}

		/// <summary>Generates a priority queue with a capacity of the parameter. Runtime O(1).</summary>
		/// <param name="compare">Delegate determining the comparison technique used for sorting.</param>
		/// <param name="minimumCapacity">The capacity you want this priority queue to have.</param>
		/// <runtime>θ(1)</runtime>
		public HeapArray(Compare<T> compare, int minimumCapacity)
		{
			_compare = compare;
			_heap = new T[minimumCapacity + 1];
			_minimumCapacity = minimumCapacity;
			_count = 0;
		}

		/// <summary>Generates a priority queue with a capacity of the parameter. Runtime O(1).</summary>
		/// <runtime>θ(1)</runtime>
		public HeapArray() : this(Towel.Compare.Default) { }

		#endregion

		#region Properties

		/// <summary>Delegate determining the comparison technique used for sorting.</summary>
		public Compare<T> Compare => _compare;

		/// <summary>The maximum items the queue can hold.</summary>
		/// <runtime>θ(1)</runtime>
		public int CurrentCapacity => _heap.Length - 1;

		/// <summary>The minumum capacity of this queue to limit low-level resizing.</summary>
		/// <runtime>θ(1)</runtime>
		public int MinimumCapacity => _minimumCapacity;

		/// <summary>The number of items in the queue.</summary>
		/// <runtime>O(1)</runtime>
		public int Count => _count;

		#endregion

		#region Methods

		/// <summary>Gets the index of the left child of the provided item.</summary>
		/// <param name="parent">The item to find the left child of.</param>
		/// <returns>The index of the left child of the provided item.</returns>
		internal static int LeftChild(int parent) => parent * 2;

		/// <summary>Gets the index of the right child of the provided item.</summary>
		/// <param name="parent">The item to find the right child of.</param>
		/// <returns>The index of the right child of the provided item.</returns>
		internal static int RightChild(int parent) => parent * 2 + 1;

		/// <summary>Gets the index of the parent of the provided item.</summary>
		/// <param name="child">The item to find the parent of.</param>
		/// <returns>The index of the parent of the provided item.</returns>
		internal static int Parent(int child) => child / 2;

		/// <summary>Enqueue an item into the priority queue and let it works its magic.</summary>
		/// <param name="addition">The item to be added.</param>
		/// <runtime>O(ln(n)), Ω(1), ε(ln(n))</runtime>
		public void Enqueue(T addition)
		{
			if (!(_count + 1 < _heap.Length))
			{
				if (_heap.Length * 2 > int.MaxValue)
				{
					throw new InvalidOperationException("this heap has become too large");
				}
				T[] _newHeap = new T[_heap.Length * 2];
				for (int i = 1; i <= _count; i++)
				{
					_newHeap[i] = _heap[i];
				}
				_heap = _newHeap;
			}
			_count++;
			_heap[_count] = addition;
			ShiftUp(_count);
		}

		/// <summary>Dequeues the item with the highest priority.</summary>
		/// <returns>The item of the highest priority.</returns>
		/// <runtime>O(ln(n))</runtime>
		public T Dequeue()
		{
			if (_count > 0)
			{
				T removal = _heap[_root];
				ArraySwap(_root, _count);
				_count--;
				ShiftDown(_root);
				return removal;
			}
			throw new InvalidOperationException("Attempting to remove from an empty priority queue.");
		}

		/// <summary>Requeues an item after a change has occured.</summary>
		/// <param name="item">The item to requeue.</param>
		/// <runtime>O(n)</runtime>
		public void Requeue(T item)
		{
			int i;
			for (i = 1; i <= _count; i++)
			{
				if (_compare(item, _heap[i]) is Equal)
				{
					break;
				}
			}
			if (i > _count)
			{
				throw new InvalidOperationException("Attempting to re-queue an item that is not in the heap.");
			}
			ShiftUp(i);
			ShiftDown(i);
		}

		/// <summary>This lets you peek at the top priority WITHOUT REMOVING it.</summary>
		/// <runtime>O(1)</runtime>
		public T Peek()
		{
			if (_count > 0)
			{
				return _heap[_root];
			}
			throw new InvalidOperationException("Attempting to peek at an empty priority queue.");
		}

		/// <summary>Standard priority queue algorithm for up sifting.</summary>
		/// <param name="index">The index to be up sifted.</param>
		/// <runtime>O(ln(n)), Ω(1)</runtime>
		internal void ShiftUp(int index)
		{
			int parent;
			while ((parent = Parent(index)) > 0 && _compare(_heap[index], _heap[parent]) is Greater)
			{
				ArraySwap(index, parent);
				index = parent;
			}
		}

		/// <summary>Standard priority queue algorithm for sifting down.</summary>
		/// <param name="index">The index to be down sifted.</param>
		/// <runtime>O(ln(n)), Ω(1)</runtime>
		internal void ShiftDown(int index)
		{
			int leftChild, rightChild;
			while ((leftChild = LeftChild(index)) <= _count)
			{
				int down = leftChild;
				if ((rightChild = RightChild(index)) <= _count && _compare(_heap[rightChild], _heap[leftChild]) is Greater)
				{
					down = rightChild;
				}
				if (_compare(_heap[down], _heap[index]) is Less)
				{
					break;
				}
				ArraySwap(index, down);
				index = down;
			}
		}

		/// <summary>Standard array swap method.</summary>
		/// <param name="indexOne">The first index of the swap.</param>
		/// <param name="indexTwo">The second index of the swap.</param>
		/// <runtime>O(1)</runtime>
		internal void ArraySwap(int indexOne, int indexTwo)
		{
			T temp = _heap[indexTwo];
			_heap[indexTwo] = _heap[indexOne];
			_heap[indexOne] = temp;
		}

		/// <summary>Returns this queue to an empty state.</summary>
		/// <runtime>O(1)</runtime>
		public void Clear()
		{
			_count = 0;
		}

		/// <summary>Converts the heap into an array using pre-order traversal (WARNING: items are not ordered).</summary>
		/// <returns>The array of priority-sorted items.</returns>
		public T[] ToArray()
		{
			T[] array = new T[_count];
			for (int i = 1; i <= _count; i++)
			{
				array[i] = _heap[i];
			}
			return array;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>Gets the enumerator of the heap.</summary>
		/// <returns>The enumerator of the heap.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i <= _count; i++)
			{
				yield return _heap[i];
			}
		}

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		public void Stepper(Step<T> step) => _heap.Stepper(step, 1, _count + 1);

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		public void Stepper(StepRef<T> step) => _heap.Stepper(step, 1, _count + 1);

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		/// <returns>The resulting status of the iteration.</returns>
		public StepStatus Stepper(StepBreak<T> step) => _heap.Stepper(step, 1, _count + 1);

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		/// <returns>The resulting status of the iteration.</returns>
		public StepStatus Stepper(StepRefBreak<T> step) => _heap.Stepper(step, 1, _count + 1);

		/// <summary>Creates a shallow clone of this data structure.</summary>
		/// <returns>A shallow clone of this data structure.</returns>
		public IDataStructure<T> Clone()
		{
			HeapArray<T> clone = new HeapArray<T>(_compare);
			T[] cloneItems = new T[_heap.Length];
			for (int i = 1; i <= _count; i++)
			{
				cloneItems[i] = _heap[i];
			}
			clone._heap = cloneItems;
			clone._count = _count;
			clone._minimumCapacity = _minimumCapacity;
			return clone;
		}

		#endregion
	}
}