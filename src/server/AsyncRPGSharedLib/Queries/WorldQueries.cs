using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Navigation;
using LitJson;

namespace AsyncRPGSharedLib.Queries
{
    public class WorldQueries
    {
        public static WorldTemplate GetWorldGenerationParameters(
            AsyncRPGDataContext context,
            int game_id)
        {
            WorldTemplate worldTemplate = new WorldTemplate();
            Games[] game = (from g in context.Games where g.GameID == game_id select g).ToArray<Games>();

            worldTemplate.dungeon_size = (GameConstants.eDungeonSize)(game[0].DungeonSize);
            worldTemplate.dungeon_difficulty = (GameConstants.eDungeonDifficulty)(game[0].DungeonDifficulty);

            return worldTemplate;
        }

        public static bool DoesRoomExist(
            AsyncRPGDataContext context,
            RoomKey roomKey)
        {
            return 
                (from r in context.Rooms
                 where r.GameID == roomKey.game_id && r.X == roomKey.x && r.Y == roomKey.y && r.Z == roomKey.z
                 select r).Count() > 0;
        }

        public static List<Portal> GetRoomPortals(
            AsyncRPGDataContext context,
            RoomKey roomKey)
        {
            List<Portal> portals = new List<Portal>();

            var query = 
                from p in context.Portals
                where p.GameID == roomKey.game_id && p.RoomX == roomKey.x && p.RoomY == roomKey.y && p.RoomZ == roomKey.z
                select p;

            foreach (var dbPortal in query)
            {
                Portal portal = Portal.CreatePortal(dbPortal);

                portals.Add(portal);
            }

            return portals;
        }

        public static int GetRoomRandomSeed(
            AsyncRPGDataContext context,
            RoomKey roomKey)
        {
            return
                (from r in context.Rooms
                 where r.GameID == roomKey.game_id && r.X == roomKey.x && r.Y == roomKey.y && r.Z == roomKey.z
                 select r.RandomSeed).Single();
        }

        public static void UpdateRoomRandomSeed(
            AsyncRPGDataContext db_context,
            RoomKey roomKey,
            int random_seed)
        {
            var room=
                (from r in db_context.Rooms
                where r.GameID == roomKey.game_id && r.X == roomKey.x && r.Y == roomKey.y && r.Z == roomKey.z
                select r).Single();

            room.RandomSeed = random_seed;

            db_context.SubmitChanges();
        }

        public static Portal GetPortal(
            AsyncRPGDataContext context,
            int portal_id)
        {
            var dbPortal=
                (from p in context.Portals
                where p.PortalID == portal_id
                select p).Single();

            return Portal.CreatePortal(dbPortal);
        }

        public static string GetRoomStaticData(
            AsyncRPGDataContext context,
            RoomKey roomKey)
        {
            return
                (from r in context.Rooms
                 where r.GameID == roomKey.game_id && r.X == roomKey.x && r.Y == roomKey.y && r.Z == roomKey.z
                 select r.StaticData).Single();
        }

        public static void InsertWorld(
            AsyncRPGDataContext db_context,
            World world)
        {
            Dictionary<int, Portal> portalIdMap = new Dictionary<int, Portal>();
            Dictionary<int, int> portalIdRemapping = new Dictionary<int, int>();
            Dictionary<int, int> reversePortalIdRemapping = new Dictionary<int, int>();

            // Insert all the rooms and portals with their initial IDs
            {
                foreach (Room room in world.Rooms)
                {
                    // Save each room into the database
                    {
                        Rooms dbRoom = new Rooms
                        {
                            X = room.room_key.x,
                            Y = room.room_key.y,
                            Z = room.room_key.z,
                            GameID = world.GameID,
                            RandomSeed = room.random_seed,
                            StaticData = JsonMapper.ToJson(room.static_room_data)
                        };

                        db_context.Rooms.InsertOnSubmit(dbRoom);
                        db_context.SubmitChanges();
                    }

                    // Save each portal into the database
                    foreach (Portal portal in room.portals)
                    {
                        Portals dbPortal = new Portals
                        {
                            PortalType = (int)portal.portal_type,
                            TargetPortalID = -1,
                            GameID = world.GameID,
                            RoomX = portal.room_x,
                            RoomY = portal.room_y,
                            RoomZ = portal.room_z,
                            RoomSide = (int)portal.room_side,
                            BboxX0 = portal.bounding_box.Min.x,
                            BboxY0 = portal.bounding_box.Min.y,
                            BboxX1 = portal.bounding_box.Max.x,
                            BboxY1 = portal.bounding_box.Max.y
                        };

                        db_context.Portals.InsertOnSubmit(dbPortal);
                        db_context.SubmitChanges();

                        portalIdMap.Add(portal.portal_id, portal);
                        portalIdRemapping.Add(portal.portal_id, dbPortal.PortalID);
                        reversePortalIdRemapping.Add(dbPortal.PortalID, portal.portal_id);

                        // Portal ID assignment has to happen after all the portals are inserted
                    }

                    // Save each mob spawner to the database
                    foreach (MobSpawner spawner in room.mobSpawners)
                    {
                        MobSpawners dbMobSpawner = new MobSpawners
                        {
                            GameID = world.GameID,
                            RoomX = room.room_key.x,
                            RoomY = room.room_key.y,
                            RoomZ = room.room_key.z,
                            X = spawner.Position.x,
                            Y = spawner.Position.y,
                            Z = spawner.Position.z,
                            MobSpawnerTableID = spawner.SpawnTable.ID,
                            RemainingSpawnCount = spawner.RemainingSpawnCount,
                            RandomSeed = spawner.RandomSeed
                        };

                        db_context.MobSpawners.InsertOnSubmit(dbMobSpawner);
                        db_context.SubmitChanges();

                        // Assign an id once the spawner has been inserted into the DB
                        spawner.ID = dbMobSpawner.MobSpawnerID;
                    }

                    // Save each energy tank to the database
                    if (room is RoomLayout)
                    {
                        RoomLayout roomLayout = (RoomLayout)room;

                        foreach (EnergyTank energyTank in roomLayout.energyTanks)
                        {
                            EnergyTanks dbEnergyTank = new EnergyTanks
                            {
                                GameID = world.GameID,
                                RoomX = room.room_key.x,
                                RoomY = room.room_key.y,
                                RoomZ = room.room_key.z,
                                X = energyTank.Position.x,
                                Y = energyTank.Position.y,
                                Z = energyTank.Position.z,
                                Ownership = (int)energyTank.Faction,
                                Energy = energyTank.Energy
                            };

                            db_context.EnergyTanks.InsertOnSubmit(dbEnergyTank);
                            db_context.SubmitChanges();

                            // Assign an id once the energy tank has been inserted into the DB
                            energyTank.ID = dbEnergyTank.EnergyTankID;
                        }

                        // Drop the energy tanks now that we've saved them
                        roomLayout.energyTanks = null;
                    }
                }
            }

            // Fix up portal IDs 
            {
                // Fixed the cached portal ids to match the ids in the database
                var query =
                    from dbPortal in db_context.Portals
                    where dbPortal.GameID == world.GameID
                    select dbPortal;

                foreach (Portals dbPortal in query)
                {
                    // Find the cache portal id from the db portal id using the reverse mapping
                    int cachePortalId = reversePortalIdRemapping[dbPortal.PortalID];

                    // Get the cached portal using the cached portal
                    Portal cachePortal = portalIdMap[cachePortalId];

                    // Remap the cached portal's target id to be a db portal id
                    int dbTargetPortalId = portalIdRemapping[cachePortal.target_portal_id];

                    // Set the target portal id in the database
                    dbPortal.TargetPortalID = dbTargetPortalId;

                    // Fix-up the portal ids on the cached portal
                    cachePortal.portal_id = dbPortal.PortalID;
                    cachePortal.target_portal_id = dbTargetPortalId;
                }

                db_context.SubmitChanges();
            }
        }

        public static bool LoadRoomTemplates(
            string connection_string,
            out Dictionary<string, RoomTemplate> roomTemplates, 
            out string result)
        {
            bool success;

            try
            {
                AsyncRPGDataContext context = new AsyncRPGDataContext(connection_string);

                roomTemplates = LoadRoomTemplates(context);
                success = true;
                result = SuccessMessages.GENERAL_SUCCESS;
            }
            catch (System.Exception ex)
            {
                roomTemplates = new Dictionary<string, RoomTemplate>();
                success = false;
                result = ex.Message;
            }

            return success;
        }

        public static Dictionary<string, RoomTemplate> LoadRoomTemplates(
            AsyncRPGDataContext context)
        {
            Dictionary<string, RoomTemplate> roomTemplates = new Dictionary<string, RoomTemplate>();

            var templates = from t in context.RoomTemplates select t;

            foreach (RoomTemplates roomTemplateDBEntry in templates)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(roomTemplateDBEntry.XML);

                RoomTemplate roomTemplate = 
                    new RoomTemplate(
                        roomTemplateDBEntry.Name, 
                        xmlDoc,
                        roomTemplateDBEntry.CompressedNavMesh,
                        roomTemplateDBEntry.CompressedVisibility);

                roomTemplates[roomTemplateDBEntry.Name] = roomTemplate;
            }

            return roomTemplates;
        }
    }
}