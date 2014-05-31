using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGDBTool
{
    class DungeonValidator
    {
        private TextWriter _logger;

        public DungeonValidator(TextWriter logger)
        {
            _logger = logger;
        }

        public bool ValidateDungeons(Command command)
        {
            bool success = true;
            string result= SuccessMessages.GENERAL_SUCCESS;

            RoomTemplateSet roomTemplateSet = new RoomTemplateSet();
            MobTypeSet mobTypeSet = new MobTypeSet();
            MobSpawnTableSet mobSpawnTableSet = new MobSpawnTableSet();
            int game_id_min = 0;
            int game_id_max = 100000; // Int32.MaxValue; This will take ~100 days to finish all 2 billion  dungeons
            string connection_string = "";

            string dumpGeometryPath = "";

            if (command.HasArgumentWithName("C"))
            {
                connection_string = command.GetTypedArgumentByName<CommandArgument_String>("C").ArgumentValue;
            }
            else
            {
                _logger.WriteLine("DungeonValidator: Missing expected connection string parameter");
                success = false;
            }

            if (command.HasArgumentWithName("G"))
            {
                game_id_min = command.GetTypedArgumentByName<CommandArgument_Int32>("G").ArgumentValue;
                game_id_max = game_id_min;
            }
            else
            {
                _logger.WriteLine("DungeonValidator: No game id given, evaluating all possible game ids");
            }

            if (game_id_min == game_id_max && command.HasArgumentWithName("D"))
            {
                dumpGeometryPath = command.GetTypedArgumentByName<CommandArgument_String>("D").ArgumentValue;
            }

            _logger.WriteLine("Validating layouts for game_ids {0} to {1}", game_id_min, game_id_max);

            // Get the room templates from the DB
            if (success && !roomTemplateSet.Initialize(connection_string, out result))
            {
                _logger.WriteLine(string.Format("DungeonValidator: Failed to load the room templates from the DB: {0}", result));
                success = false;
            }

            // Get the mob type set from the DB
            if (success && !mobTypeSet.Initialize(connection_string, out result))
            {
                _logger.WriteLine(string.Format("DungeonValidator: Failed to load the mob types from the DB: {0}", result));
                success = false;
            }

            // Get the mob spawn templates from the DB
            if (success && !mobSpawnTableSet.Initialize(connection_string, mobTypeSet, out result))
            {
                _logger.WriteLine(string.Format("DungeonValidator: Failed to load the mob spawn tables from the DB: {0}", result));
                success = false;
            }

            if (success)
            {
                DateTime startTime = DateTime.Now;

                // Test all possible world size configurations for each desired game id
                WorldTemplate[] worldTemplates = new WorldTemplate[] { 
                    new WorldTemplate(GameConstants.eDungeonSize.small, GameConstants.eDungeonDifficulty.normal), 
                    new WorldTemplate(GameConstants.eDungeonSize.medium, GameConstants.eDungeonDifficulty.normal),
                    new WorldTemplate(GameConstants.eDungeonSize.large, GameConstants.eDungeonDifficulty.normal),
                };

                for (int game_id = game_id_min; success && game_id <= game_id_max; ++game_id)
                {
                    foreach (WorldTemplate worldTemplate in worldTemplates)
                    {
                        DungeonLayout layout = new DungeonLayout(game_id, worldTemplate, roomTemplateSet, mobSpawnTableSet);

                        // Create the initial set of rooms for the world
                        if (!layout.BuildRoomLayout(out result))
                        {
                            _logger.WriteLine(
                                string.Format("DungeonValidator: Failed to generate dungeon layout, game_id:{0}, size:{1}", 
                                game_id, worldTemplate.dungeon_size));
                            _logger.WriteLine(result);
                            success = false;
                        }

                        // Verify that this is a valid dungeon
                        if (success)
                        {
                            Dictionary<int, Portal> portalIdToPortalMap = BuildPortalIdMap(layout);

                            // Verify that all portals are connected correctly
                            success &= VerifyRoomPortals(layout, portalIdToPortalMap);

                            // Verify that every room is accessible to every other room
                            success &= VerifyRoomAccessibility(layout, portalIdToPortalMap);

                            // Verify monster spawners
                            success &= VerifyMobSpawners(layout);
                        }

                        // Dump the generated layout to a .obj file
                        if (dumpGeometryPath.Length > 0)
                        {
                            DumpLayoutGeometry(dumpGeometryPath, layout);
                        }
                    }

                    if (game_id_max > game_id_min)
                    {
                        if ((game_id % 1000) == 0)
                        {
                            TimeSpan elapsed = DateTime.Now.Subtract(startTime);

                            int percent_complete = 100 * (game_id - game_id_min) / (game_id_max - game_id_min);

                            _logger.Write("\r[{0:c}] {1}/{2} {3}%", 
                                elapsed,
                                game_id-game_id_min, 
                                game_id_max-game_id_min, 
                                percent_complete);
                        }
                    }
                }

                // Write out a new line after the timing info
                _logger.WriteLine();
            }

            return success;
        }

        private Dictionary<int, Portal> BuildPortalIdMap(
            DungeonLayout layout)
        {
            Dictionary<int, Portal> portalIdToPortalMap = new Dictionary<int, Portal>();

            // Generate a mapping from portal id to portal
            for (DungeonLayout.RoomIndexIterator iterator = new DungeonLayout.RoomIndexIterator(layout.RoomGrid);
                iterator.Valid;
                iterator.Next())
            {
                DungeonLayout.RoomIndex roomIndex = iterator.Current;
                Room room = layout.GetRoomByIndex(roomIndex);

                // Add all of the portals to the map
                foreach (Portal portal in room.portals)
                {
                    portalIdToPortalMap.Add(portal.portal_id, portal);
                }
            }

            return portalIdToPortalMap;
        }

        private bool VerifyRoomPortals(
            DungeonLayout layout,
            Dictionary<int, Portal> portalIdToPortalMap)
        {
            bool success = true;
            MathConstants.eSignedDirection[] roomSides = new MathConstants.eSignedDirection[] {
                MathConstants.eSignedDirection.positive_x,
                MathConstants.eSignedDirection.negative_x,
                MathConstants.eSignedDirection.positive_y,
                MathConstants.eSignedDirection.negative_y,
                MathConstants.eSignedDirection.positive_z,
                MathConstants.eSignedDirection.negative_z
            };

            for (DungeonLayout.RoomIndexIterator iterator = new DungeonLayout.RoomIndexIterator(layout.RoomGrid);
                iterator.Valid;
                iterator.Next())
            {
                DungeonLayout.RoomIndex roomIndex = iterator.Current;
                Room room = layout.GetRoomByIndex(roomIndex);

                // Verify the the portal bitmask matches the existing portals
                foreach (MathConstants.eSignedDirection roomSide in roomSides)
                {
                    if (room.RoomHasPortalOnSide(roomSide))
                    {
                        if (room.portals.Count(p => p.room_side == roomSide) == 0)
                        {
                            _logger.WriteLine("DungeonValidator: FAILED: Expected portal on room side {0} but found none!", roomSide);
                            _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                            _logger.WriteLine(string.Format("  room_key: {0},{1},{2}",
                                room.room_key.x, room.room_key.y, room.room_key.z));
                            success = false;
                            break;
                        }
                    }
                    else
                    {
                        if (room.portals.Count(p => p.room_side == roomSide) != 0)
                        {
                            _logger.WriteLine("DungeonValidator: FAILED: Expected no portal on room side {0} but found one!", roomSide);
                            _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                            _logger.WriteLine(string.Format("  room_key: {0},{1},{2}",
                                room.room_key.x, room.room_key.y, room.room_key.z));
                            success = false;
                            break;
                        }
                    }
                }

                // Verify that the portals are the right expected type
                foreach (Portal portal in room.portals)
                {
                    MathConstants.eSignedDirection roomSide = portal.room_side;

                    switch (portal.portal_type)
                    {
                        case ePortalType.door:
                            if (roomSide != MathConstants.eSignedDirection.positive_x && roomSide != MathConstants.eSignedDirection.negative_x &&
                                roomSide != MathConstants.eSignedDirection.positive_y && roomSide != MathConstants.eSignedDirection.negative_y)
                            {
                                _logger.WriteLine(
                                    string.Format(
                                    "DungeonValidator: FAILED: Door portal id={0} on unexpected side={1}",
                                    portal.portal_id, roomSide));
                                _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                                _logger.WriteLine(string.Format("  room_key: {0},{1},{2}",
                                    room.room_key.x, room.room_key.y, room.room_key.z));
                                success = false;
                            }
                            break;
                        case ePortalType.stairs:
                            if (roomSide != MathConstants.eSignedDirection.positive_z && roomSide != MathConstants.eSignedDirection.negative_z)
                            {
                                _logger.WriteLine(
                                    string.Format(
                                    "DungeonValidator: FAILED: Stairs portal id={0} on unexpected side={1}",
                                    portal.portal_id, roomSide));
                                _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                                _logger.WriteLine(string.Format("  room_key: {0},{1},{2}",
                                    room.room_key.x, room.room_key.y, room.room_key.z));
                                success = false;
                            }
                            break;
                        case ePortalType.teleporter:
                            if (roomSide != MathConstants.eSignedDirection.none)
                            {
                                _logger.WriteLine(
                                    string.Format(
                                    "DungeonValidator: FAILED: Teleporter portal id={0} on unexpected side={1}",
                                    portal.portal_id, roomSide));
                                _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                                _logger.WriteLine(string.Format("  room_key: {0},{1},{2}",
                                    room.room_key.x, room.room_key.y, room.room_key.z));
                                success = false;
                            }
                            break;
                    }
                }

                // Verify that the portals connect to a valid target portal
                foreach (Portal portal in room.portals)
                {
                    if (portalIdToPortalMap.ContainsKey(portal.target_portal_id))
                    {
                        Portal targetPortal = portalIdToPortalMap[portal.target_portal_id];

                        if (targetPortal.target_portal_id != portal.portal_id)
                        {
                            _logger.WriteLine("DungeonValidator: FAILED: Target Portal is not connected to a valid portal!");
                            _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                            _logger.WriteLine(string.Format("  room_key: {0},{1},{2}",
                                room.room_key.x, room.room_key.y, room.room_key.z));
                            success = false;
                            break;
                        }
                    }
                    else
                    {
                        _logger.WriteLine("DungeonValidator: FAILED: Portal is not connected to a valid target portal!");
                        _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                        _logger.WriteLine(string.Format("  room_key: {0},{1},{2}",
                            room.room_key.x, room.room_key.y, room.room_key.z));
                        success = false;
                        break;
                    }
                }
            }

            return success;
        }

        private bool VerifyRoomAccessibility(
            DungeonLayout layout,
            Dictionary<int, Portal> portalIdToPortalMap)
        {
            UnionFind<RoomKey> roomUnion = new UnionFind<RoomKey>();
            RoomKey targetRoomKey = new RoomKey();
            bool success = true;

            // Do a first pass over the rooms to fill out the union and the portal map
            for (DungeonLayout.RoomIndexIterator iterator = new DungeonLayout.RoomIndexIterator(layout.RoomGrid);
                iterator.Valid;
                iterator.Next())
            {
                DungeonLayout.RoomIndex roomIndex = iterator.Current;
                Room room = layout.GetRoomByIndex(roomIndex);

                // Add the room to the union set
                roomUnion.AddElement(room.room_key);
            }

            // Union together all of the rooms connected by portals
            for (DungeonLayout.RoomIndexIterator iterator = new DungeonLayout.RoomIndexIterator(layout.RoomGrid);
                iterator.Valid;
                iterator.Next())
            {
                DungeonLayout.RoomIndex roomIndex = iterator.Current;
                Room room = layout.GetRoomByIndex(roomIndex);

                foreach (Portal portal in room.portals)
                {
                    Portal targetPortal= portalIdToPortalMap[portal.target_portal_id];

                    targetRoomKey.game_id= layout.GameID;
                    targetRoomKey.x= targetPortal.room_x;
                    targetRoomKey.y= targetPortal.room_y;
                    targetRoomKey.z= targetPortal.room_z;

                    roomUnion.Union(room.room_key, targetRoomKey);
                }
            }

            // Verify that all rooms share the same connectivity id
            int sharedConnectivityId = -1;
            for (DungeonLayout.RoomIndexIterator iterator = new DungeonLayout.RoomIndexIterator(layout.RoomGrid);
                iterator.Valid;
                iterator.Next())
            {
                DungeonLayout.RoomIndex roomIndex = iterator.Current;
                Room room = layout.GetRoomByIndex(roomIndex);

                int roomConnectivityId= roomUnion.FindRootIndex(room.room_key);

                if (sharedConnectivityId != -1)
                {
                    if (sharedConnectivityId != roomConnectivityId)
                    {
                        _logger.WriteLine("DungeonValidator: FAILED: Found room not connected to other rooms in dungeon!");
                        _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                        _logger.WriteLine(string.Format("  room_key: {0},{1},{2}", 
                            room.room_key.x, room.room_key.y, room.room_key.z));
                        success= false;
                        break;
                    }
                }
                else
                {
                    sharedConnectivityId= roomConnectivityId;
                }
            }

            return success;
        }

        private bool VerifyMobSpawners(
            DungeonLayout layout)
        {
            bool success = true;


            for (DungeonLayout.RoomIndexIterator iterator = new DungeonLayout.RoomIndexIterator(layout.RoomGrid);
                iterator.Valid;
                iterator.Next())
            {
                DungeonLayout.RoomIndex roomIndex = iterator.Current;
                Room room = layout.GetRoomByIndex(roomIndex);
                RoomTemplate roomTemplate= 
                    layout.LayoutRoomTemplateSet.GetTemplateByName(room.static_room_data.room_template_name);

                // We don't add the mob spawner to the room if we create it
                // and it doesn't have any mobs left (spawner initially has 0 mobs.
                // TODO: Add this check back in when we support non-zero min spawn count
                //if (roomTemplate.MobSpawnerTemplates.Count() != room.mobSpawners.Count)
                //{
                //    _logger.WriteLine("DungeonValidator: FAILED: Mob spawner template count != mob spawner count");
                //    _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                //    _logger.WriteLine(string.Format("  room_key: {0},{1},{2}",
                //        room.room_key.x, room.room_key.y, room.room_key.z));
                //    success = false;
                //    break;
                //}

                foreach (MobSpawner spawner in room.mobSpawners)
                {
                    if (spawner.SpawnTable == null)
                    {
                        _logger.WriteLine("DungeonValidator: FAILED: Mob spawner missing spawn table!");
                        _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                        _logger.WriteLine(string.Format("  room_key: {0},{1},{2}",
                            room.room_key.x, room.room_key.y, room.room_key.z));
                        success = false;
                        break;
                    }

                    while (spawner.RemainingSpawnCount > 0)
                    {
                        Mob mob= spawner.SpawnRandomMob();

                        if (mob == null)
                        {
                            _logger.WriteLine("DungeonValidator: FAILED: Mob spawner failed to produce valid mob!");
                            _logger.WriteLine(string.Format("  game_id: {0}", layout.GameID));
                            _logger.WriteLine(string.Format("  room_key: {0},{1},{2}",
                                room.room_key.x, room.room_key.y, room.room_key.z));
                            success = false;
                            break;
                        }
                    }
                }
            }

            return success;
        }

        private void DumpLayoutGeometry(
            string dumpGeometryPath, 
            DungeonLayout layout)
        {
            GeometryFileWriter fileWriter = new GeometryFileWriter();
            string filename= string.Format("DungeonLayout_Game{0}_Size{1}", layout.GameID, layout.LayoutWorldTemplate.dungeon_size);
            string header = string.Format("DungeonLayout for GameID:{0} DungeonSize: {1}", layout.GameID, layout.LayoutWorldTemplate.dungeon_size);
            string result = SuccessMessages.GENERAL_SUCCESS;
            Vector3d roomSize = new Vector3d(WorldConstants.ROOM_X_SIZE, WorldConstants.ROOM_Y_SIZE, WorldConstants.ROOM_Z_SIZE);

            for (DungeonLayout.RoomIndexIterator iterator = new DungeonLayout.RoomIndexIterator(layout.RoomGrid);
                iterator.Valid;
                iterator.Next())
            {
                DungeonLayout.RoomIndex roomIndex = iterator.Current;
                Room room = layout.GetRoomByIndex(roomIndex);
                AABB3d roomAABB = new AABB3d(room.world_position, room.world_position + roomSize);
                AABB3d shrunkRoomAABB = roomAABB.ScaleAboutCenter(0.5f);
                AABB3d centerAABB = roomAABB.ScaleAboutCenter(0.05f);

                // Add the AABB for this room
                fileWriter.AppendAABB(shrunkRoomAABB);

                // Create portal AABBs to all adjacent rooms
                for (MathConstants.eSignedDirection roomSide = MathConstants.eSignedDirection.first;
                    roomSide < MathConstants.eSignedDirection.count; 
                    ++roomSide)
                {
                    if (room.RoomHasPortalOnSide(roomSide))
                    {
                        DungeonLayout.RoomIndex neighborRoomIndex = null;
                        switch (roomSide)
                        {
                            case MathConstants.eSignedDirection.positive_x:
                                neighborRoomIndex = roomIndex.Offset(1, 0, 0);
                                break;
                            case MathConstants.eSignedDirection.negative_x:
                                neighborRoomIndex = roomIndex.Offset(-1, 0, 0);
                                break;
                            case MathConstants.eSignedDirection.positive_y:
                                neighborRoomIndex = roomIndex.Offset(0, 1, 0);
                                break;
                            case MathConstants.eSignedDirection.negative_y:
                                neighborRoomIndex = roomIndex.Offset(0, -1, 0);
                                break;
                            case MathConstants.eSignedDirection.positive_z:
                                neighborRoomIndex = roomIndex.Offset(0, 0, 1);
                                break;
                            case MathConstants.eSignedDirection.negative_z:
                                neighborRoomIndex = roomIndex.Offset(0, 0, -1);
                                break;
                        }

                        Room neighborRoom = layout.GetRoomByIndex(neighborRoomIndex);
                        AABB3d neighborRoomAABB = new AABB3d(neighborRoom.world_position, neighborRoom.world_position + roomSize);
                        AABB3d neighborCenterAABB = neighborRoomAABB.ScaleAboutCenter(0.05f);
                        AABB3d portalAABB = centerAABB.EncloseAABB(neighborCenterAABB);

                        fileWriter.AppendAABB(portalAABB);
                    }
                }

                // TODO: DumpLayoutGeometry: Color the rooms by teleporter pair
            }

            if (!fileWriter.SaveFile(dumpGeometryPath, filename, header, out result))
            {
                _logger.WriteLine("DungeonValidator: WARNING: Failed to save layout geometry file");
                _logger.WriteLine(result);
            }
        }
    }
}
