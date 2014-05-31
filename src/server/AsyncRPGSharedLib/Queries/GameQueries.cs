using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Protocol;

namespace AsyncRPGSharedLib
{
    public class GameQueries
    {
        public static int CreateGame(
            AsyncRPGDataContext context,
            int account_id,
            string game_name,
            GameConstants.eDungeonSize dungeonSize,
            GameConstants.eDungeonDifficulty dungeonDifficulty,
            bool irc_enabled,
            string irc_server,
            int irc_port,
            bool irc_encryption_enabled)
        {
            int new_game_id = -1;

            // Create a random 256-bit (32 byte) encryption key for encrypting chat
            string irc_encryption_key = RNGUtilities.CreateNonDeterministicRandomBase64String(256);
            Debug.Assert(irc_encryption_key.Length == 44);

            if (irc_server.Length == 0)
            {
                irc_server = IrcConstants.DEFAULT_IRC_SERVER;
            }

            if (irc_port < IrcConstants.LOWEST_VALID_IRC_PORT || irc_port > IrcConstants.HIGHEST_VALID_IRC_PORT)
            {
                irc_port = IrcConstants.DEFAULT_IRC_PORT;
            }

            {
                Games newGame = new Games
                {
                    OwnerAccountID = account_id,
                    Name = game_name,
                    DungeonSize = (int)dungeonSize,
                    DungeonDifficulty = (int)dungeonDifficulty,
                    IrcEnabed = irc_enabled,
                    IrcServer = irc_server,
                    IrcPort = irc_port,
                    IrcEncryptionKey = irc_encryption_key,
                    IrcEncryptionEnabed = irc_encryption_enabled
                };

                context.Games.InsertOnSubmit(newGame);
                context.SubmitChanges();

                new_game_id = newGame.GameID;
            }

            return new_game_id;
        }

        public static void DeleteGame(
            AsyncRPGDataContext context,
            int gameid)
        {
            // Detach all of the Characters from the game
            {
                var query =
                    from character in context.Characters
                    where character.GameID == gameid
                    select character;

                foreach (Characters character in query)
                {
                    character.GameID = -1;
                    character.RoomX = 0;
                    character.RoomY = 0;
                    character.RoomZ = 0;
                    character.X = 0;
                    character.Y = 0;
                    character.Z = 0;
                    character.Angle = 0;
                }

                context.SubmitChanges();
            }

            // Free all of the portals
            {
                var query =
                    from portal in context.Portals
                    where portal.GameID == gameid
                    select portal;

                foreach (Portals portal in query)
                {
                    context.Portals.DeleteOnSubmit(portal);
                }

                context.SubmitChanges();
            }

            // Free all of the mobs
            {
                var query =
                    from mob in context.Mobs
                    where mob.GameID == gameid
                    select mob;

                foreach (Mobs mob in query)
                {
                    context.Mobs.DeleteOnSubmit(mob);
                }

                context.SubmitChanges();
            }

            // Free all of the mob spawners
            {
                var query =
                    from mobSpawner in context.MobSpawners
                    where mobSpawner.GameID == gameid
                    select mobSpawner;

                foreach (MobSpawners mobSpawner in query)
                {
                    context.MobSpawners.DeleteOnSubmit(mobSpawner);
                }

                context.SubmitChanges();
            }

            // TODO DeleteGame: Delete loot

            // Free all of the rooms
            {
                var query =
                    from room in context.Rooms
                    where room.GameID == gameid
                    select room;

                foreach (Rooms room in query)
                {
                    context.Rooms.DeleteOnSubmit(room);
                }

                context.SubmitChanges();
            }

            // Free all of the game_events associated from the game
            GameEventQueries.DeleteAllEventsForGame(context, gameid);

            // Finally, Free the game itself
            {
                var query =
                    from game in context.Games
                    where game.GameID == gameid
                    select game;

                foreach (Games game in query)
                {
                    context.Games.DeleteOnSubmit(game);
                }

                context.SubmitChanges();
            }
        }

        public static GameResponseEntry[] GetGameList(
            AsyncRPGDataContext context)
        {
            List<GameResponseEntry> gameList = new List<GameResponseEntry>();
            var query = from g in context.Games
                        join a in context.Accounts on g.OwnerAccountID equals a.AccountID into sr
                        from x in sr.DefaultIfEmpty()
                         select new
                         {
                             g.GameID,
                             g.Name,
                             g.OwnerAccountID,
                             OwnerAccountName= x.UserName ?? "" // Can be null if the game isn't owned by anyone
                         };

            foreach (var dbEntry in query)
            {
                GameResponseEntry entry = new GameResponseEntry();

                entry.game_id = dbEntry.GameID;
                entry.game_name = dbEntry.Name;
                entry.owner_account_id = dbEntry.OwnerAccountID;
                entry.owner_account_name = dbEntry.OwnerAccountName;
                entry.character_names = null;

                gameList.Add(entry);
            }

            //join g in context.games on c.gameid equals g.gameid
            return gameList.ToArray();
        }

        public static string[] GetCharacterNamesInGame(
            AsyncRPGDataContext context,
            int gameid)
        {
            return (from c in context.Characters
                         where c.GameID == gameid
                         select c.Name).ToArray();
        }

        public static string GetGameName(
            AsyncRPGDataContext context,
            int gameid)
        {
            return (from g in context.Games where g.GameID == gameid select g.Name).Single();
        }

        public static void GetGameIRCDetails(
            AsyncRPGDataContext context,
            int gameid,
            out bool irc_enabled,
            out string irc_server,
            out int irc_port,
            out string irc_encryption_key,
            out bool irc_encryption_enabled)
        {
            var query =
                (from g in context.Games
                 where g.GameID == gameid
                 select new { g.IrcEnabed, g.IrcServer, g.IrcPort, g.IrcEncryptionEnabed, g.IrcEncryptionKey }).Single();

            irc_enabled = query.IrcEnabed;
            irc_server = query.IrcServer;
            irc_port = query.IrcPort;
            irc_encryption_key = query.IrcEncryptionKey;
            irc_encryption_enabled = query.IrcEncryptionEnabed;
        }

        public static void BindCharacterToGame(
            AsyncRPGDataContext context,
            int character_id,
            int new_game_id)
        {
            // Get the most recent event that occurred in the new game
            int last_game_event_id = GameEventQueries.GetLastGameEvent(context, new_game_id);

            // Tell the character which game they are bound to
            // and set an initial location
            var dbCharacter =
                (from c in context.Characters
                 where c.CharacterID == character_id
                 select c).Single();

            dbCharacter.GameID = new_game_id;
            dbCharacter.RoomX = 0;
            dbCharacter.RoomY = 0;
            dbCharacter.RoomZ = 0;
            dbCharacter.X = 0;
            dbCharacter.Y = 0;
            dbCharacter.Z = 0;
            dbCharacter.Angle = 0;
            dbCharacter.LastSentEventID = last_game_event_id;
            dbCharacter.NewEventsPosted = false;

            context.SubmitChanges();
        }

        public static void UnBindCharacterFromGame(
            AsyncRPGDataContext context,
            int character_id)
        {
            var dbCharacter =
                (from c in context.Characters
                 where c.CharacterID == character_id
                 select c).Single();

            dbCharacter.GameID = -1;
            dbCharacter.RoomX = 0;
            dbCharacter.RoomY = 0;
            dbCharacter.RoomZ = 0;
            dbCharacter.X = 0;
            dbCharacter.Y = 0;
            dbCharacter.Z = 0;
            dbCharacter.Angle = 0;

            context.SubmitChanges();
        }

        public static bool VerifyGameExists(
            AsyncRPGDataContext context,
            int gameid)
        {
            return (from g in context.Games where g.GameID == gameid select g).Count() > 0;
        }

        public static bool VerifyAccountOwnsGame(
            AsyncRPGDataContext context,
            int account_id,
            int gameid)
        {
            return
                (from game in context.Games
                 where game.GameID == gameid && game.OwnerAccountID == account_id
                 select game).Count() > 0;
        }
    }
}