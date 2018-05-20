using System;
using ZeroRpc.Net;
using Dict = System.Collections.Generic.Dictionary<string, object>;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private Client client;

        private bool HasEventHandler => client != null;

        public void SubscribeToEventHandler(string address)
        {
            Unsubscribe();

            client = new Client();
            client.Connect(address);
            Console.WriteLine($"Subscribed to event handler on {address}");
        }

        internal void Unsubscribe()
        {
            if (client == null)
                return;
            client.Dispose();
            client = null;
        }

        private void Emit(string eventName, Dict arguments)
        {
            client?.InvokeAsync("emit", eventName, arguments);
        }
    }
}
