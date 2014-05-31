using System;
using System.Linq;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class RoomRequestProcessor : RequestProcessor
    {
        // Request Data
        private RoomKey m_room_key;

        // Processor Data
        private World m_world;

        // Result Data
        private Point3d m_room_world_position;
        private PortalEntry[] m_portals;
        private MobState[] m_mobs;
        private EnergyTankState[] m_energyTanks;
        private StaticRoomData m_staticRoomData;

        public RoomRequestProcessor(
            RoomKey roomKey)
        {
            m_room_key = new RoomKey(roomKey);
            m_world = null;
        }

        public RoomKey RoomKey
        {
            get { return m_room_key; }
        }

        public Point3d RoomWorldPosition
        {
            get { return m_room_world_position; }
        }

        public PortalEntry[] Portals
        {
            get { return m_portals; }
        }

        public MobState[] Mobs
        {
            get { return m_mobs; }
        }

        public EnergyTankState[] EnergyTanks
        {
            get { return m_energyTanks; }
        }

        public StaticRoomData StaticRoomData
        {
            get { return m_staticRoomData; }
        }

        override protected bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = true;

            result_code = SuccessMessages.GENERAL_SUCCESS;

            success = LookupWorld(requestCache, m_room_key.game_id, out m_world, out result_code);

            if (success)
            {
                success = LookupRoom(requestCache, out result_code);
            }

            if (success)
            {
                LookupMobs(requestCache);
                LookupEnergyTanks(requestCache);
            }

            return success;
        }

        private bool LookupRoom(
            RequestCache requestCache,
            out string result_code)
        {
            // Get the room data for the room that the player is currently in
            return
                m_world.GetRoom(
                    requestCache.DatabaseContext,
                    m_room_key,
                    out m_room_world_position,
                    out m_portals,
                    out m_staticRoomData,
                    out result_code);
        }

        private void LookupMobs(
            RequestCache requestCache)
        {
            int mobCount = requestCache.GetMobs(m_room_key).Count();
            int mobIndex = 0;

            m_mobs = new MobState[mobCount];
            foreach (Mob mob in requestCache.GetMobs(m_room_key))
            {
                m_mobs[mobIndex] = new MobState
                {
                    mob_id = mob.ID,
                    mob_type_name = mob.MobType.Name,
                    health = mob.Health,
                    energy = mob.Energy,
                    game_id = mob.RoomKey.game_id,
                    room_x = mob.RoomKey.x,
                    room_y = mob.RoomKey.y,
                    room_z = mob.RoomKey.z,
                    x = mob.Position.x,
                    y = mob.Position.y,
                    z = mob.Position.z,
                    angle = mob.Angle
                };

                ++mobIndex;
            }
        }

        private void LookupEnergyTanks(
            RequestCache requestCache)
        {
            int energyTankCount = requestCache.GetEnergyTanks(m_room_key).Count();
            int energyTankIndex = 0;

            m_energyTanks = new EnergyTankState[energyTankCount];
            foreach (EnergyTank energyTank in requestCache.GetEnergyTanks(m_room_key))
            {
                m_energyTanks[energyTankIndex] = new EnergyTankState
                {
                    energy_tank_id = energyTank.ID,
                    energy = energyTank.Energy,
                    ownership = (int)energyTank.Faction,
                    room_x = energyTank.RoomKey.x,
                    room_y = energyTank.RoomKey.y,
                    room_z = energyTank.RoomKey.z,
                    x = energyTank.Position.x,
                    y = energyTank.Position.y
                };

                ++energyTankIndex;
            }
        }
    }
}
