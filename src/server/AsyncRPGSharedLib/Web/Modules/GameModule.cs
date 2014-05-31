using System;
using AsyncRPGSharedLib.RequestProcessors;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Web.Modules;
using AsyncRPGSharedLib.Web.Cache;
using AsyncRPGSharedLib.Web.Interfaces;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Protocol;

namespace AsyncRPGSharedLib.Web.Modules
{
    [RestModuleName("Game")]
    public class GameModule : RestModule
    {
        public GameModule()
            : base()
        {
        }

        public GameModule(
            ICacheAdapter appCache,
            ISessionAdapter session,
            IResponseAdapter response) :
            base(appCache, session, response)
        {
        }

        public string CreateGame(
            string game_name,
            int dungeon_size,
            int dungeon_difficulty,
            bool irc_enabled,
            string irc_server,
            int irc_port,
            bool irc_encryption_enabled)
        {
            BasicResponse response = new BasicResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result))
            {
                int account_id = RestUtilities.GetSessionAccountID(Session);

                GameCreateRequestProcessor requestProcessor =
                    new GameCreateRequestProcessor(
                        RestUtilities.GetSessionAccountID(Session),
                        game_name,
                        (GameConstants.eDungeonSize)dungeon_size,
                        (GameConstants.eDungeonDifficulty)dungeon_difficulty,
                        irc_enabled,
                        irc_server,
                        irc_port,
                        irc_encryption_enabled);

                if (requestProcessor.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out response.result))
                {
                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string DeleteGame(
            int game_id)
        {
            BasicResponse response = new BasicResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result))
            {
                GameDeleteRequestProcessor requestProcessor =
                    new GameDeleteRequestProcessor(
                        RestUtilities.GetSessionAccountID(Session),
                        game_id);

                if (requestProcessor.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out response.result))
                {
                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string GetGameList()
        {
            GameListResponse response = new GameListResponse();
            GameListRequestProcessor requestProcessor = new GameListRequestProcessor();

            if (requestProcessor.ProcessRequest(
                    ApplicationConstants.CONNECTION_STRING,
                    Application,
                    out response.result))
            {
                response.game_list = requestProcessor.GameList;
                response.result = SuccessMessages.GENERAL_SUCCESS;
            }

            return JSONUtilities.SerializeJSONResponse<GameListResponse>(response);
        }

        public string BindCharacterToGame(
            int character_id,
            int game_id)
        {
            BasicResponse response = new BasicResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionOwnsCharacter(Session, character_id, out response.result))
            {
                BindCharacterToGameRequestProcessor requestProcessor =
                    new BindCharacterToGameRequestProcessor(character_id, game_id);

                if (requestProcessor.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out response.result))
                {
                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }
    }
}
