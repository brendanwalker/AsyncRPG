using System;
using System.Collections.Generic;
using System.Xml;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using LitJson;

namespace AsyncRPGSharedLib.Environment
{
    public class RoomTemplate
    {
        private string m_templateName;
        private NavMesh m_navMesh;
        private List<TreasureTemplate> m_treasures;
        private List<TrapTemplate> m_traps;
        private List<MobSpawnerTemplate> m_mobSpawners;
        private List<PortalTemplate> m_portals;
        private List<EnergyTankTemplate> m_energyTanks;
        private TypedFlags<MathConstants.eSignedDirection> m_portalRoomSideBitmask;

        private static TypedFlags<MathConstants.eSignedDirection> kAllPortalRoomSides =
            new TypedFlags<MathConstants.eSignedDirection>(
                TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x) |
                TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x) |
                TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y) |
                TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y) |
                TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_z) |
                TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_z)
            );

        public bool IsFullyConnected
        {
            get
            {
                return m_portalRoomSideBitmask == kAllPortalRoomSides;
            }
        }

        public TypedFlags<MathConstants.eSignedDirection> PortalRoomSideBitmask
        {
            get
            {
                return m_portalRoomSideBitmask;
            }
        }

        public NavMesh NavMeshTemplate
        {
            get { return m_navMesh; }
        }

        public IEnumerable<PortalTemplate> PortalTemplates
        {
            get { return m_portals; }
        }

        public IEnumerable<TreasureTemplate> TreasureTemplates
        {
            get { return m_treasures; }
        }

        public IEnumerable<TrapTemplate> TrapTemplates
        {
            get { return m_traps; }
        }

        public IEnumerable<MobSpawnerTemplate> MobSpawnerTemplates
        {
            get { return m_mobSpawners; }
        }

        public IEnumerable<EnergyTankTemplate> EnergyTankTemplates
        {
            get { return m_energyTanks; }
        }

        public string TemplateName
        {
            get { return m_templateName; }
        }

        public RoomTemplate(string templateName, XmlDocument xmlDoc)
        {
            m_templateName = templateName;
            m_treasures = new List<TreasureTemplate>();
            m_traps = new List<TrapTemplate>();
            m_mobSpawners = new List<MobSpawnerTemplate>();
            m_portals = new List<PortalTemplate>();
            m_energyTanks = new List<EnergyTankTemplate>();
            m_portalRoomSideBitmask = new TypedFlags<MathConstants.eSignedDirection>();

            m_navMesh = NavMesh.FromNavMeshXML(xmlDoc.SelectSingleNode("/level/NavMesh"));
            ParseEntities(xmlDoc.SelectSingleNode("/level/Entities"));
        }

        public RoomTemplate(string templateName, XmlDocument xmlDoc, byte[] compressedNavCells, byte[] compressedPVS)
        {
            m_templateName = templateName;
            m_treasures = new List<TreasureTemplate>();
            m_traps = new List<TrapTemplate>();
            m_mobSpawners = new List<MobSpawnerTemplate>();
            m_portals = new List<PortalTemplate>();
            m_energyTanks = new List<EnergyTankTemplate>();
            m_portalRoomSideBitmask = new TypedFlags<MathConstants.eSignedDirection>();

            m_navMesh = NavMesh.FromCompressedNavMeshData(compressedNavCells, compressedPVS);
            ParseEntities(xmlDoc.SelectSingleNode("/level/Entities"));
        }

        private void ParseEntities(XmlNode entityXml)
        {            
            foreach (XmlNode energyTankNode in entityXml.SelectNodes("EnergyTank"))
            {
                m_energyTanks.Add(new EnergyTankTemplate(energyTankNode));
            }

            foreach (XmlNode treasureNode in entityXml.SelectNodes("Treasure"))
            {
                m_treasures.Add(new TreasureTemplate(treasureNode));
            }

            foreach (XmlNode trapNode in entityXml.SelectNodes("Trap"))
            {
                m_traps.Add(new TrapTemplate(trapNode));
            }

            foreach (XmlNode mobSpawnerNode in entityXml.SelectNodes("MobSpawner"))
            {
                m_mobSpawners.Add(new MobSpawnerTemplate(mobSpawnerNode));
            }

            m_portalRoomSideBitmask.Clear();
            foreach (XmlNode portalNode in entityXml.SelectNodes("Portal"))
            {
                PortalTemplate portalTemplate = new PortalTemplate(portalNode);

                m_portals.Add(portalTemplate);
                if (portalTemplate.PortalRoomSide != MathConstants.eSignedDirection.none)
                {
                    m_portalRoomSideBitmask.Set(portalTemplate.PortalRoomSide, true);
                }
            }
        }
    }

    public class RoomTemplateSet
    {
        private Dictionary<string, RoomTemplate> m_roomTemplates;

        public RoomTemplateSet()
        {
            m_roomTemplates = new Dictionary<string, RoomTemplate>();
        }

        public bool Initialize(string connectionString, out string result)
        {
            return WorldQueries.LoadRoomTemplates(connectionString, out m_roomTemplates, out result);
        }

        public void Initialize(AsyncRPGDataContext db_context)
        {
            m_roomTemplates = WorldQueries.LoadRoomTemplates(db_context);
        }

        public int Count
        {
            get 
            {
                return m_roomTemplates.Values.Count;
            }
        }

        public IEnumerator<RoomTemplate> RoomTemplates
        {
            get
            {
                return m_roomTemplates.Values.GetEnumerator();
            }
        }

        public Dictionary<string, RoomTemplate> RoomTemplateDictionary
        {
            get
            {
                return m_roomTemplates;
            }
        }

        public RoomTemplate GetTemplateByName(string templateName)
        {
            return m_roomTemplates[templateName];
        }

        public void GetTemplatesWithPortalBitmask(
            TypedFlags<MathConstants.eSignedDirection> portalBitmask, 
            List<RoomTemplate> out_templates)
        {
            foreach (RoomTemplate roomTemplate in m_roomTemplates.Values)
            {
                if (roomTemplate.PortalRoomSideBitmask == portalBitmask)
                {
                    out_templates.Add(roomTemplate);
                }
            }
        }
    }

    public class Room
    {
        public Point3d world_position; // World space coordinates of the lower-left hand corner (min corner)
        public RoomKey room_key;             // World room indices
        public List<Portal> portals;
        public List<MobSpawner> mobSpawners;
        public TypedFlags<MathConstants.eSignedDirection> portalRoomSideBitmask;
        public int connectivity_id;
        public int random_seed;
        public StaticRoomData static_room_data;
        public NavMesh runtime_nav_mesh;

        public Room(RoomKey rk)
        {
            room_key = rk;
            random_seed = rk.GetHashCode();
            world_position.Set(
                (float)room_key.x * WorldConstants.ROOM_X_SIZE,
                (float)room_key.y * WorldConstants.ROOM_Y_SIZE,
                (float)room_key.z * WorldConstants.ROOM_Z_SIZE);
            portalRoomSideBitmask = new TypedFlags<MathConstants.eSignedDirection>();
            connectivity_id= -1;

            portals = new List<Portal>();
            mobSpawners = new List<MobSpawner>();
            static_room_data = new StaticRoomData();
            static_room_data.room_template_name = "";
            static_room_data.objects = null;
            runtime_nav_mesh = new NavMesh();
        }

        public bool RoomHasStairs
        {
            get
            {
                // This room has stairs if it has a portal leading up or down
                return
                    portalRoomSideBitmask.Test(MathConstants.eSignedDirection.positive_z) ||
                    portalRoomSideBitmask.Test(MathConstants.eSignedDirection.negative_z);
            }
        }

        public bool RoomHasAllPossibleDoors
        {
            get
            {
                return
                    portalRoomSideBitmask.Test(MathConstants.eSignedDirection.positive_x) &&
                    portalRoomSideBitmask.Test(MathConstants.eSignedDirection.negative_x) &&
                    portalRoomSideBitmask.Test(MathConstants.eSignedDirection.positive_y) &&
                    portalRoomSideBitmask.Test(MathConstants.eSignedDirection.negative_y);
            }
        }

        public bool RoomIsIsolated
        {
            get
            {
                return portalRoomSideBitmask.IsEmpty();
            }
        }

        public bool RoomHasPortalOnSide(MathConstants.eSignedDirection side)
        {
            return portalRoomSideBitmask.Test(side);
        }

        public void RoomFlagPortalOnSide(MathConstants.eSignedDirection side, bool flag)
        {
            portalRoomSideBitmask.Set(side, flag);
        }

        //TODO: LoadRoom - Convert this over to a request processor
        public static bool LoadRoom(
            AsyncRPGDataContext context,
            World world,
            RoomKey room_key, 
            out Room room, 
            out string result)
        {
            bool success;
            string json_static_data = WorldQueries.GetRoomStaticData(context, room_key);

            room = null;
            success = false;

            // Load static room data for this room
            if (json_static_data.Length > 0)
            {
                StaticRoomData static_room_data = null;

                try
                {
                    if (json_static_data.Length == 0)
                    {
                        throw new ArgumentException();
                    }

                    static_room_data = JsonMapper.ToObject<StaticRoomData>(json_static_data);
                }
                catch (System.Exception)
                {
                    static_room_data = null;
                }

                if (static_room_data != null)
                {
                    room = new Room(room_key);
                    room.static_room_data = static_room_data;

                    success = true;
                    result = SuccessMessages.GENERAL_SUCCESS;
                }
                else
                {
                    result = ErrorMessages.DB_ERROR + "(Failed to parse room static data)";
                }
            }
            else
            {
                result = ErrorMessages.DB_ERROR + "(Failed to get room static data)";
            }

            // If the static room data parsed, load everything else 
            if (success)
            {
                RoomTemplate roomTemplate = world.RoomTemplates.GetTemplateByName(room.static_room_data.room_template_name);

                // Load the random seed for the room
                room.random_seed = WorldQueries.GetRoomRandomSeed(context, room_key);

                // Setup the runtime nav mesh
                room.runtime_nav_mesh = new NavMesh(room.room_key, roomTemplate.NavMeshTemplate);
                
                // Load all of the portals for this room
                room.portals = WorldQueries.GetRoomPortals(context, room_key);

                // Flag all of the room sides that have portals
                foreach (Portal p in room.portals)
                {
                    room.portalRoomSideBitmask.Set(p.room_side, true);
                }

                // Load mob spawners for this room
                room.mobSpawners = MobQueries.GetMobSpawners(context, world.MobSpawnTables, room_key);
            }

            return success;
        }
    }
}