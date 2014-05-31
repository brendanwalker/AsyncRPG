using System;
using System.Collections.Generic;
using System.Xml;
using AsyncRPGSharedLib.RequestProcessors;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Navigation;

namespace AsyncRPGSharedLib.Environment
{
    public class EnergyTankTemplate
    {
        private int m_id;
        private Point3d m_position;
        private int m_energy;

        public EnergyTankTemplate(XmlNode xmlNode)
        {
            int pixel_x = Int32.Parse(xmlNode.Attributes["x"].Value);
            int pixel_y = Int32.Parse(xmlNode.Attributes["y"].Value);

            m_id = Int32.Parse(xmlNode.Attributes["id"].Value);
            m_energy = Int32.Parse(xmlNode.Attributes["Energy"].Value);

            m_position = GameConstants.ConvertPixelPositionToRoomPosition(pixel_x, pixel_y);
        }

        public int ID
        {
            get { return m_id; }
        }

        public Point3d Position
        {
            get { return m_position; }
        }

        public int Energy
        {
            get { return m_energy; }
        }
    }

    public class EnergyTank : CacheableObject, IEnvironmentEntity
    {
        private int m_energy_tank_id;
        private RoomKey m_room_key;
        private Point3d m_position;
        private int m_energy;
        private GameConstants.eFaction m_faction;

        public EnergyTank()
        {
            m_room_key = new RoomKey();
            m_energy_tank_id = -1;
            m_position = new Point3d();
            m_energy = 0;
            m_faction = GameConstants.eFaction.neutral;
        }

        public static EnergyTank CreateEnergyTank(RoomKey roomKey, EnergyTankTemplate template)
        {
            EnergyTank newEnergyTank = new EnergyTank();

            newEnergyTank.m_room_key = new RoomKey(roomKey);
            newEnergyTank.m_energy_tank_id = -1; // energy tank ID not set until this gets saved into the DB
            newEnergyTank.m_position = new Point3d(template.Position);
            newEnergyTank.m_energy = template.Energy;
            newEnergyTank.m_faction = GameConstants.eFaction.neutral;

            return newEnergyTank;
        }

        public static EnergyTank CreateEnergyTank(EnergyTanks dbEnergyTank)
        {
            EnergyTank newEnergyTank = new EnergyTank();

            newEnergyTank.m_room_key = new RoomKey(dbEnergyTank.GameID, dbEnergyTank.RoomX, dbEnergyTank.RoomY, dbEnergyTank.RoomZ);
            newEnergyTank.m_energy_tank_id = dbEnergyTank.EnergyTankID;
            newEnergyTank.m_position = new Point3d(dbEnergyTank.X, dbEnergyTank.Y, dbEnergyTank.Z);
            newEnergyTank.m_energy = dbEnergyTank.Energy;
            newEnergyTank.m_faction = (GameConstants.eFaction)dbEnergyTank.Ownership;

            return newEnergyTank;
        }

        public override void WriteDirtyObjectToDatabase(AsyncRPGDataContext db_context)
        {
            if (IsDirty)
            {
                EnergyTankQueries.UpdateEnergyTank(db_context, this);
                IsDirty = false;
            }
        }

        public int ID
        {
            get { return m_energy_tank_id; }
            set { m_energy_tank_id = value; }
        }

        public RoomKey RoomKey
        {
            get { return m_room_key; }
        }

        public Point3d Position
        {
            get { return m_position; }
        }

        public int Energy
        {
            get { return m_energy; }
            set { m_energy = value; IsDirty = true; }
        }

        public int Health
        {
            get { return 0; }
        }

        public GameConstants.eFaction Faction
        {
            get { return m_faction; }
            set { m_faction = value; IsDirty = true; }
        }
    }
}
