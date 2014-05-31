using System;
using System.Collections.Generic;

namespace AsyncRPGSharedLib.Navigation
{
    public class UnionFind<T> where T : IEquatable<T>
    {
        public UnionFind()
        {
            m_set = new List<SetEntry>();
            m_payloadToIndexMap = new Dictionary<T, int>();
        }

        public int SetSize
        {
            get
            {
                return m_set.Count;
            }
        }

        public void AddElement(T payload)
        {
            // Keep a mapping of where a given payload exists in the UnionFind
            m_payloadToIndexMap.Add(payload, m_set.Count);

            // Add the payload to the set
            {
                SetEntry newEntry = new SetEntry();

                newEntry.parent_index = m_set.Count;
                newEntry.rank = 0;
                newEntry.payload = payload;

                m_set.Add(newEntry);
            }
        }

        public T GetElement(int index)
        {
            return m_set[index].payload;
        }

        public void Union(T payloadA, T payloadB)
        {
            int indexA = FindElementIndex(payloadA);
            int indexB = FindElementIndex(payloadB);

            Union(indexA, indexB);
        }

        public void Union(int indexA, int indexB)
        {
            int rootIndexA = FindRootIndex(indexA);
            int rootIndexB = FindRootIndex(indexB);

            if (rootIndexA != rootIndexB)
            {
                // A and B are not in the same set, merge them
                if (m_set[rootIndexA].rank < m_set[rootIndexB].rank)
                {
                    SetEntry entryA = m_set[rootIndexA];

                    entryA.parent_index = rootIndexB;
                }
                else if (m_set[rootIndexA].rank > m_set[rootIndexB].rank)
                {
                    SetEntry entryB = m_set[rootIndexB];

                    entryB.parent_index = rootIndexA;
                }
                else
                {
                    SetEntry entryA = m_set[rootIndexA];
                    SetEntry entryB = m_set[rootIndexB];

                    entryB.parent_index = rootIndexA;
                    entryA.rank++;
                }
            }
        }

        public bool AreElementsConnected(T payloadA, T payloadB)
        {
            int indexA = FindElementIndex(payloadA);
            int indexB = FindElementIndex(payloadB);

            return AreElementsConnected(indexA, indexB);
        }

        public bool AreElementsConnected(int indexA, int indexB)
        {
            int rootIndexA = FindRootIndex(indexA);
            int rootIndexB = FindRootIndex(indexB);

            return (rootIndexA == rootIndexB);
        }

        public int FindRootIndex(T payload)
        {
            int index = FindElementIndex(payload);

            return FindRootIndex(index);
        }

        public int FindRootIndex(int index)
        {
            if (index != m_set[index].parent_index)
            {
                SetEntry setEntry = m_set[index];

                setEntry.parent_index = FindRootIndex(m_set[index].parent_index);
            }

            return m_set[index].parent_index;
        }

        public int FindElementIndex(T payload)
        {
            int index = -1;

            if (!m_payloadToIndexMap.TryGetValue(payload, out index))
            {
                index = -1;
            }

            return index;
        }

        private class SetEntry
        {
            public T payload;
            public int parent_index;
            public int rank;
        }

        private List<SetEntry> m_set;
        private Dictionary<T, int> m_payloadToIndexMap;
    }
}
