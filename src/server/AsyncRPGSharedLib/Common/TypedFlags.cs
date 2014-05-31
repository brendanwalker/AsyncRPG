using System;

namespace AsyncRPGSharedLib.Common
{
    [Serializable]
    public class TypedFlags<T>
    {
        public TypedFlags()
        {
            Clear();
        }

        public TypedFlags(TypedFlags<T> other)
        {
            m_bitmask = other.m_bitmask;
        }

        public TypedFlags(uint bitmask)
        {
            m_bitmask = bitmask;
        }

        public uint Bitmask
        {
            get { return m_bitmask; }
            set { m_bitmask = value; }
        }

        public void Clear()
        {
            m_bitmask = 0;
        }

        public bool IsEmpty()
        {
            return m_bitmask == 0;
        }

        public bool Test(T bit_index)
        {
            int index = (int)(object)bit_index;

            return (index >= 0 && index < 32) ? ((m_bitmask & (1 << index)) > 0) : false;
        }

        public void Set(T bit_index, bool flag)
        {
            int index = (int)(object)bit_index;

            if (index >= 0 && index < 32)
            {
                if (flag)
                {
                    m_bitmask = m_bitmask | (uint)(1 << index);
                }
                else
                {
                    m_bitmask = m_bitmask & ~(uint)(1 << index);
                }
            }
        }

        public override int GetHashCode()
        {
            return (int)m_bitmask;
        }

        public override bool Equals(System.Object obj)
        {
            TypedFlags<T> other = obj as TypedFlags<T>;

            return m_bitmask == other.m_bitmask;
        }

        public bool Equals(TypedFlags<T> p)
        {
            return m_bitmask == p.m_bitmask;
        }

        public static bool operator ==(TypedFlags<T> a, TypedFlags<T> b)
        {
            return a.m_bitmask == b.m_bitmask;
        }

        public static bool operator !=(TypedFlags<T> a, TypedFlags<T> b)
        {
            return a.m_bitmask != b.m_bitmask;
        }

        public static uint FLAG(T bit_index)
        {
            return (uint)(1 << (int)(object)bit_index);
        }

        private uint m_bitmask;
    }
}
