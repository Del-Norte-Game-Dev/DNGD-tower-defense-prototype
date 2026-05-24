using System;
using System.Collections.Generic;

// min binary heap, parent ≤ children
public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    private struct HeapNode {
        public TElement element;
        public TPriority priority;

        public HeapNode (TElement e, TPriority p)
        {
            element = e;
            priority = p;
        }
    }

    private List<HeapNode> heap;
    public int Count => heap.Count;

    public PriorityQueue()
    {
        heap = new List<HeapNode>();
    }

    public void Enqueue(TElement e, TPriority p)
    {
        HeapNode current = new HeapNode(e, p);
        heap.Add(current);

        int index = heap.Count - 1;

        while (index > 0)
        {
            int parentIndex = GetParent(index);

            if (heap[parentIndex].priority.CompareTo(heap[index].priority) <= 0)
                break;

            SwapElement(parentIndex, index);
            index = parentIndex;
        }
    }

    public TElement Dequeue()
    {
        if (heap.Count == 0)
            return default;

        TElement result = heap[0].element;

        // move last to root
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);

        int index = 0;

        while (true)
        {
            int leftChild = GetLeftChild(index);
            int rightChild = GetRightChild(index);

            int smallest = index;

            // check left
            if (leftChild < heap.Count &&
                heap[leftChild].priority.CompareTo(heap[smallest].priority) < 0)
            {
                smallest = leftChild;
            }

            // check right
            if (rightChild < heap.Count &&
                heap[rightChild].priority.CompareTo(heap[smallest].priority) < 0)
            {
                smallest = rightChild;
            }

            if (smallest == index)
                break;

            SwapElement(index, smallest);
            index = smallest;
        }

        return result;
    }

    public TElement Peek()
    {
        if (heap.Count == 0)
            return default;

        return heap[0].element;
    }

    private int GetParent(int index)
    {
        return (index - 1) / 2;
    }

    private int GetLeftChild(int index)
    {
        return index * 2 + 1;
    }

    private int GetRightChild(int index)
    {
        return index * 2 + 2;
    }

    private void SwapElement(int i, int j)
    {
        HeapNode temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }
}
