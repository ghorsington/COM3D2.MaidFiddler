using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayerStatus;
using wf;
using ZeroRpc.Net.ServiceProviders;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private Dictionary<string, MethodInfo> playerGetters;
        private Dictionary<string, MethodInfo> playerSetters;

        public string[] GetPlayerStatusList()
        {
            return playerSetters.Keys.ToArray();
        }

        [MethodDocumentation("Sets player value.\n\nArguments:\n1. [String] Value name (from GetPlayerStatusList)\n2. [Any] Value to set")]
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

            method.Invoke(GameMain.Instance.CharacterMgr.status, new[] {val});
        }

        public object GetPlayerData(string valueName)
        {
            if (!playerGetters.TryGetValue(valueName, out MethodInfo method))
                throw new ArgumentException($"There is no getter for: {valueName}");

            return method.Invoke(GameMain.Instance.CharacterMgr.status, new object[0]);
        }

        public void UnlockAllTrophies(bool enableAll = false)
        {
            Trophy.CreateData();

            if (enableAll)
            {
                var commonIdManager =
                        typeof(Trophy).GetField("commonIdManager", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as
                                CsvCommonIdManager;

                if (commonIdManager != null)
                {
                    commonIdManager.enabledIdList.Clear();
                    foreach (var idKvPair in commonIdManager.idMap.ToArray())
                        commonIdManager.enabledIdList.Add(idKvPair.Key);
                }
            }

            var data = Trophy.GetAllDatas(false);

            foreach (Trophy.Data trophyData in data)
                GameMain.Instance.CharacterMgr.status.AddHaveTrophy(trophyData.id);
        }

        public void UnlockAllStockItems()
        {
            var havePartsItems =
                    typeof(Status).GetField("havePartsItems_", BindingFlags.Instance | BindingFlags.NonPublic)
                                  ?.GetValue(GameMain.Instance.CharacterMgr.status) as Dictionary<string, bool>;

            if (havePartsItems != null)
                foreach (string key in havePartsItems.Keys.ToArray())
                    havePartsItems[key] = true;

            foreach (var itemData in Shop.item_data_dic)
                if (itemData.Value is Shop.ItemDataEvent itemDataEvent)
                    GameMain.Instance.CharacterMgr.status.AddHaveItem(itemDataEvent.target_flag_name);
        }

        private void InitPlayerStatus()
        {
            playerSetters = new Dictionary<string, MethodInfo>();
            playerGetters = new Dictionary<string, MethodInfo>();

            foreach (PropertyInfo propertyInfo in typeof(Status).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                MethodInfo methodSet = propertyInfo.GetSetMethod();
                MethodInfo methodGet = propertyInfo.GetGetMethod();
                if (methodSet != null)
                    playerSetters[propertyInfo.Name] = methodSet;
                if (methodGet != null)
                    playerGetters[propertyInfo.Name] = methodGet;
            }
        }
    }
}