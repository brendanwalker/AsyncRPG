using System;
using System.Linq;
using AsyncRPGSharedLib.RequestProcessors;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Web.Modules;
using AsyncRPGSharedLib.Web.Cache;
using AsyncRPGSharedLib.Web.Interfaces;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.Web.Modules
{
    [RestModuleName("Character")]
    public class CharacterModule : RestModule
    {
        public CharacterModule()
            : base()
        {
        }

        public CharacterModule(
            ICacheAdapter appCache,
            ISessionAdapter session,
            IResponseAdapter response) :
            base(appCache, session, response)
        {
        }

        public string CreateCharacter(
            string name,
            int archetype,
            int gender,
            int picture_id)
        {
            BasicResponse response = new BasicResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result))
            {
                int archetypeCount = EnumUtilities.GetEnumValues<GameConstants.eArchetype>().Count();
                CharacterCreateRequestProcessor requestProcessor =
                    new CharacterCreateRequestProcessor(
                        RestUtilities.GetSessionAccountID(Session),
                        name,
                        (gender > 0) ? GameConstants.eGender.Male : GameConstants.eGender.Female,
                        (GameConstants.eArchetype)Math.Max(Math.Min(archetype, archetypeCount - 1), 0),
                        picture_id);

                if (requestProcessor.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out response.result))
                {
                    Session["CharacterIDs"] = requestProcessor.AccountCharacterIDs;

                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string GetCharacterFullState(int character_id)
        {
            CharacterStateResponse response = new CharacterStateResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result))
            {
                if (CharacterQueries.GetFullCharacterState(
                        ApplicationConstants.CONNECTION_STRING,
                        character_id,
                        out response.character_state,
                        out response.result))
                {
                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }

            return JSONUtilities.SerializeJSONResponse<CharacterStateResponse>(response);
        }

        public string GetCharacterList(string username)
        {
            CharacterListResponse response = new CharacterListResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionLoggedInAsUser(Session, username, out response.result))
            {
                if (CharacterQueries.GetAccountCharacterList(
                        ApplicationConstants.CONNECTION_STRING,
                        RestUtilities.GetSessionAccountID(Session),
                        out response.character_list,
                        out response.result))
                {
                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }

            return JSONUtilities.SerializeJSONResponse<CharacterListResponse>(response);
        }

        public string DeleteCharacter(int character_id)
        {
            BasicResponse response = new BasicResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionOwnsCharacter(Session, character_id, out response.result))
            {
                CharacterDeleteRequestProcessor requestProcessor =
                    new CharacterDeleteRequestProcessor(
                        RestUtilities.GetSessionAccountID(Session),
                        character_id);

                if (requestProcessor.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out response.result))
                {
                    Session["CharacterIDs"] = requestProcessor.RemainingCharacterIDs;

                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string PingCharacter(int character_id)
        {
            GamePongResponse response = new GamePongResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionOwnsCharacter(Session, character_id, out response.result))
            {
                PingCharacterRequestProcessor pingProcessor = new PingCharacterRequestProcessor(character_id);

                if (pingProcessor.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out response.result))
                {
                    response.event_list = pingProcessor.ResultGameEvents;
                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
                else
                {
                    response.event_list = new GameEvent[] { };
                }
            }


            return JSONUtilities.SerializeJSONResponse<GamePongResponse>(response);
        }

        public string MoveCharacter(
            int character_id,
            float x,
            float y,
            float z,
            float angle)
        {
            CharacterMoveResponse response = new CharacterMoveResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionOwnsCharacter(Session, character_id, out response.result))
            {
                MoveRequestProcessor requestProcessor =
                    new MoveRequestProcessor(
                        character_id,
                        new Point3d(x, y, z),
                        angle);

                if (requestProcessor.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out response.result))
                {
                    response.event_list = requestProcessor.ResultEventList;
                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
                else
                {
                    response.event_list = new GameEvent[] { };
                }
            }

            return JSONUtilities.SerializeJSONResponse<CharacterMoveResponse>(response);
        }

        public string PortalCharacter(
            int character_id,
            float x,
            float y,
            float z,
            float angle,
            int portal_id)
        {
            CharacterPortalResponse response = new CharacterPortalResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionOwnsCharacter(Session, character_id, out response.result))
            {
                PortalRequestProcessor requestProcessor =
                    new PortalRequestProcessor(
                        character_id,
                        new Point3d(x, y, z),
                        angle,
                        portal_id);

                if (requestProcessor.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out response.result))
                {
                    response.event_list = requestProcessor.ResultEventList;
                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
                else
                {
                    response.event_list = new GameEvent[] { };
                }
            }

            return JSONUtilities.SerializeJSONResponse<CharacterPortalResponse>(response);
        }

        public string GetCharacterPosition(
            int character_id)
        {
            CharacterGetPositionResponse response = new CharacterGetPositionResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionOwnsCharacter(Session, character_id, out response.result))
            {
                RoomKey roomKey = null;

                float response_x= 0.0f;
                float response_y = 0.0f;
                float response_z = 0.0f;
                float response_angle = 0.0f;

                if (!CharacterQueries.GetCharacterPosition(
                        ApplicationConstants.CONNECTION_STRING,
                        character_id,
                        out roomKey,
                        out response_x,
                        out response_y,
                        out response_z,
                        out response_angle))
                {
                    response.x= (double)response_x;
                    response.y = (double)response_y;
                    response.z = (double)response_z;
                    response.angle = (double)response_angle;
                    response.game_id = -1;
                    response.room_x = 0;
                    response.room_y = 0;
                    response.room_z = 0;
                    response.angle = 0;
                    response.result = ErrorMessages.DB_ERROR;
                }
            }

            return JSONUtilities.SerializeJSONResponse<CharacterGetPositionResponse>(response);
        }
    }
}