using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace COM3D2.MaidFiddler.Core.Utils
{
    public class ExecutorHelper : MonoBehaviour
    {
        private List<Action> actions = new List<Action>();

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            var locActions = new List<Action>();
            lock (actions)
            {
                locActions.AddRange(actions);
                actions.Clear();
            }
            
            foreach (var locAction in locActions)
                locAction();
        }

        public void RunSync(Action action)
        {
            var mre = new ManualResetEvent(false);
            Exception ex = null;
            
            lock (actions)
            {
                actions.Add(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                    mre.Set();
                });
            }

            mre.WaitOne();
            if (ex != null)
                throw ex;
        }
    }
}