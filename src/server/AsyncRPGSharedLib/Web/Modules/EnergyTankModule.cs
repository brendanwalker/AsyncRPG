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
    [RestModuleName("EnergyTank")]
    public class EnergyTankModule : RestModule
    {
        public EnergyTankModule()
            : base()
        {
        }

        public EnergyTankModule(
            ICacheAdapter appCache,
            ISessionAdapter session,
            IResponseAdapter response) :
            base(appCache, session, response)
        {
        }

        public string HackEnergyTank(
            int character_id, 
            int energy_tank_id)
        {
            CharacterHackEnergyTankResponse response = new CharacterHackEnergyTankResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionOwnsCharacter(Session, character_id, out response.result))
            {
                HackEnergyTankRequestProcessor requestProcessor =
                    new HackEnergyTankRequestProcessor(
                        character_id,
                        energy_tank_id);

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

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string DrainEnergyTank(
            int character_id,
            int energy_tank_id)
        {
            CharacterDrainEnergyTankResponse response = new CharacterDrainEnergyTankResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionOwnsCharacter(Session, character_id, out response.result))
            {
                DrainEnergyTankRequestProcessor requestProcessor =
                    new DrainEnergyTankRequestProcessor(
                        character_id,
                        energy_tank_id);

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

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

    }
}