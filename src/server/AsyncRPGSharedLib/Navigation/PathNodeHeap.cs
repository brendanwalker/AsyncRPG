using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncRPGSharedLib.Navigation
{
    class PathNodeHeap
    {
        private List<PathNode> m_heap;

        public PathNodeHeap()
        {
            Clear();
        }

        public void Clear()
        {
            m_heap = new List<PathNode>();
        }

        public PathNode Top()
        {
            return (m_heap.Count > 0) ? m_heap[0] : null;
        }

        public PathNode Pop()
        {
            PathNode result = null;

            if (m_heap.Count > 0)
            {
                m_heap.RemoveAt(0);

                if (m_heap.Count > 0)
                {
                    TrickleDown(0);
                }
            }

            return result;
        }

        public void Push(PathNode node)
        {
            m_heap.Add(node);
            BubbleUp(m_heap.Count - 1);
        }

        public bool Empty()
        {
            return m_heap.Count == 0;
        }

        private void BubbleUp(int heapIndex)
        {
            while (heapIndex > 0)
            {
                int parentIndex = (heapIndex - 1) / 2;

                // If the node has a smaller cost than it's parent
                // swap the node with it's parent and continue
                if (m_heap[heapIndex].Total < m_heap[parentIndex].Total)
                {
                    PathNode temp = m_heap[heapIndex];
                    m_heap[heapIndex] = m_heap[parentIndex];
                    m_heap[parentIndex] = temp;

                    heapIndex = parentIndex;
                }
                // Otherwise the node is where it should be in the heap
                else
                {
                    break;
                }
            }
        }

        private void TrickleDown(int heapIndex)
        {
            while (heapIndex < (m_heap.Count / 2))
            {
                // Start with the left child
                int childIndex = 2 * heapIndex + 1;

                // If the right child has a smaller cost, switch to that child
                if ((childIndex < (m_heap.Count - 1)) &&
                    (m_heap[childIndex].Total > m_heap[childIndex + 1].Total))
                {
                    childIndex++;
                }

                // If the node has a greater cost then it's smallest child
                // then swap the node with the child and repeat the process
                if ((childIndex < (m_heap.Count - 1)) &&
                    m_heap[heapIndex].Total > m_heap[childIndex].Total)
                {
                    PathNode temp = m_heap[heapIndex];

                    m_heap[heapIndex] = m_heap[childIndex];
                    m_heap[childIndex] = temp;

                    heapIndex = childIndex;
                }
                // Otherwise the node is where it should be in the heap
                else
                {
                    break;
                }
            }
        }
    }
}
