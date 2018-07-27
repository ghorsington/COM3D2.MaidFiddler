using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using COM3D2.MaidFiddler.Core.Hooks;
using PlayerStatus;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private Dictionary<string, MethodInfo> playerGetters;
        private Dictionary<string, MethodInfo> playerSetters;
        private Dictionary<string, bool> playerStatusLocks;

        public string[] GetPlayerStatusList()
        {
            return playerSetters.Keys.ToArray();
        }

        public void SetPlayerData(string valueName, object value)
        {
            if (!playerSetters.TryGetValue(valueName, out MethodInfo method))
                throw new ArgumentException($"There is no such value name: {valueName}");
            Type paramType = method.GetParameters()[0].ParameterType;

            object val;
            try
            {
                val = Convert.ChangeType(value, paramType);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Cannot convert value to {paramType.FullName}.", e);
            }

            if (playerStatusLocks.TryGetValue(valueName, out bool prev))
                playerStatusLocks[valueName] = false;

            method.Invoke(GameMain.Instance.CharacterMgr.status, new[] {val});

            if (prev)
                playerStatusLocks[valueName] = true;
        }

        public object GetPlayerData(string valueName)
        {
            if (!playerGetters.TryGetValue(valueName, out MethodInfo method))
                throw new ArgumentException($"There is no getter for: {valueName}");

            return method.Invoke(GameMain.Instance.CharacterMgr.status, new object[0]);
        }

        public Dictionary<string, object> GetAllPlayerData()
        {
            var result = new Dictionary<string, object>();

            var settableProps = new Dictionary<string, object>();
            result["props"] = settableProps;

            foreach (var setter in playerSetters)
                settableProps[setter.Key] = playerGetters[setter.Key].Invoke(GameMain.Instance.CharacterMgr.status, new object[0]);

            result["locked_props"] = playerStatusLocks.Where(p => p.Value).Select(p => p.Key).ToList();

            return result;
        }

        public void TogglePlayerStatusLock(string prop, bool state)
        {
            playerStatusLocks[prop] = state;
        }

        private void ShouldPlayerPropChange(object sender, PlayerStatusChangeArgs e)
        {
            e.Locked = playerStatusLocks[e.Status];
        }

        private void OnPlayerStatusChanged(object sender, PlayerStatusChangeArgs e)
        {
            if (GameMain.Instance.CharacterMgr == null || GameMain.Instance.CharacterMgr.status == null)
                return;

            Emit("player_prop_changed",
                 new Dictionary<string, object>
                 {
                         ["prop_name"] = e.Status,
                         ["value"] = playerGetters[e.Status].Invoke(GameMain.Instance.CharacterMgr.status, new object[0])
                 });
        }

        private void InitPlayerStatus()
        {
            playerSetters = new Dictionary<string, MethodInfo>();
            playerGetters = new Dictionary<string, MethodInfo>();
            playerStatusLocks = new Dictionary<string, bool>();

            PlayerStatusHooks.ShouldPropertyChange += ShouldPlayerPropChange;
            PlayerStatusHooks.PropertyChanged += OnPlayerStatusChanged;

            foreach (PropertyInfo propertyInfo in typeof(Status).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                MethodInfo methodSet = propertyInfo.GetSetMethod();
                MethodInfo methodGet = propertyInfo.GetGetMethod();
                if (methodSet != null && methodGet != null)
                {
                    playerSetters[propertyInfo.Name] = methodSet;
                    playerStatusLocks[propertyInfo.Name] = false;
                }

                if (methodGet != null)
                    playerGetters[propertyInfo.Name] = methodGet;
            }
        }
    }
}