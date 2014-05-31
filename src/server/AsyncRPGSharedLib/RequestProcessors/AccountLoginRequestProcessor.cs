using System;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class AccountLoginRequestProcessor : RequestProcessor
    {
        // Request Data
        string m_username;
        string m_password;

        // Result
        int m_accountId = -1;
        int m_opsLevel = 0;
        string m_emailAddress = "";
        int[] m_characterIDs = null;

        public AccountLoginRequestProcessor(
            string username,
            string password)
        {
            m_username = username;
            m_password = password;
        }

        public int AccountID
        {
            get { return m_accountId; }
        }

        public int OpsLevel
        {
            get { return m_opsLevel; }
        }

        public string EmailAddress
        {
            get { return m_emailAddress; }
        }

        public int[] AccountCharacterIDs
        {
            get { return m_characterIDs; }
        }

        protected override bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            bool success;

            if (AccountQueries.VerifyCredentials(
                    requestCache.DatabaseContext,
                    m_username,
                    m_password,
                    out m_accountId,
                    out m_emailAddress,
                    out m_opsLevel,
                    out result_code))
            {
                CharacterQueries.GetCharacterIDList(requestCache.DatabaseContext, m_accountId, out m_characterIDs);

                result_code= SuccessMessages.GENERAL_SUCCESS;
                success= true;
            }
            else
            {
                m_characterIDs = new int[] {};
                success= false;
            }

            return success;
        }
    }
}
