using System;
using COM3D2.MaidFiddler.Common.Service;

namespace COM3D2.MaidFiddler.GUI.Remoting
{
    public class GameEvents
    {
        public GameEvents()
        {
            EventHandler = new EventHandlerImpl(this);
        }

        public IMaidFiddlerEventHandler EventHandler { get; }
        public event EventHandler OnTest;

        private class EventHandlerImpl : IMaidFiddlerEventHandler
        {
            private readonly GameEvents parent;

            public EventHandlerImpl(GameEvents parent)
            {
                this.parent = parent;
            }

            public void Test()
            {
                parent.OnTest?.Invoke(null, EventArgs.Empty);
            }

            public string Test2(string foo)
            {
                return foo;
            }
        }
    }
}