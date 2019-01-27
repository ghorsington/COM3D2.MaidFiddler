using System;

namespace COM3D2.MaidFiddler.Common.IPC {
    public class RemoteException : Exception
    {
        public string RemoteStackTrace { get; }

        public RemoteException(string message, string remoteStackTrace) : base(message)
        {
            RemoteStackTrace = remoteStackTrace;
        }
    }
}