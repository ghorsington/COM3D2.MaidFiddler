using System;

namespace MiniIPC {
    public class RemoteException : Exception
    {
        public string RemoteStackTrace { get; }

        public RemoteException(string message, string remoteStackTrace) : base(message)
        {
            RemoteStackTrace = remoteStackTrace;
        }
    }
}