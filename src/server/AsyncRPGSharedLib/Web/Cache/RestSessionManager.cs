using AsyncRPGSharedLib.Utility;
using System;
using System.Collections.Generic;
using System.Net;

namespace AsyncRPGSharedLib.Web.Cache
{
    public class RestSessionManager
    {
        public const int SESSION_KEY_BITS = 128;
        public const string SESSION_KEY = "ASP.NET_SessionId";
        public const int SESSION_EXPIRATION_MINUTES = 60;

        private Dictionary<string, RestSession> m_sessions;

        public RestSessionManager()
        {
            m_sessions = new Dictionary<string, RestSession>();
        }

        public RestSession GetSession(
            RestRequest request, 
            out bool newSession)
        {
            Cookie sessionCookie = request.Cookies[SESSION_KEY];
            string sessionId = "";
            RestSession session = null;

            if (sessionCookie != null)
            {
                sessionId = sessionCookie.Value;
                newSession = false;
            }
            else
            {
                sessionId = GenerateNewSessionKey();
                newSession = true;
            }

            if (!m_sessions.TryGetValue(sessionId, out session))
            {
                // TODO: What about expiring old sessions?
                session = new RestSession(sessionId);
                m_sessions.Add(sessionId, session);
            }            

            return session;
        }

        public void FreeSession(RestSession session)
        {
            if (m_sessions.ContainsKey(session.SessionID))
            {
                m_sessions.Remove(session.SessionID);
            }
        }

        private string GenerateNewSessionKey()
        {
            // Generate a random 128-bit string
            string newSessionKey = RNGUtilities.CreateNonDeterministicRandomBase64String(SESSION_KEY_BITS);

            // Make sure this is actually a new key
            while (m_sessions.ContainsKey(newSessionKey))
            {
                newSessionKey = RNGUtilities.CreateNonDeterministicRandomBase64String(SESSION_KEY_BITS);
            }

            return newSessionKey;
        }
    }
}
