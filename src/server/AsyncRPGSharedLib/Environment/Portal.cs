using System;
using System.Xml;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Utility;

namespace AsyncRPGSharedLib.Environment
{
    public enum ePortalType
    {
        door,
        stairs,
        teleporter
    }

    public class PortalTemplate
    {
        private int portal_id;
        private ePortalType portal_type;
        private AABB3d bounding_box;
        private MathConstants.eSignedDirection roomSide;

        public int PortalID
        {
            get { return portal_id; }
        }

        public AABB3d BoundingBox
        {
            get { return bounding_box; }
        }

        public MathConstants.eSignedDirection PortalRoomSide
        {
            get { return roomSide; }
        }

        public ePortalType PortalType
        {
            get { return portal_type; }
        }

        public PortalTemplate(XmlNode xmlNode)
        {
            int pixel_x = Int32.Parse(xmlNode.Attributes["x"].Value);
            int pixel_y = Int32.Parse(xmlNode.Attributes["y"].Value);
            int pixel_width =
                (xmlNode.Attributes["width"] != null)
                ? Int32.Parse(xmlNode.Attributes["width"].Value)
                : (int)GameConstants.TILE_PIXEL_SIZE;
            int pixel_height =
                (xmlNode.Attributes["height"] != null)
                ? Int32.Parse(xmlNode.Attributes["height"].Value)
                : (int)GameConstants.TILE_PIXEL_SIZE;

            Point3d position0 = GameConstants.ConvertPixelPositionToRoomPosition(pixel_x, pixel_y);
            Point3d position1 = GameConstants.ConvertPixelPositionToRoomPosition(pixel_x + pixel_width, pixel_y + pixel_height);

            portal_id = Int32.Parse(xmlNode.Attributes["id"].Value);

            bounding_box = 
                new AABB3d(
                    Point3d.Min(position0, position1),
                    Point3d.Max(position0, position1));

            roomSide = (MathConstants.eSignedDirection)Enum.Parse(typeof(MathConstants.eSignedDirection), xmlNode.Attributes["Direction"].Value);

            portal_type = (ePortalType)Enum.Parse(typeof(ePortalType), xmlNode.Attributes["Type"].Value);
        }
    }

    public class Portal
    {
        public int portal_id;
        public int target_portal_id;
        public int room_x;
        public int room_y;
        public int room_z;
        public ePortalType portal_type;
        public MathConstants.eSignedDirection room_side;
        public AABB3d bounding_box; // Room Relative

        public Portal()
        {
            portal_id= -1;
            target_portal_id = -1;
            room_x= 0;
            room_y= 0;
            room_z= 0;
            portal_type = ePortalType.door;
            room_side = MathConstants.eSignedDirection.positive_x;
            bounding_box = new AABB3d();
        }

        public void GetDefaultOpposingRoomPosition(
            out int opposing_room_x,
            out int opposing_room_y,
            out int opposing_room_z)
        {
            opposing_room_x = this.room_x;
            opposing_room_y = this.room_y;
            opposing_room_z = this.room_z;

            switch (room_side)
            {
                case MathConstants.eSignedDirection.positive_x:
                    {
                        opposing_room_x = this.room_x + 1;
                    }
                    break;
                case MathConstants.eSignedDirection.negative_x:
                    {
                        opposing_room_x = this.room_x - 1;
                    }
                    break;
                case MathConstants.eSignedDirection.positive_y:
                    {
                        opposing_room_y = this.room_y + 1;
                    }
                    break;
                case MathConstants.eSignedDirection.negative_y:
                    {
                        opposing_room_y = this.room_y - 1;
                    }
                    break;
                case MathConstants.eSignedDirection.positive_z:
                    {
                        opposing_room_z = this.room_z + 1;
                    }
                    break;
                case MathConstants.eSignedDirection.negative_z:
                    {
                        opposing_room_z = this.room_z - 1;
                    }
                    break;
            }
        }

        public static Portal CreatePortal(Room room, MathConstants.eSignedDirection roomSide)
        {
            Portal newPortal = new Portal();

            newPortal.portal_id = -1; // portal ID not set until this portal gets saved into the DB
            newPortal.target_portal_id = -1; 
            newPortal.room_side = roomSide;
            newPortal.room_x = room.room_key.x;
            newPortal.room_y = room.room_key.y;
            newPortal.room_z = room.room_key.z;

            switch (roomSide)
            {
                case MathConstants.eSignedDirection.positive_x:
                    {
                        Point3d p1= WorldConstants.ROOM_BOUNDS.Max;                        
                        Point3d p0= p1 - Vector3d.I*WorldConstants.PORTAL_WIDTH - Vector3d.J*WorldConstants.ROOM_Y_SIZE;

                        newPortal.bounding_box= new AABB3d(p0, p1);
                    }
                    break;
                case MathConstants.eSignedDirection.negative_x:
                    {
                        Point3d p0 = WorldConstants.ROOM_BOUNDS.Min;
                        Point3d p1 = p0 + Vector3d.I * WorldConstants.PORTAL_WIDTH + Vector3d.J * WorldConstants.ROOM_Y_SIZE;

                        newPortal.bounding_box= new AABB3d(p0, p1);
                    }
                    break;
                case MathConstants.eSignedDirection.positive_y:
                    {
                        Point3d p1 = WorldConstants.ROOM_BOUNDS.Max;
                        Point3d p0 = p1 - Vector3d.I * WorldConstants.ROOM_X_SIZE - Vector3d.J * WorldConstants.PORTAL_WIDTH;

                        newPortal.bounding_box= new AABB3d(p0, p1);
                    }
                    break;
                case MathConstants.eSignedDirection.negative_y:
                    {
                        Point3d p0 = WorldConstants.ROOM_BOUNDS.Min;
                        Point3d p1 = p0 + Vector3d.I * WorldConstants.ROOM_X_SIZE + Vector3d.J * WorldConstants.PORTAL_WIDTH;

                        newPortal.bounding_box= new AABB3d(p0, p1);
                    }
                    break;
                case MathConstants.eSignedDirection.positive_z:
                case MathConstants.eSignedDirection.negative_z:
                    {
                        int column = RNGUtilities.RandomInt(WorldConstants.ROOM_X_TILES / 3, 2 * WorldConstants.ROOM_X_TILES / 3);
                        int row = RNGUtilities.RandomInt(WorldConstants.ROOM_Y_TILES / 3, 2 * WorldConstants.ROOM_Y_TILES / 3);
                        Point3d p0 = WorldConstants.GetTilePosition(column, row);
                        Point3d p1 = p0 + Vector3d.I * WorldConstants.ROOM_TILE_SIZE + Vector3d.J * WorldConstants.ROOM_TILE_SIZE;

                        newPortal.bounding_box = new AABB3d(p0, p1);
                    }
                    break;
            }

            return newPortal;
        }

        // Setup this portal in the given room to oppose the target portal
        public static Portal CreateOpposingPortal(Portal targetPortal)
        {
            Portal newPortal = new Portal();

            newPortal.portal_id = -1; // Id not set until we save the portal into the DB
            newPortal.target_portal_id = targetPortal.portal_id;
            newPortal.bounding_box = targetPortal.bounding_box;

            switch (targetPortal.room_side)
            {
                case MathConstants.eSignedDirection.positive_x:
                    {
                        newPortal.room_side = MathConstants.eSignedDirection.negative_x;
                        newPortal.room_x = targetPortal.room_x + 1;
                        newPortal.room_y = targetPortal.room_y;
                        newPortal.room_z = targetPortal.room_z;

                        newPortal.bounding_box.Move(
                            -Vector3d.I * (WorldConstants.ROOM_BOUNDS.XWidth - targetPortal.bounding_box.XWidth));
                    }
                    break;
                case MathConstants.eSignedDirection.negative_x:
                    {
                        newPortal.room_side = MathConstants.eSignedDirection.positive_x;
                        newPortal.room_x = targetPortal.room_x - 1;
                        newPortal.room_y = targetPortal.room_y;
                        newPortal.room_z = targetPortal.room_z;

                        newPortal.bounding_box.Move(
                            Vector3d.I * (WorldConstants.ROOM_BOUNDS.XWidth - targetPortal.bounding_box.XWidth));
                    }
                    break;
                case MathConstants.eSignedDirection.positive_y:
                    {
                        newPortal.room_side = MathConstants.eSignedDirection.negative_y;
                        newPortal.room_x = targetPortal.room_x;
                        newPortal.room_y = targetPortal.room_y + 1;
                        newPortal.room_z = targetPortal.room_z;

                        newPortal.bounding_box.Move(
                            -Vector3d.J * (WorldConstants.ROOM_BOUNDS.YWidth - targetPortal.bounding_box.YWidth));
                    }
                    break;
                case MathConstants.eSignedDirection.negative_y:
                    {
                        newPortal.room_side = MathConstants.eSignedDirection.positive_y;
                        newPortal.room_x = targetPortal.room_x;
                        newPortal.room_y = targetPortal.room_y - 1;
                        newPortal.room_z = targetPortal.room_z;

                        newPortal.bounding_box.Move(
                            Vector3d.J * (WorldConstants.ROOM_BOUNDS.YWidth - targetPortal.bounding_box.YWidth));
                    }
                    break;
                case MathConstants.eSignedDirection.positive_z:
                    {
                        newPortal.room_side = MathConstants.eSignedDirection.negative_z;
                        newPortal.room_x = targetPortal.room_x;
                        newPortal.room_y = targetPortal.room_y;
                        newPortal.room_z = targetPortal.room_z + 1;

                        // Keep same bounding box
                    }
                    break;
                case MathConstants.eSignedDirection.negative_z:
                    {
                        newPortal.room_side = MathConstants.eSignedDirection.positive_z;
                        newPortal.room_x = targetPortal.room_x;
                        newPortal.room_y = targetPortal.room_y;
                        newPortal.room_z = targetPortal.room_z - 1;

                        // Keep same bounding box
                    }
                    break;
            }

            return newPortal;
        }

        public static Portal CreatePortal(Portals dbPortal)
        {
            Portal portal = new Portal();

            portal.portal_id = dbPortal.PortalID;
            portal.target_portal_id = dbPortal.TargetPortalID;
            portal.room_x = dbPortal.RoomX;
            portal.room_y = dbPortal.RoomY;
            portal.room_z = dbPortal.RoomZ;
            portal.room_side = (MathConstants.eSignedDirection)dbPortal.RoomSide;
            portal.bounding_box.SetPointBounds(
                new Point3d((float)dbPortal.BboxX0, (float)dbPortal.BboxY0, 0F),
                new Point3d((float)dbPortal.BboxX1, (float)dbPortal.BboxY1, 0F));

            return portal;
        }
    }
}