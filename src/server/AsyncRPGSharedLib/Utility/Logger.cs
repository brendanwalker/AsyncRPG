namespace AsyncRPGSharedLib.Utility
{
    public class Logger
    {
        public delegate void LogDelegate(string message);
        private LogDelegate m_logDelegate;

        public Logger(LogDelegate logDelegate)
        {
            m_logDelegate = logDelegate;
        }

        public void LogInfo(string message)
        {
            m_logDelegate("[INFO] " + message);
        }

        public void LogWarning(string message)
        {
            m_logDelegate("[WARN] " + message);
        }

        public void LogError(string message)
        {
            m_logDelegate("[ERROR] " + message);
        }
    }
}
