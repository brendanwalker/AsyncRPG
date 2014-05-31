using System;
using System.Collections.Generic;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Utility;

namespace AsyncRPGSharedLib.Environment
{
    public class WorldTemplate
    {
        public const int k_max_dungeon_lateral_room_count = 10;

        public GameConstants.eDungeonSize dungeon_size;
        public GameConstants.eDungeonDifficulty dungeon_difficulty;

        public WorldTemplate()
        {
            dungeon_size = GameConstants.eDungeonSize.small;
            dungeon_difficulty = GameConstants.eDungeonDifficulty.normal;
        }


        public WorldTemplate(GameConstants.eDungeonSize size, GameConstants.eDungeonDifficulty difficulty)
        {
            dungeon_size = size;
            dungeon_difficulty = difficulty;
        }

        public int DungeonLateralRoomCount
        {
            get
            {
                int roomCount = 0;

                switch (dungeon_size)
                {
                    case GameConstants.eDungeonSize.small:
                        roomCount = 5;
                        break;
                    case GameConstants.eDungeonSize.medium:
                        roomCount = 7;
                        break;
                    case GameConstants.eDungeonSize.large:
                        roomCount = k_max_dungeon_lateral_room_count;
                        break;
                    default:
                        break;
                }

                return roomCount;
            }
        }

        public int DungeonFloorCount
        {
            get
            {
                int roomCount = 0;

                switch (dungeon_size)
                {
                    case GameConstants.eDungeonSize.small:
                        roomCount = 2;
                        break;
                    case GameConstants.eDungeonSize.medium:
                        roomCount = 3;
                        break;
                    case GameConstants.eDungeonSize.large:
                        roomCount = 4;
                        break;
                    default:
                        break;
                }

                return roomCount;
            }
        }

        public Range<float> DungeonRoomDensity
        {
            get
            {
                Range<float> density = null;

                switch (dungeon_size)
                {
                    case GameConstants.eDungeonSize.small:
                        density = new Range<float>(.5f, 0.75f);
                        break;
                    case GameConstants.eDungeonSize.medium:
                        density = new Range<float>(.6f, 0.75f);
                        break;
                    case GameConstants.eDungeonSize.large:
                        density = new Range<float>(.6f, 0.8f);
                        break;
                    default:
                        break;
                }

                return density;
            }
        }

        public Range<int> StairsPerFloor // Tuple of (min, max)
        {
            get
            {
                return new Range<int>(1, 5);
            }
        }

        public int MaxSpawnsPerRoomEntry
        {
            get
            {
                int max_spawns = 0;

                switch (dungeon_difficulty)
                {
                    case GameConstants.eDungeonDifficulty.easy:
                        max_spawns = 1;
                        break;
                    case GameConstants.eDungeonDifficulty.normal:
                        max_spawns = 3;
                        break;
                    case GameConstants.eDungeonDifficulty.hard:
                        max_spawns = 5;
                        break;
                }

                return max_spawns;
            }
        }
    }

    public class World
    {
        private int m_game_id;
        private Dictionary<RoomKey, Room> m_rooms;
        private WorldBuilder m_world_builder;
        private WorldTemplate m_world_template;

        public World(WorldBuilder world_builder, WorldTemplate template, int game_id)
        {
            m_game_id = game_id;
            m_world_builder = world_builder;
            m_world_template = template;
            m_rooms = new Dictionary<RoomKey, Room>(new RoomKeyEqualityComparer());            
        }

        public int GameID
        {
            get { return m_game_id; }
        }

        public IEnumerable<RoomKey> RoomKeys
        {
            get { return m_rooms.Keys; }
        }

        public IEnumerable<Room> Rooms
        {
            get { return m_rooms.Values; }
        }

        public WorldTemplate WorldTemplate
        {
            get { return m_world_template; }
        }

        public RoomTemplateSet RoomTemplates
        {
            get { return m_world_builder.RoomTemplates; }
        }

        public MobTypeSet MobTypes
        {
            get { return m_world_builder.MobTypes; }
        }

        public MobSpawnTableSet MobSpawnTables
        {
            get { return m_world_builder.MobSpawnTables; }
        }

        public void ApplyLayout(
            DungeonLayout layout)
        {
            // Copy over all the rooms in the layout into the world
            for (DungeonLayout.RoomIndexIterator iterator = new DungeonLayout.RoomIndexIterator(layout.RoomGrid);
                iterator.Valid;
                iterator.Next())
            {
                DungeonLayout.RoomIndex roomIndex = iterator.Current;
                RoomLayout room = layout.GetRoomByIndex(roomIndex);
                RoomTemplate roomTemplate= layout.LayoutRoomTemplateSet.GetTemplateByName(room.static_room_data.room_template_name);

                room.runtime_nav_mesh = new NavMesh(room.room_key, roomTemplate.NavMeshTemplate);

                m_rooms.Add(room.room_key, room);
            }
        }

        public bool GetRoom(
            AsyncRPGDataContext context,
            RoomKey room_key,
            out Room room, 
            out string result)
        {
            bool success = true;

            room = null;
            result = SuccessMessages.GENERAL_SUCCESS;

            if (m_rooms.ContainsKey(room_key))
            {
                room = m_rooms[room_key];
            }
            else
            {

                if (!WorldQueries.DoesRoomExist(context, room_key))
                {
                    room = null;
                    success = false;
                    result = ErrorMessages.INVALID_ROOM;
                }

                if (success && 
                    !Room.LoadRoom(context, this, room_key, out room, out result))
                {
                    result = ErrorMessages.DB_ERROR;
                    success = false;
                }

                // If we got a room, update the cache
                if (success)
                {
                    m_rooms[room_key] = room;
                }
            }

            return success;
        }

        public bool GetRoom(
            AsyncRPGDataContext context,
            RoomKey roomKey,
            out Point3d world_position,
            out PortalEntry[] portals,
            out StaticRoomData staticRoomData,
            out string result)
        {
            Room cached_room = null;

            bool success =
                GetRoom(
                    context,
                    roomKey,
                    out cached_room,
                    out result);

            if (success && cached_room != null)
            {
                world_position= new Point3d(cached_room.world_position);

                portals = new PortalEntry[cached_room.portals.Count];
                for (int portal_index = 0; portal_index < cached_room.portals.Count; ++portal_index)
                {
                    portals[portal_index] = new PortalEntry();
                    portals[portal_index].id = cached_room.portals[portal_index].portal_id;
                    portals[portal_index].target_portal_id = cached_room.portals[portal_index].target_portal_id;
                    portals[portal_index].x0 = cached_room.portals[portal_index].bounding_box.Min.x;
                    portals[portal_index].y0 = cached_room.portals[portal_index].bounding_box.Min.y;
                    portals[portal_index].x1 = cached_room.portals[portal_index].bounding_box.Max.x;
                    portals[portal_index].y1 = cached_room.portals[portal_index].bounding_box.Max.y;
                }

                staticRoomData = cached_room.static_room_data;
            }
            else
            {
                world_position = new Point3d();
                portals = null;
                staticRoomData = null;
            }

            return success;
        }

        public bool HasRoomCached(RoomKey room_key)
        {
            return m_rooms.ContainsKey(room_key);
        }

        public Room GetCachedRoom(RoomKey room_key)
        {
            return m_rooms[room_key];
        }
    }
}