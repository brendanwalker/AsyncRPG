using System;
using System.Linq;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Web.Cache;
using AsyncRPGSharedLib.Web.Interfaces;

namespace AsyncRPGSharedLib.Web
{
    public class RestUtilities
    {
        public static bool ValidateJSONRequestHasAuthenticatedSession(
            ISessionAdapter session,
            out string error_code)
        {
            bool success = true;

            error_code = SuccessMessages.GENERAL_SUCCESS;

            if (!IsSessionAuthenticated(session))
            {
                error_code = ErrorMessages.NOT_AUTHENTICATED;
                success = false;
            }

            return success;
        }

        public static bool ValidateJSONRequestSessionLoggedInAsUser(
            ISessionAdapter session,
            string username,
            out string error_code)
        {
            bool success = true;

            error_code = SuccessMessages.GENERAL_SUCCESS;

            if (GetSessionUsername(session) != username)
            {
                error_code = ErrorMessages.NOT_LOGGED_IN;
                success = false;
            }

            return success;
        }

        public static bool ValidateJSONRequestSessionOwnsCharacter(
            ISessionAdapter session,
            int characterId,
            out string error_code)
        {
            bool success = true;

            error_code = SuccessMessages.GENERAL_SUCCESS;

            if (!VerifySessionOwnsCharacter(session, characterId))
            {
                error_code = ErrorMessages.INVALID_REQUEST;
                success = false;
            }

            return success;
        }

        public static bool ValidateJSONRequestSessionHasGMOpsLevel(
            ISessionAdapter session,
            out string error_code)
        {
            bool success = false;
            int session_account_id = GetSessionAccountID(session);
            DatabaseConstants.OpsLevel opsLevel =
                AccountQueries.GetAccountOpsLevel(
                    ApplicationConstants.CONNECTION_STRING,
                    session_account_id,
                    out error_code);

            if (opsLevel != DatabaseConstants.OpsLevel.invalid)
            {
                if (opsLevel >= DatabaseConstants.OpsLevel.game_master)
                {
                    error_code = SuccessMessages.GENERAL_SUCCESS;
                    success = true;
                }
                else
                {
                    error_code = ErrorMessages.INSUFFICIENT_OPS_LEVEL;
                }
            }

            return success;
        }

        public static bool ValidateJSONRequestSessionHasAdminOpsLevel(
            ISessionAdapter session,
            out string error_code)
        {
            bool success = true;
            int session_account_id = GetSessionAccountID(session);
            DatabaseConstants.OpsLevel opsLevel =
                AccountQueries.GetAccountOpsLevel(
                    ApplicationConstants.CONNECTION_STRING,
                    session_account_id,
                    out error_code);

            if (opsLevel != DatabaseConstants.OpsLevel.invalid)
            {
                if (opsLevel >= DatabaseConstants.OpsLevel.admin)
                {
                    error_code = SuccessMessages.GENERAL_SUCCESS;
                    success = true;
                }
                else
                {
                    error_code = ErrorMessages.INSUFFICIENT_OPS_LEVEL;
                }
            }

            return success;
        }

        public static bool IsSessionAuthenticated(ISessionAdapter session)
        {
            bool isAuthenticated = false;

            if (session["Authenticated"] != null)
            {
                isAuthenticated = (bool)session["Authenticated"];
            }

            return isAuthenticated;
        }

        public static bool IsSessionAdminAuthenticated(ISessionAdapter session)
        {
            bool isAdminAuthenticated = false;

            if (session["Authenticated"] != null && session["OpsLevel"] != null)
            {
                isAdminAuthenticated = (bool)session["Authenticated"] && (bool)session["OpsLevel"];
            }

            return isAdminAuthenticated;
        }

        public static int GetSessionAccountID(ISessionAdapter session)
        {
            return (int)session["AccountId"];
        }

        public static string GetSessionUsername(ISessionAdapter session)
        {
            return (string)session["Username"];
        }

        public static bool VerifySessionOwnsCharacter(ISessionAdapter session, int character_id)
        {
            int[] characters_ids = (int[])session["CharacterIDs"];
            bool sessionOwnsCharacter = characters_ids.Contains(character_id);

            return sessionOwnsCharacter;
        }

    }
}
