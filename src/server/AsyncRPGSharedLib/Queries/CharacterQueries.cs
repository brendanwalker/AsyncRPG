using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.RequestProcessors;

namespace AsyncRPGSharedLib.Queries
{
    public class CharacterQueries
    {
        public static void CreateCharacter(
            AsyncRPGDataContext context,
            int account_id,
            string name,
            GameConstants.eGender gender,
            GameConstants.eArchetype archetype,
            int picture_id)
        {
            Characters newCharacter = new Characters
            {
                AccountID= account_id,
                GameID= -1,
                RoomX = 0,
                RoomY = 0,
                RoomZ = 0,
                LastPingTime = DateTime.Now,
                LastSentEventID = -1,
                NewEventsPosted = false,
                X= 0.0f,
                Y= 0.0f,
                Z= 0.0f,
                Angle= 0.0f,
                Name= name,
                Gender = (gender == GameConstants.eGender.Male),
                Archetype = (int)archetype,
                PictureID = picture_id,
                PowerLevel= 1,
                Energy= 0                        
            };

            context.Characters.InsertOnSubmit(newCharacter);
            context.SubmitChanges();
        }

        public static void DeleteCharacter(
            AsyncRPGDataContext context,
            int character_id)
        {
            context.Characters.DeleteOnSubmit(context.Characters.Single(c => c.CharacterID == character_id));
            context.SubmitChanges();
        }

        public static void GetGameCharacterList(
            AsyncRPGDataContext context,
            int game_id,
            out CharacterState[] character_list)
        {
            List<CharacterState> temp_character_list = new List<CharacterState>();

            var query =
                from c in context.Characters
                join g in context.Games on c.GameID equals g.GameID
                where c.GameID == game_id
                select new
                {
                    c.CharacterID,
                    c.Name,
                    c.Archetype,
                    c.Gender,
                    c.PictureID,
                    c.PowerLevel,
                    c.RoomX, c.RoomY, c.RoomZ,
                    c.X, c.Y, c.Z, c.Angle,
                    c.GameID,
                    GameName = g.Name
                };

            foreach (var dbCharacter in query)
            {
                CharacterState entry = new CharacterState();

                entry.character_id = dbCharacter.CharacterID;
                entry.character_name = dbCharacter.Name;
                entry.archetype = dbCharacter.Archetype;
                entry.gender = dbCharacter.Gender ? 1 : 0;
                entry.picture_id = dbCharacter.PictureID;
                entry.power_level = dbCharacter.PowerLevel;
                entry.room_x = dbCharacter.RoomX;
                entry.room_y = dbCharacter.RoomY;
                entry.room_z = dbCharacter.RoomZ;
                entry.x = (float)dbCharacter.X;
                entry.y = (float)dbCharacter.Y;
                entry.z = (float)dbCharacter.Z;
                entry.angle = (float)dbCharacter.Angle;
                entry.game_id = dbCharacter.GameID;
                entry.game_name = dbCharacter.GameName;

                temp_character_list.Add(entry);
            }

            character_list = temp_character_list.ToArray();
        }

        // Called directly by GetCharacterFullState() webservice
        public static bool GetFullCharacterState(
            string connection_string,
            int character_id,
            out CharacterState character_state,
            out string result)
        {
            bool success = true;

            result = SuccessMessages.GENERAL_SUCCESS;

            character_state = new CharacterState();

            using (AsyncRPGDataContext context = new AsyncRPGDataContext(connection_string))
            {
                try
                {
                    character_state = CharacterQueries.GetFullCharacterState(context, character_id);
                }
                catch (System.Exception)
                {
                    success = false;
                    result = ErrorMessages.DB_ERROR+"(Failed to get full character state)";
                }
            }

            return success;
        }

        public static CharacterState GetFullCharacterState(
            AsyncRPGDataContext context,
            int character_id)
        {
            CharacterState character_state = new CharacterState();

            var dbCharacter =
                (from c in context.Characters
                    join g in context.Games on c.GameID equals g.GameID into sr
                    from x in sr.DefaultIfEmpty()
                    where c.CharacterID == character_id
                    select new
                    {
                        c.Name,
                        c.Archetype,
                        c.Gender,
                        c.PictureID,
                        c.PowerLevel,
                        c.Energy,
                        c.RoomX,
                        c.RoomY,
                        c.RoomZ,
                        c.X,
                        c.Y,
                        c.Z,
                        c.Angle,
                        c.GameID,
                        GameName = x.Name ?? "" // Can be null if character isn't in a game
                    }).SingleOrDefault();

            character_state.character_id = character_id;
            character_state.character_name = dbCharacter.Name;
            character_state.archetype = dbCharacter.Archetype;
            character_state.gender = dbCharacter.Gender ? 1 : 0;
            character_state.picture_id = dbCharacter.PictureID;
            character_state.power_level = dbCharacter.PowerLevel;
            character_state.energy = dbCharacter.Energy;
            character_state.room_x = dbCharacter.RoomX;
            character_state.room_y = dbCharacter.RoomY;
            character_state.room_z = dbCharacter.RoomZ;
            character_state.x = (float)dbCharacter.X;
            character_state.y = (float)dbCharacter.Y;
            character_state.z = (float)dbCharacter.Z;
            character_state.angle = (float)dbCharacter.Angle;
            character_state.game_id = dbCharacter.GameID;
            character_state.game_name = dbCharacter.GameName;

            return character_state;
        }


        // Called directly by GetCharacterList() webservice
        public static bool GetAccountCharacterList(
            string connection_string,
            int account_id,
            out CharacterState[] character_list,
            out string result)
        {
            bool success = true;

            character_list = null;
            result = SuccessMessages.GENERAL_SUCCESS;

            using (AsyncRPGDataContext context = new AsyncRPGDataContext(connection_string))
            {
                try
                {
                    CharacterQueries.GetAccountCharacterList(context, account_id, out character_list);
                }
                catch (System.Exception)
                {
                    success = false;
                    result = ErrorMessages.DB_ERROR + "(Failed to get account character list state)";
                }
            }

            return success;
        }

        public static void GetAccountCharacterList(
            AsyncRPGDataContext context,
            int account_id,
            out CharacterState[] character_list)
        {
            var query= 
                from c in context.Characters
                join g in context.Games on c.GameID equals g.GameID into sr
                from x in sr.DefaultIfEmpty()
                where c.AccountID == account_id
                select new
                {
                    c.GameID,
                    GameName = x.Name ?? "", // Can be null if character isn't in a game
                    c.CharacterID,
                    c.Name,
                    c.Archetype,
                    c.Gender,
                    c.PictureID,
                    c.PowerLevel,
                    c.RoomX,
                    c.RoomY,
                    c.RoomZ,
                    c.X,
                    c.Y,
                    c.Z,
                    c.Angle
                };

            List<CharacterState> temp_character_list = new List<CharacterState>();

            foreach (var dbCharacter in query)
            {
                CharacterState entry = new CharacterState();

                entry.character_id = dbCharacter.CharacterID;
                entry.character_name = dbCharacter.Name;
                entry.archetype = dbCharacter.Archetype;
                entry.gender = dbCharacter.Gender ? 1 : 0;
                entry.picture_id = dbCharacter.PictureID;
                entry.power_level = dbCharacter.PowerLevel;
                entry.room_x = dbCharacter.RoomX;
                entry.room_y = dbCharacter.RoomY;
                entry.room_z = dbCharacter.RoomZ;
                entry.x = (double)dbCharacter.X;
                entry.y = (double)dbCharacter.Y;
                entry.z = (double)dbCharacter.Z;
                entry.angle = (double)dbCharacter.Angle;
                entry.game_id = dbCharacter.GameID;
                entry.game_name = dbCharacter.GameName;

                temp_character_list.Add(entry);
            }

            character_list = temp_character_list.ToArray();
        }

        public static void GetCharacterIDList(
            AsyncRPGDataContext db_context,
            int account_id,
            out int[] character_id_list)
        {
            character_id_list = 
                (from c in db_context.Characters
                    where c.AccountID == account_id
                    select c.CharacterID).ToArray();
        }

        public static List<Player> GetPlayers(
            AsyncRPGDataContext db_context,
            RoomKey roomKey)
        {
            List<Player> players = new List<Player>();

            var roomPlayerQuery =
                from c in db_context.Characters
                where c.GameID == roomKey.game_id && c.RoomX == roomKey.x && c.RoomY == roomKey.y && c.RoomZ == roomKey.z
                select c;

            foreach (Characters dbCharacter in roomPlayerQuery)
            {
                Player player = Player.CreatePlayer(dbCharacter);

                players.Add(player);
            }

            return players;
        }

        public static Player GetPlayer(
            AsyncRPGDataContext db_context,
            int characterID)
        {
            return
                Player.CreatePlayer(
                    (from c in db_context.Characters
                     where c.CharacterID == characterID
                     select c).Single());
        }

        public static void UpdatePlayer(
            AsyncRPGDataContext db_context,
            Player player)
        {
            var dbCharacter =
                (from p in db_context.Characters
                 where p.CharacterID == player.ID
                 select p).Single();

            dbCharacter.CharacterID = player.ID;
            dbCharacter.Name = player.Name;
            dbCharacter.Archetype = (int)player.Archetype;
            dbCharacter.Gender = (player.Gender == GameConstants.eGender.Male);
            dbCharacter.PictureID = player.PictureID;
            dbCharacter.PowerLevel = player.PowerLevel;
            dbCharacter.Energy = player.Energy;
            //TODO: dbCharacter.Health = this.Health;
            dbCharacter.GameID = player.RoomKey.game_id;
            dbCharacter.RoomX = player.RoomKey.x;
            dbCharacter.RoomY = player.RoomKey.y;
            dbCharacter.RoomZ = player.RoomKey.z;
            dbCharacter.X = player.Position.x;
            dbCharacter.Y = player.Position.y;
            dbCharacter.Z = player.Position.z;
            dbCharacter.Angle = player.Angle;

            db_context.SubmitChanges();
        }

        public static string GetCharacterName(
            AsyncRPGDataContext context,
            int character_id)
        {
            return
                (from c in context.Characters
                    where c.CharacterID == character_id
                    select c.Name).SingleOrDefault();
        }

        public static bool GetCharacterNewEventFlag(
            AsyncRPGDataContext context,
            int character_id)
        {
            return (from c in context.Characters where c.CharacterID == character_id select c.NewEventsPosted).Single();
        }

        public static int GetCharacterGameId(
            AsyncRPGDataContext context,
            int character_id)
        {
            return (from c in context.Characters where c.CharacterID == character_id select c.GameID).Single();
        }

        public static DateTime GetCharacterLastPingTime(
            AsyncRPGDataContext context,
            int character_id)
        {
            return
                (from c in context.Characters
                 where c.CharacterID == character_id
                 select c.LastPingTime).SingleOrDefault();
        }

        public static string GetCharacterEmail(
            AsyncRPGDataContext context,
            int character_id)
        {
            return
                (from c in context.Characters
                    join a in context.Accounts on c.AccountID equals a.AccountID
                    where c.CharacterID == character_id
                    select a.EmailAddress).SingleOrDefault();
        }

        public static void GetCharacterPosition(
            AsyncRPGDataContext context,
            int character_id,
            out RoomKey roomKey,
            out Point3d position,
            out float angle)
        {
            var dbCharacter =
                (from c in context.Characters
                 where c.CharacterID == character_id
                 select new
                 {
                     roomKey = new RoomKey(c.GameID, c.RoomX, c.RoomY, c.RoomZ),
                     position = new Point3d((float)c.X, (float)c.Y, (float)c.Z),
                     angle = (float)c.Angle
                 }).SingleOrDefault();

            roomKey= dbCharacter.roomKey;
            position = dbCharacter.position;
            angle = dbCharacter.angle;
        }

        // Called directly by GetCharacterPosition WebService
        public static bool GetCharacterPosition(
            string connection_string,
            int character_id,
            out RoomKey roomKey,
            out float x,
            out float y,
            out float z,
            out float angle)
        {
            bool success = false;

            roomKey = new RoomKey();
            x = y = z = angle = 0;

            using (AsyncRPGDataContext context = new AsyncRPGDataContext(connection_string))
            {
                try
                {
                    var dbCharacter =
                        (from c in context.Characters
                         where c.CharacterID == character_id
                         select c).SingleOrDefault();

                    roomKey.game_id = dbCharacter.GameID;
                    roomKey.x = dbCharacter.RoomX;
                    roomKey.y = dbCharacter.RoomY;
                    roomKey.z = dbCharacter.RoomZ;
                    x = dbCharacter.X;
                    y = dbCharacter.Y;
                    z = dbCharacter.Z;

                    success = true;
                }
                catch (System.Exception)
                {
                    success = false;
                }
            }

            return success;
        }

        public static int GetCharacterEnergy(
            AsyncRPGDataContext context,
            int character_id)
        {
            return
                (from c in context.Characters
                    where c.CharacterID == character_id
                    select c.Energy).SingleOrDefault();
        }

        public static void UpdateCharacterPosition(
            AsyncRPGDataContext context,
            int character_id,
            RoomKey roomKey,
            Point3d position,
            float angle)
        {
            var dbCharacter =
                (from c in context.Characters
                 where c.CharacterID == character_id
                 select c).SingleOrDefault();

            dbCharacter.RoomX = roomKey.x;
            dbCharacter.RoomY = roomKey.y;
            dbCharacter.RoomZ = roomKey.z;
            dbCharacter.X = position.x;
            dbCharacter.Y = position.y;
            dbCharacter.Z = position.z;
            dbCharacter.Angle = angle;

            context.SubmitChanges();
        }

        public static void UpdateCharacterEnergy(
            AsyncRPGDataContext context,
            int character_id,
            int energy)
        {
            var dbCharacter =
                (from c in context.Characters
                 where c.CharacterID == character_id
                 select c).SingleOrDefault();

            dbCharacter.Energy = energy;

            context.SubmitChanges();
        }

        public static void UpdateCharacterLastPingTime(
            AsyncRPGDataContext context,
            int character_id)
        {
            var query = (from c in context.Characters
                         where c.CharacterID == character_id
                         select c).Single();

            query.LastPingTime = DateTime.Now;

            context.SubmitChanges();
        }

        public static void ClearCharacterNewEventFlag(
            AsyncRPGDataContext context,
            int character_id)
        {
            var query = (from c in context.Characters
                         where c.CharacterID == character_id
                         select c).Single();

            query.NewEventsPosted = false;

            context.SubmitChanges();
        }

        public static int GetCharacterLastEventId(
            AsyncRPGDataContext context,
            int character_id)
        {
            return (from c in context.Characters where c.CharacterID == character_id select c.LastSentEventID).Single();
        }


        public static void UpdateCharacterLastEventId(
            AsyncRPGDataContext context,
            int character_id,
            int last_event_id)
        {
            var query =
                (from c in context.Characters
                 where c.CharacterID == character_id
                 select c).Single();

            query.LastSentEventID = last_event_id;

            context.SubmitChanges();
        }
    }
}