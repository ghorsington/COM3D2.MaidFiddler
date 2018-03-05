using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using COM3D2.MaidFiddler.Plugin.Utils;
using PlayerStatus;

namespace COM3D2.MaidFiddler.Plugin.Service
{
    public partial class MaidFiddlerServiceProvider
    {
        private Dictionary<string, MethodInfo> playerSetters;

        private void InitPlayerStatus()
        {
            playerSetters = new Dictionary<string, MethodInfo>();

            foreach (PropertyInfo propertyInfo in typeof(Status).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                MethodInfo method = propertyInfo.GetSetMethod();
                if(method != null)
                    playerSetters[propertyInfo.Name] = method;
            }
        }

        public void SetCredits(object credits)
        {
            if(!long.TryParse(credits.ToString(), out long cred))
                throw new ArgumentException("The value must be a number less than 2^63.");
            GameMain.Instance.CharacterMgr.status.money = cred;
        }

        public string[] GetPlayerStatusList()
        {
            return playerSetters.Keys.ToArray();
        }

        public void SetPlayerData(string valueName, object value)
        {
            if(!playerSetters.TryGetValue(valueName, out MethodInfo method))
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

            method.Invoke(GameMain.Instance.CharacterMgr.status, new [] { val });
        }

        public void UnlockAllTrophies(bool enableAll = false)
        {
            Trophy.CreateData();

            if (enableAll)
            {
                wf.CsvCommonIdManager commonIdManager = typeof(Trophy).GetField("commonIdManager", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as wf.CsvCommonIdManager;

                if (commonIdManager != null)
                {
                    commonIdManager.enabledIdList.Clear();
                    foreach (KeyValuePair<int, KeyValuePair<string, string>> idKvPair in commonIdManager.idMap.ToArray())
                        commonIdManager.enabledIdList.Add(idKvPair.Key);
                }
            }

            List<Trophy.Data> data = Trophy.GetAllDatas(false);

            foreach (Trophy.Data trophyData in data)
            {
                GameMain.Instance.CharacterMgr.status.AddHaveTrophy(trophyData.id);
            }
        }

        public void UnlockAllStockItems()
        {
            try
            {
                Dictionary<string, bool> havePartsItems = typeof(Status).GetField("havePartsItems_", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(GameMain.Instance.CharacterMgr.status) as Dictionary<string, bool>;

                if (havePartsItems != null)
                {
                    foreach (string key in havePartsItems.Keys.ToArray())
                    {
                        havePartsItems[key] = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debugger.WriteLine(LogLevel.Error, $"Error while unlockin stock items: {e}");
            }
            

            foreach (KeyValuePair<int, Shop.ItemDataBase> itemData in Shop.item_data_dic)
            {
                if (itemData.Value is Shop.ItemDataEvent itemDataEvent)
                {
                    GameMain.Instance.CharacterMgr.status.AddHaveItem(itemDataEvent.target_flag_name);
                }
            }
        }
    }
}
