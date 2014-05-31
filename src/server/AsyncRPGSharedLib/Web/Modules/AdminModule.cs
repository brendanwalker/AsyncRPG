using System;
using System.Collections.Generic;
using System.Text;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.RequestProcessors;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Web.Modules;
using AsyncRPGSharedLib.Web.Cache;
using AsyncRPGSharedLib.Web.Interfaces;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.Web.Modules
{
    [RestModuleName("Admin")]
    public class AdminModule : RestModule
    {
        public AdminModule()
            : base()
        {
        }

        public AdminModule(
            ICacheAdapter appCache,
            ISessionAdapter session,
            IResponseAdapter response) :
            base(appCache, session, response)
        {
        }

        public string RecreateDatabase()
        {
            BasicResponse response = new BasicResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionHasAdminOpsLevel(Session, out response.result))
            {
                StringBuilder result = new StringBuilder();
                Logger logger = new Logger((string message) => { result.AppendLine(message); });

                try
                {
                    string constructionResult = "";
                    DatabaseManagerConfig dbConfig =
                        new DatabaseManagerConfig(
                            ApplicationConstants.CONNECTION_STRING,
                            ApplicationConstants.MOBS_DIRECTORY,
                            ApplicationConstants.MAPS_DIRECTORY);
                    DatabaseManager dbManager = new DatabaseManager(dbConfig);

                    if (!dbManager.ReCreateDatabase(logger, out constructionResult))
                    {
                        logger.LogError(constructionResult);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.LogError(string.Format("Failed to recreate database: {0}", ex.Message));                        
                }

                response.result = result.ToString();
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string SetAccountOpsLevel(int account_id, int ops_level)
        {
            BasicResponse response = new BasicResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionHasAdminOpsLevel(Session, out response.result))
            {
                if (AccountQueries.SetAccountOpsLevel(
                        ApplicationConstants.CONNECTION_STRING,
                        account_id,
                        (DatabaseConstants.OpsLevel)ops_level,
                        out response.result))
                {
                    response.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string DebugClearCachedWorld(int game_id)
        {
            BasicResponse response = new BasicResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out response.result) &&
                RestUtilities.ValidateJSONRequestSessionHasAdminOpsLevel(Session, out response.result))
            {
                WorldCache.ClearWorld(Application, game_id);
                WorldBuilderCache.ClearWorldBuilder(Application);

                response.result = SuccessMessages.GENERAL_SUCCESS;
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        //public string UpgradeDatabase()
        //{
        //    BasicResponse response = new BasicResponse();

        //    if (RestUtilities.ValidateJSONRequest<DebugWorldClearRequest>(
        //            Session,
        //            null,
        //            new RestUtilities.JSONRequestValidator<DebugWorldClearRequest>[] {
        //                new RestUtilities.JSONRequestValidator<DebugWorldClearRequest>( RestUtilities.ValidateJSONRequestHasAuthenticatedSession ),
        //                new RestUtilities.JSONRequestValidator<DebugWorldClearRequest>( RestUtilities.ValidateJSONRequestSessionHasAdminOpsLevel )
        //            },
        //            out response.result))
        //    {
        //        response.result = ConfigQueries.UpgradeDatabase(ApplicationConstants.CONNECTION_STRING);
        //    }

        //    return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        //}

        public string DumpRoomTemplateReport()
        {
            string result = "";

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out result) &&
                RestUtilities.ValidateJSONRequestSessionHasAdminOpsLevel(Session, out result))
            {
                RoomTemplateRequestProcessor requestProcessor = new RoomTemplateRequestProcessor();

                if (requestProcessor.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        Application,
                        out result))
                {
                    result = requestProcessor.RoomTemplateReport.ToString();
                }
            }

            return result;
        }
    }
}
