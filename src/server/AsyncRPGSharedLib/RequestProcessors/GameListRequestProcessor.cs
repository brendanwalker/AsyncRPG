using System;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class GameListRequestProcessor : RequestProcessor
    {
        // Result
        GameResponseEntry[] gameList;

        public GameListRequestProcessor()
        {
            gameList= null;
        }

        public GameResponseEntry[] GameList
        {
            get { return gameList; }
        }

        protected override bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            result_code = SuccessMessages.GENERAL_SUCCESS;
            
            // Get the list of games without character names
            gameList = GameQueries.GetGameList(requestCache.DatabaseContext);

            // For each game, get the character list
            foreach (GameResponseEntry entry in gameList)
            {
                entry.character_names= 
                    GameQueries.GetCharacterNamesInGame(
                        requestCache.DatabaseContext, 
                        entry.game_id);
            }

            return true;
        }
    }
}
