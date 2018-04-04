﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroRpc.Net;
using Dict = System.Collections.Generic.Dictionary<string, object>;

namespace COM3D2.MaidFiddler.Plugin.Service
{
    public partial class Service
    {
        private Client client;

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
