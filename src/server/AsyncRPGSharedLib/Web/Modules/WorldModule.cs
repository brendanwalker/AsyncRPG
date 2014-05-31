using System;
using System.Collections.Generic;
using AsyncRPGSharedLib.RequestProcessors;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Web.Modules;
using AsyncRPGSharedLib.Web.Cache;
using AsyncRPGSharedLib.Web.Interfaces;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Protocol;

namespace AsyncRPGSharedLib.Web.Modules
{
    [RestModuleName("World")]
    public class WorldModule : RestModule
    {
        public WorldModule()
            : base()
        {
        }

        public WorldModule(
            ICacheAdapter appCache,
            ISessionAdapter session,
            IResponseAdapter response) :
            base(appCache, session, response)
        {
        }

        public string WorldGetFullGameStateRequest(
            int character_id)
        {
            WorldGetFullGameStateResponse response = new WorldGetFullGameStateResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionOwnsCharacter(Session, character_id, out response.result))
            {
                FullGameStateRequestProcessor fullGameRequest = new FullGameStateRequestProcessor(character_id);

                if (fullGameRequest.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out response.result))
                {
                    // IRC Details
                    response.irc_enabled = fullGameRequest.IrcEnabled;
                    response.irc_server = fullGameRequest.IrcServer;
                    response.irc_port = fullGameRequest.IrcPort;
                    response.irc_encryption_key = fullGameRequest.IrcEncryptionKey;
                    response.irc_encryption_enabled = fullGameRequest.IrcEncryptionEnabled;

                    // Character data for all Characters in the game
                    response.characters = fullGameRequest.Characters;

                    // List of events relevant to the requesting character since they last logged in
                    response.event_list = fullGameRequest.EventList;

                    // Room Data for the room that requesting player is in
                    response.room_x = fullGameRequest.MyRoomKey.x;
                    response.room_y = fullGameRequest.MyRoomKey.y;
                    response.room_z = fullGameRequest.MyRoomKey.z;
                    response.world_x = fullGameRequest.MyRoomWorldPosition.x;
                    response.world_y = fullGameRequest.MyRoomWorldPosition.y;
                    response.world_z = fullGameRequest.MyRoomWorldPosition.z;
                    response.portals = fullGameRequest.Portals;
                    response.mobs = fullGameRequest.Mobs;
                    response.energyTanks = fullGameRequest.EnergyTanks;
                    response.data = fullGameRequest.StaticRoomData;

                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }

            return JSONUtilities.SerializeJSONResponse<WorldGetFullGameStateResponse>(response);
        }

        public string GetRoomData(
            int game_id,
            int room_x,
            int room_y,
            int room_z)
        {
            WorldGetRoomDataResponse response = new WorldGetRoomDataResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result))
            {
                RoomRequestProcessor roomRequest =
                    new RoomRequestProcessor(new RoomKey(game_id, room_x, room_y, room_z));

                if (roomRequest.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out response.result))
                {
                    // Room Data for the room that requesting player is in
                    response.room_x = roomRequest.RoomKey.x;
                    response.room_y = roomRequest.RoomKey.y;
                    response.room_z = roomRequest.RoomKey.z;
                    response.world_x = roomRequest.RoomWorldPosition.x;
                    response.world_y = roomRequest.RoomWorldPosition.y;
                    response.world_z = roomRequest.RoomWorldPosition.z;
                    response.portals = roomRequest.Portals;
                    response.mobs = roomRequest.Mobs;
                    response.energyTanks = roomRequest.EnergyTanks;
                    response.data = roomRequest.StaticRoomData;

                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }

            return JSONUtilities.SerializeJSONResponse<WorldGetRoomDataResponse>(response);
        }
    }
}
