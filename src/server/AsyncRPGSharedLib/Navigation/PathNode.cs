namespace AsyncRPGSharedLib.Navigation
{
    public class PathNode
    {
        public enum eSetMembership
        {
            open_set,
            closed_set
        }

        private int m_ID;
        private PathNode m_parentNode;
        private uint m_navCellIndex;
        private float m_cost;
        private float m_total;
        private eSetMembership m_setMembership;

        public PathNode(int ID, PathNode parentNode, uint navCellIndex, float cost, float total)
        {
            m_ID = ID;
            m_parentNode = parentNode;
            m_navCellIndex = navCellIndex;
            m_cost = cost;
            m_total = total;
            m_setMembership = eSetMembership.open_set;
        }

        public void MarkAsInClosedSet()
        {
            m_setMembership = eSetMembership.closed_set;
        }

        public int NodeID
        {
            get { return m_ID; }
        }

        public PathNode ParentNode
        {
            get { return m_parentNode; }
            set { m_parentNode = value; }
        }

        public uint NavCellIndex
        {
            get { return m_navCellIndex; }
        }

        public float Cost
        {
            get { return m_cost; }
            set { m_cost = value; }
        }

        public float Total
        {
            get { return m_total; }
            set { m_total = value; }
        }

        public bool InOpenSet
        {
            get { return m_setMembership == eSetMembership.open_set; }
        }

        public bool InClosedSet
        {
            get { return m_setMembership == eSetMembership.closed_set; }
        }
    }
}
