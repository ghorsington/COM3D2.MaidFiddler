using System.Collections;
using COM3D2.MaidFiddler.Core.IPC;
using COM3D2.MaidFiddler.Core.Utils;
using UnityEngine;
using Dict = System.Collections.Generic.Dictionary<string, object>;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        public PipeEventEmitter eventServer;
        private Coroutine emitLoop;

        private bool EmitEvents { get; set; } = true;

        public void DisconnectEventHander()
        {
            Unsubscribe();
        }

        internal IEnumerator EmitLoop()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                eventServer.EmitEvents();
            }
        }

        internal void Unsubscribe()
        {
            eventServer.Disconnect();
        }

        private void InitEventEmitter()
        {
            Debugger.WriteLine(LogLevel.Info, "Creating event emitter");
            eventServer = new PipeEventEmitter("MaidFildderEventEmitter");
            emitLoop = parent.StartCoroutine(EmitLoop());
        }

        private void Emit(string eventName, Dict arguments)
        {
            if (eventServer.IsConnected && EmitEvents)
                eventServer.AddEvent(eventName, arguments);
        }
    }
}