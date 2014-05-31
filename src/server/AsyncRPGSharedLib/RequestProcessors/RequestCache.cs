using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Web.Interfaces;

namespace AsyncRPGSharedLib.RequestProcessors
{
    class RequestRoomCache
    {
        private RoomKey m_roomKey;
        
        private bool m_allEnergyTanksCached;
        private Dictionary<int, EnergyTank> m_energyTanks;
        
        private bool m_allMobsCached;
        private Dictionary<int, Mob> m_mobs;

        private bool m_allPlayersCached;
        private Dictionary<int, Player> m_players;

        public RequestRoomCache(RoomKey roomKey)
        {
            m_roomKey = new RoomKey(roomKey);

            m_allEnergyTanksCached = false;
            m_energyTanks = new Dictionary<int, EnergyTank>();

            m_allMobsCached= false;
            m_mobs = new Dictionary<int, Mob>();

            m_allPlayersCached = false;
            m_players = new Dictionary<int, Player>();
        }

        public RoomKey RoomKey
        {
            get { return m_roomKey; }
        }

        public IEnumerable<EnergyTank> GetEnergyTanks(AsyncRPGDataContext db_context)
        {
            if (!m_allEnergyTanksCached)
            {
                foreach (EnergyTank energyTank in EnergyTankQueries.GetEnergyTanks(db_context, m_roomKey))
                {
                    if (!m_energyTanks.ContainsKey(energyTank.ID))
                    {
                        m_energyTanks.Add(energyTank.ID, energyTank);
                    }
                }
            }

            return m_energyTanks.Values;
        }

        public EnergyTank GetEnergyTank(AsyncRPGDataContext db_context, int energyTankID)
        {
            EnergyTank energyTank= null;

            if (!m_energyTanks.TryGetValue(energyTankID, out energyTank))
            {
                energyTank = EnergyTankQueries.GetEnergyTank(db_context, energyTankID);

                m_energyTanks.Add(energyTankID, energyTank);
            }

            return energyTank;
        }

        public bool RemoveEnergyTank(int energyTankId)
        {
            bool containsEnergyTank= m_energyTanks.ContainsKey(energyTankId);

            if (containsEnergyTank)
            {
                m_energyTanks.Remove(energyTankId);
                m_allEnergyTanksCached = false;
            }

            return containsEnergyTank;
        }

        public IEnumerable<Mob> GetMobs(AsyncRPGDataContext db_context, MobTypeSet mobTypeSet)
        {
            if (!m_allMobsCached)
            {
                foreach (Mob mob in MobQueries.GetMobs(db_context, mobTypeSet, m_roomKey))
                {
                    if (!m_mobs.ContainsKey(mob.ID))
                    {
                        m_mobs.Add(mob.ID, mob);
                    }
                }
            }

            return m_mobs.Values;
        }

        public Mob GetMob(AsyncRPGDataContext db_context, MobTypeSet mobTypeSet, int mobID)
        {
            Mob mob = null;

            if (!m_mobs.TryGetValue(mobID, out mob))
            {
                mob = MobQueries.GetMob(db_context, mobTypeSet, mobID);

                m_mobs.Add(mobID, mob);
            }

            return mob;
        }

        public void AddMob(Mob mob)
        {
            m_mobs.Add(mob.ID, mob);
        }

        public bool RemoveMob(int mobId)
        {
            bool containsMob = m_mobs.ContainsKey(mobId);

            if (containsMob)
            {
                m_mobs.Remove(mobId);
                m_allMobsCached = false;
            }

            return containsMob;
        }

        public IEnumerable<Player> GetPlayers(AsyncRPGDataContext db_context)
        {
            if (!m_allPlayersCached)
            {
                foreach (Player player in CharacterQueries.GetPlayers(db_context, m_roomKey))
                {
                    if (!m_players.ContainsKey(player.ID))
                    {
                        m_players.Add(player.ID, player);
                    }
                }
            }

            return m_players.Values;
        }

        public Player GetPlayer(AsyncRPGDataContext db_context, int playerID)
        {
            Player player = null;

            if (!m_players.TryGetValue(playerID, out player))
            {
                player = CharacterQueries.GetPlayer(db_context, playerID);

                m_players.Add(playerID, player);
            }

            return player;
        }

        public void AddPlayer(Player player)
        {
            m_players.Add(player.ID, player);
        }

        public bool RemovePlayer(int playerId)
        {
            bool containsPlayer = m_players.ContainsKey(playerId);

            if (containsPlayer)
            {
                m_players.Remove(playerId);
                m_allPlayersCached = false;
            }

            return containsPlayer;
        }

        public void WriteDirtyObjectsToDatabase(AsyncRPGDataContext dbContext)
        {
            foreach (Mob mob in m_mobs.Values)
            {
                mob.WriteDirtyObjectToDatabase(dbContext);
            }

            foreach (Player player in m_players.Values)
            {
                player.WriteDirtyObjectToDatabase(dbContext);
            }

            foreach (EnergyTank energyTank in m_energyTanks.Values)
            {
                energyTank.WriteDirtyObjectToDatabase(dbContext);
            }
        }
    }

    public class RequestCache
    {
        private ICacheAdapter m_sessionCache;
        private AsyncRPGDataContext m_dbContext;
        private Dictionary<RoomKey, RequestRoomCache> m_requestRoomCache;

        public RequestCache(
            ICacheAdapter sessionCache,
            AsyncRPGDataContext dbContext)
        {
            m_sessionCache = sessionCache;
            m_dbContext = dbContext;
            m_requestRoomCache = null;
        }

        public ICacheAdapter SessionCache
        {
            get { return m_sessionCache; }
        }

        public AsyncRPGDataContext DatabaseContext
        {
            get { return m_dbContext; }
        }

        private RequestRoomCache GetRequestRoomCache(
            RoomKey roomKey)
        {
            RequestRoomCache requestRoomCache = null;

            if (m_requestRoomCache != null)
            {
                if (!m_requestRoomCache.TryGetValue(roomKey, out requestRoomCache))
                {
                    requestRoomCache = new RequestRoomCache(roomKey);

                    m_requestRoomCache.Add(requestRoomCache.RoomKey, requestRoomCache);
                }
            }
            else
            {
                requestRoomCache = new RequestRoomCache(roomKey);

                m_requestRoomCache= new Dictionary<RoomKey, RequestRoomCache>();
                m_requestRoomCache.Add(requestRoomCache.RoomKey, requestRoomCache);
            }

            return requestRoomCache;
        }

        public World GetWorld(
            int game_id)
        {
            return WorldCache.GetWorld(m_dbContext, m_sessionCache, game_id);
        }

        public MobTypeSet GetMobTypeSet(
            int game_id)
        {
            return GetWorld(game_id).MobTypes;
        }

        public IEnumerable<EnergyTank> GetEnergyTanks(
            RoomKey roomKey)
        {
            return GetRequestRoomCache(roomKey).GetEnergyTanks(m_dbContext);
        }

        public EnergyTank GetEnergyTank(
            RoomKey roomKey,
            int energyTankID)
        {
            return GetRequestRoomCache(roomKey).GetEnergyTank(m_dbContext, energyTankID);
        }

        public bool RemoveEnergyTank(
            RoomKey roomKey,
            int energyTankId)
        {
            return GetRequestRoomCache(roomKey).RemoveEnergyTank(energyTankId);
        }

        public IEnumerable<Mob> GetMobs(
            RoomKey roomKey)
        {
            return GetRequestRoomCache(roomKey).GetMobs(m_dbContext, GetMobTypeSet(roomKey.game_id));
        }

        public Mob GetMob(
            RoomKey roomKey, 
            int mobID)
        {
            return GetRequestRoomCache(roomKey).GetMob(m_dbContext, GetMobTypeSet(roomKey.game_id), mobID);
        }

        public void AddMob(
            Mob mob)
        {
            Debug.Assert(mob.ID >= 0);

            GetRequestRoomCache(mob.RoomKey).AddMob(mob);
        }

        public bool RemoveMob(
            RoomKey roomKey,
            int mobID)
        {
            return GetRequestRoomCache(roomKey).RemoveMob(mobID);
        }

        public IEnumerable<Player> GetPlayers(
            RoomKey roomKey)
        {
            return GetRequestRoomCache(roomKey).GetPlayers(m_dbContext);
        }

        public Player GetPlayer(
            RoomKey roomKey,
            int playerID)
        {
            return GetRequestRoomCache(roomKey).GetPlayer(m_dbContext, playerID);
        }

        public bool RemovePlayer(
            RoomKey roomKey,
            int playerID)
        {
            return GetRequestRoomCache(roomKey).RemovePlayer(playerID);
        }

        public bool MovePlayer(
            RoomKey fromRoomKey,
            RoomKey toRoomKey,
            Point3d toRoomPosition,
            float toRoomAngle,
            int playerID)
        {
            bool success = false;
            RequestRoomCache fromRoom = GetRequestRoomCache(fromRoomKey);
            Player player = fromRoom.GetPlayer(m_dbContext, playerID);

            if (player != null)
            {
                if (fromRoomKey != toRoomKey)
                {
                    fromRoom.RemovePlayer(playerID);
                    GetRequestRoomCache(toRoomKey).AddPlayer(player);

                    player.RoomKey.Set(toRoomKey);
                }

                player.Position.Set(toRoomPosition);
                player.Angle = toRoomAngle;

                success = true;
            }

            return success;
        }

        public void WriteDirtyObjectsToDatabase()
        {
            if (m_requestRoomCache != null)
            {
                foreach (RequestRoomCache roomCache in m_requestRoomCache.Values)
                {
                    roomCache.WriteDirtyObjectsToDatabase(m_dbContext);
                }
            }
        }
    }
}
