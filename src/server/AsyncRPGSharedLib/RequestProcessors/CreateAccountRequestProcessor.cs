using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class CreateAccountRequestProcessor : RequestProcessor
    {
        // Request Data
        private string m_username;
        private string m_password;
        private string m_emailAddress;
        private string m_webServiceURL;

        // Result Data
        private string m_emailVerificationString;

        public CreateAccountRequestProcessor(
            string username,
            string password,
            string emailAddress,
            string webServiceURL)
        {
            m_username = username;
            m_password = password;
            m_emailAddress = emailAddress;
            m_webServiceURL = webServiceURL;

            m_emailVerificationString = "";
        }

        public string EMailVerificationString
        {
            get { return m_emailVerificationString; }
        }

        override protected bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success = true;
            string emailVerificationString = "";

            result_code = SuccessMessages.GENERAL_SUCCESS;

            // If the username contains any non-alphanumeric Characters, we can't accept it
            {
                Regex rgx = new Regex("[^a-zA-Z0-9]");

                m_username = rgx.Replace(m_username, "");
            }

            if (success &&
                m_username.Length == 0)
            {
                result_code = ErrorMessages.INVALID_USERNAME;
                success = false;
            }

            if (success && m_password.Length == 0)
            {
                result_code = ErrorMessages.INVALID_PASSWORD;
                success = false;
            }

            if (success &&
                !AsyncRPGSharedLib.Protocol.EMail.isWellFormedAddress(m_emailAddress))
            {
                result_code = ErrorMessages.MALFORMED_EMAIL;
                success = false;
            }

            // Make sure the username isn't already taken
            if (success &&
                !AccountQueries.VerifyUsernameAvailable(requestCache.DatabaseContext, m_username))
            {
                result_code = ErrorMessages.RESERVED_USERNAME;
                success = false;
            }

            if (success)
            {
                if (MailConstants.VerifyAccountEmail)
                {
                    AccountQueries.CreateAccount(
                        requestCache.DatabaseContext,
                        m_username,
                        m_password,
                        m_emailAddress,
                        out m_emailVerificationString);

                    if (AsyncRPGSharedLib.Protocol.EMail.SendVerifyAccountMessage(
                            m_emailAddress,
                            m_webServiceURL,
                            m_username,
                            emailVerificationString))
                    {
                        result_code = SuccessMessages.GENERAL_SUCCESS + "! Sending verification e-mail to " + m_emailAddress+".";
                    }
                    else
                    {
                        result_code = ErrorMessages.SMTP_ERROR;
                        success = false;
                    }
                }
                else
                {
                    AccountQueries.CreateAccountNoEmailVerify(
                        requestCache.DatabaseContext,
                        m_username,
                        m_password,
                        m_emailAddress,
                        DatabaseConstants.OpsLevel.player);

                    result_code = SuccessMessages.GENERAL_SUCCESS + "! Account now active (email verification off).";
                }
            }

            return success;
        }
    }
}
