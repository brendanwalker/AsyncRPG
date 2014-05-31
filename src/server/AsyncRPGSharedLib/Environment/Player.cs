using System;
using AsyncRPGSharedLib.RequestProcessors;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.Environment
{
    public class Player : CacheableObject, IEnvironmentEntity
    {
        private int m_character_id;
        private string m_character_name;
        private GameConstants.eArchetype m_archetype;
        private GameConstants.eGender m_gender;
        private int m_picture_id;
        private int m_power_level;
        private int m_energy;
        private int m_health;
        private RoomKey m_room_key;
        private Point3d m_position;
        public float m_angle;

        public Player()
        {
            m_character_id = -1;
            m_character_name = "";
            m_archetype = GameConstants.eArchetype.warrior;
            m_gender = GameConstants.eGender.Female;
            m_picture_id = -1;
            m_power_level = 0;
            m_energy = 0;
            m_health = 0;
            m_room_key = new RoomKey();
            m_position = new Point3d();
            m_energy = 0;
        }

        public static Player CreatePlayer(CharacterState characterState)
        {
            Player player = new Player();

            player.m_character_id = characterState.character_id;
            player.m_character_name = characterState.character_name;
            player.m_archetype = (GameConstants.eArchetype)characterState.archetype;
            player.m_gender = (GameConstants.eGender)characterState.gender;
            player.m_picture_id = characterState.picture_id;
            player.m_power_level = characterState.power_level;
            player.m_energy = characterState.energy;
            player.m_health = 0; //TODO: characterState.health;
            player.m_room_key = 
                new RoomKey(
                    characterState.game_id, 
                    characterState.room_x, 
                    characterState.room_y, 
                    characterState.room_z);
            player.m_position = new Point3d((float)characterState.x, (float)characterState.y, (float)characterState.z);
            player.m_angle = (float)characterState.angle;

            return player;
        }

        public static Player CreatePlayer(Characters dbCharacter)
        {
            Player player = new Player();

            player.m_character_id = dbCharacter.CharacterID;
            player.m_character_name = dbCharacter.Name;
            player.m_archetype = (GameConstants.eArchetype)dbCharacter.Archetype;
            player.m_gender = dbCharacter.Gender ? GameConstants.eGender.Male : GameConstants.eGender.Female;
            player.m_picture_id = dbCharacter.PictureID;
            player.m_power_level = dbCharacter.PowerLevel;
            player.m_energy = dbCharacter.Energy;
            player.m_health = 0; //TODO: characterState.health;
            player.m_room_key =
                new RoomKey(
                    dbCharacter.GameID, 
                    dbCharacter.RoomX, 
                    dbCharacter.RoomY, 
                    dbCharacter.RoomZ);
            player.m_position = new Point3d(dbCharacter.X, dbCharacter.Y, dbCharacter.Z);
            player.m_angle = (float)dbCharacter.Angle;

            return player;
        }

        public override void WriteDirtyObjectToDatabase(AsyncRPGDataContext db_context)
        {
            if (IsDirty)
            {
                CharacterQueries.UpdatePlayer(db_context, this);
                IsDirty = false;
            }
        }

        public int ID
        {
            get { return m_character_id; }
            set { m_character_id = value; }
        }


        public string Name
        {
            get { return m_character_name; }
            set { m_character_name = value; IsDirty = true; }
        }

        public GameConstants.eArchetype Archetype
        {
            get { return m_archetype; }
            set { m_archetype = value; IsDirty = true; } 
        }

        public GameConstants.eGender Gender
        {
            get { return m_gender; }
            set { m_gender = value; IsDirty = true; }
        }

        public GameConstants.eFaction Faction 
        {
            get { return GameConstants.eFaction.player; }
        }

        public int PictureID
        {
            get { return m_picture_id; }
            set { m_picture_id = value; IsDirty = true; }
        }

        public int PowerLevel
        {
            get { return m_power_level; }
            set { m_power_level = value; IsDirty = true; }
        }

        public int Health
        {
            get { return m_health; }
            set { m_health = value; IsDirty = true; }
        }

        public int Energy
        {
            get { return m_energy; }
            set { m_energy = value; IsDirty = true; }
        }

        public RoomKey RoomKey
        {
            get { return m_room_key; }
            set { m_room_key = new RoomKey(value); IsDirty = true; }
        }

        public Point3d Position
        {
            get { return m_position; }
            set { m_position = new Point3d(value); IsDirty = true; }
        }

        public float Angle
        {
            get { return m_angle; }
            set { m_angle = value; IsDirty = true; }
        }
    }
}
