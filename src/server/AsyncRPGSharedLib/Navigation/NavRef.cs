namespace AsyncRPGSharedLib.Navigation
{
    public class NavRef
    {
        private int m_navCellIndex;
        private RoomKey m_roomKey;

        public NavRef(NavRef navRef)
        {
            m_navCellIndex = navRef.m_navCellIndex;
            m_roomKey = new RoomKey(navRef.m_roomKey);
        }

		public NavRef()
		{
			m_navCellIndex = -1;
			m_roomKey = new RoomKey();
		}

        public NavRef(int navCellIndex, RoomKey roomKey)
        {
            m_navCellIndex = navCellIndex;

            if (roomKey != null)
            {
                m_roomKey = new RoomKey(roomKey);
            }
            else
            {
                m_roomKey = new RoomKey();
            }
        }

        public bool IsValid
        {
            get { return m_navCellIndex >= 0 && m_roomKey != null; }
        }

        public int NavCellIndex
        {
            get { return m_navCellIndex; }
        }

        public RoomKey NavRoomKey
        {
            get { return m_roomKey; }
        }

        public bool Equals(NavRef other)
        {
            return this.m_navCellIndex == other.m_navCellIndex
                && ((this.m_roomKey != null && this.m_roomKey.Equals(other.m_roomKey))
                    || (this.m_roomKey == null && other.m_roomKey == null));
        }
    }
}
