using AsyncRPGSharedLib.Environment;

namespace AsyncRPGSharedLib.Navigation
{
    public class PathStep
    {
        private NavRef m_navRef;
        private Point3d m_point;

        public PathStep(NavRef navRef, Point3d point)
        {
            m_navRef = new NavRef(navRef.NavCellIndex, navRef.NavRoomKey);
            m_point = new Point3d(point);
        }

        public NavRef StepNavRef
        {
            get { return m_navRef; }
        }

        public Point3d StepPoint
        {
            get { return m_point; }
        }
    }
}
