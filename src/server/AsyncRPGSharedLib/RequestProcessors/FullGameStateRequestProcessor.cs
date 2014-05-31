using System;
using System.Linq;
using System.Collections.Generic;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class FullGameStateRequestProcessor : RequestProcessor
    {
        // Request Data
        private int m_character_id;

        // Processor Data
        private World m_world;
        private int m_game_id;
        private RoomKey m_room_key;

        // Result Data
        private bool m_irc_enabled;
        private string m_irc_server;
        private int m_irc_port;
        private string m_irc_encryption_key;
        private bool m_irc_encryption_enabled;
        private CharacterState[] m_characters;
        private CharacterState m_my_character;
        private Point3d m_room_world_position;
        private PortalEntry[] m_portals;
        private MobState[] m_mobs;
        private EnergyTankState[] m_energyTanks;
        private StaticRoomData m_staticRoomData;
        private GameEvent[] m_event_list;

        public FullGameStateRequestProcessor(
            int character_id)
        {
            m_character_id = character_id;

            m_world = null;
            m_game_id = -1;
        }

        public bool IrcEnabled
        {
            get { return m_irc_enabled; }
        }

        public string IrcServer
        {
            get { return m_irc_server; }
        }

        public int IrcPort
        {
            get { return m_irc_port; }
        }

        public string IrcEncryptionKey
        {
            get { return m_irc_encryption_key; }
        }

        public bool IrcEncryptionEnabled
        {
            get { return m_irc_encryption_enabled; }
        }

        public CharacterState[] Characters
        {
            get { return m_characters; }
        }

        public CharacterState MyCharacter
        {
            get { return m_my_character; }
        }

        public RoomKey MyRoomKey
        {
            get { return m_room_key; }
        }

        public Point3d MyRoomWorldPosition
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

        public GameEvent[] EventList
        {
            get { return m_event_list; }
        }

        override protected bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = true;
            result_code = SuccessMessages.GENERAL_SUCCESS;

            success = LookupCharacterGameID(requestCache, out result_code);

            if (success)
            {
                LookupGameIRCDetails(requestCache);
                success = LookupGameCharacterData(requestCache, out result_code);
            }

            if (success)
            {
                success = LookupWorld(requestCache, m_game_id, out m_world, out result_code);
            }

            if (success)
            {
                success = LookupRoom(requestCache, out result_code);
            }

            if (success)
            {
                LookupMobs(requestCache);
                LookupEnergyTanks(requestCache);
                success = PingCharacter(requestCache, m_character_id, out m_event_list, out result_code);
            }

            return success;
        }

        private bool LookupCharacterGameID(
            RequestCache requestCache,
            out string result_code)
        {
            bool success;

            // Get the game that the character currently belongs to
            m_game_id = CharacterQueries.GetCharacterGameId(requestCache.DatabaseContext, m_character_id);

            if (m_game_id >= 0)
            {
                result_code = SuccessMessages.GENERAL_SUCCESS;
                success = true;
            }
            else
            {
                result_code = ErrorMessages.NOT_BOUND;
                success = false;
            }

            return success;
        }

        private void LookupGameIRCDetails(
            RequestCache requestCache)
        {
            // $SECURITY 
            // Sending the IRC key unencrypted like this is not secure.
            // We should be encrypting it with the client's public key RSA-style.
            // Also we should re-generate this key any time some one removes their character from the game.
            // Anytime a character joins the game we should assume that they can snoop on the IRC channel.
            GameQueries.GetGameIRCDetails(
                    requestCache.DatabaseContext,
                    m_game_id,
                    out m_irc_enabled,
                    out m_irc_server,
                    out m_irc_port,
                    out m_irc_encryption_key,
                    out m_irc_encryption_enabled);

        }

        private bool LookupGameCharacterData(
            RequestCache requestCache,
            out string result_code)
        {
            bool success;

            CharacterQueries.GetGameCharacterList(
                requestCache.DatabaseContext,
                m_game_id, 
                out m_characters);

            m_my_character = Array.Find(m_characters, c => c.character_id == m_character_id);

            success= (m_my_character != null);
            result_code = success ? SuccessMessages.GENERAL_SUCCESS : ErrorMessages.DB_ERROR + "(My character not associated with game)";

            return success;
        }

        private bool LookupRoom(
            RequestCache requestCache,
            out string result_code)
        {
            m_room_key= 
                new RoomKey(m_my_character.game_id,
                            m_my_character.room_x,
                            m_my_character.room_y,
                            m_my_character.room_z);

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
