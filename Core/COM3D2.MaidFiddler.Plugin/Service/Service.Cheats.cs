using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using COM3D2.MaidFiddler.Core.Hooks;
using PlayerStatus;
using wf;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        public void SetUnlockRanges(bool value)
        {
            MathUtilHooks.UnlockRange = value;
        }

        public void SetShowAllYotogiSkills(bool value)
        {
            MiscHooks.EnableYotogiSkills = value;
        }

        public void SetEnableAllYotogiCommand(bool value)
        {
            MiscHooks.EnableAllCommands = value;
        }

        public void SetEnableAllScenarios(bool value)
        {
            MiscHooks.EnableAllScenarios = value;
        }

        public void UnlockAllTrophies(bool enableAll = false)
        {
            Trophy.CreateData();

            if (enableAll)
                if (typeof(Trophy).GetField("commonIdManager", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) is
                            CsvCommonIdManager commonIdManager)
                {
                    commonIdManager.enabledIdList.Clear();
                    foreach (var idKvPair in commonIdManager.idMap.ToArray())
                        commonIdManager.enabledIdList.Add(idKvPair.Key);
                }

            var data = Trophy.GetAllDatas(false);

            foreach (Trophy.Data trophyData in data)
                GameMain.Instance.CharacterMgr.status.AddHaveTrophy(trophyData.id);
        }

        public void UnlockAllStockItems()
        {
            if (typeof(Status).GetField("havePartsItems_", BindingFlags.Instance | BindingFlags.NonPublic)
                              ?.GetValue(GameMain.Instance.CharacterMgr.status) is Dictionary<string, bool> havePartsItems)
                foreach (string key in havePartsItems.Keys.ToArray())
                    havePartsItems[key] = true;

            foreach (var itemData in Shop.item_data_dic)
                if (itemData.Value is Shop.ItemDataEvent itemDataEvent)
                    GameMain.Instance.CharacterMgr.status.AddHaveItem(itemDataEvent.target_flag_name);
        }
    }
}