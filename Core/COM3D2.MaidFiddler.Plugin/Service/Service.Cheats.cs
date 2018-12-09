using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using COM3D2.MaidFiddler.Core.Hooks;
using MaidStatus;
using PlayerStatus;
using wf;
using Yotogis;
using Status = PlayerStatus.Status;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private static readonly HashSet<string> maxBlacklist =
                new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) {"employmentDay", "popularRank"};

        public bool ToggleAllParametersLockActive(bool toggle)
        {
            if (selectedMaid == null)
                return false;

            var locks = maidLockList[selectedMaidGuid];

            foreach (string param in locks.Keys.ToArray())
                locks[param] = toggle;

            return true;
        }

        public bool MaxAllActive()
        {
            if (selectedMaid == null)
                return false;

            EmitEvents = false;
            MaxAll(selectedMaid);
            EmitEvents = true;

            return true;
        }

        public bool FixYotogiSkillsActive()
        {
            if (selectedMaid == null || MiscHooks.EnableYotogiSkills)
                return false;

            EmitEvents = false;
            FixYotogiSkills(selectedMaid);
            EmitEvents = true;

            return true;
        }

        public bool FixYotogiSkillsAll()
        {
            if (MiscHooks.EnableYotogiSkills)
                return false;

            EmitEvents = false;
            foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
                FixYotogiSkills(maid);
            EmitEvents = true;
            return true;
        }

        public bool MaxAllForAllMaids()
        {
            EmitEvents = false;
            foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
                MaxAll(maid);
            EmitEvents = true;
            return true;
        }

        public bool UnlockAllYotogiSkillsActive(bool max = false)
        {
            if (selectedMaid == null)
                return false;
            EmitEvents = false;
            UnlockAllYotogiSkills(selectedMaid, max);
            EmitEvents = true;
            return true;
        }

        public bool UlockAllJobClassActive(bool max = false)
        {
            if (selectedMaid == null)
                return false;
            EmitEvents = false;
            UnlockAllJobClass(selectedMaid, max);
            EmitEvents = true;
            return true;
        }

        public bool UnlockAllYotogiClassActive(bool max = false)
        {
            if (selectedMaid == null)
                return false;
            EmitEvents = false;
            UnlockAllYotogiClass(selectedMaid, max);
            EmitEvents = true;
            return true;
        }

        public bool UnlockAllActive(bool max = false)
        {
            if (selectedMaid == null)
                return false;
            EmitEvents = false;
            UnlockAllYotogiClass(selectedMaid, max);
            UnlockAllJobClass(selectedMaid, max);
            UnlockAllYotogiSkills(selectedMaid, max);
            EmitEvents = true;
            return true;
        }

        public bool UnlockAllForAllMaids()
        {
            EmitEvents = false;

            foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                UnlockAllYotogiClass(maid, true);
                UnlockAllJobClass(maid, true);
                UnlockAllYotogiSkills(maid, true);
            }

            EmitEvents = true;
            return true;
        }

        public void SetAllFreeModeItemsVisible(bool toggle)
        {
            MiscHooks.EnableAllFreeModeItems = toggle;
        }

        public void SetAllNTRSkillsVisible(bool toggle)
        {
            MiscHooks.DisplayNTRSkills = toggle;
        }

        public void SetAllYotogiStagesVisible(bool toggle)
        {
            MiscHooks.EnableAllStagesVisible = toggle;
        }

        public void SetEnableAllDances(bool value)
        {
            MiscHooks.EnableAllDances = value;
        }

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

        public void SetEnableAllScheduleItems(bool value)
        {
            MiscHooks.EnableAllScheduleItems = value;
        }

        public void MaxCredits()
        {
            Status status = GameMain.Instance.CharacterMgr.status;

            status.casinoCoin = 999999999;
            status.money = 999999999;
        }

        public void MaxGrade()
        {
            Status status = GameMain.Instance.CharacterMgr.status;

            status.clubGrade = 5;
            status.baseClubEvaluation = 999;
            status.clubGauge = 100;
        }

        public void MaxFacilityGrades()
        {
            if (typeof(FacilityManager).GetField("m_FacilityExpArray", BindingFlags.NonPublic | BindingFlags.Instance)
                                       ?.GetValue(GameMain.Instance.FacilityMgr) is Dictionary<int, SimpleExperienceSystem> facilityExp)
                foreach (var exp in facilityExp)
                    exp.Value.SetLevel(exp.Value.GetMaxLevel());

            foreach (Facility facility in GameMain.Instance.FacilityMgr.GetFacilityArray())
            {
                if (facility == null)
                    continue;
                facility.expSystem.SetLevel(facility.expSystem.GetMaxLevel());
                facility.facilityValuation = 9999;
                facility.facilityExperienceValue = 9999;
                facility.facilityIncome = 999999999;
            }
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
                {
                    GameMain.Instance.CharacterMgr.status.AddHaveItem(itemDataEvent.target_flag_name);
                }
                else
                {
                    GameMain.Instance.CharacterMgr.status.AddShopLineup(itemData.Value.id);
                    GameMain.Instance.CharacterMgr.status.SetShopLineupStatus(itemData.Value.id, ShopItemStatus.Purchased);
                }
        }

        private void FixYotogiSkills(Maid maid)
        {
            foreach (int i in maid.status.yotogiSkill.datas.GetKeyArray())
            {
                var skillData = Skill.Get(i);
                if(!skillData.IsExecPersonal(maid.status.personal))
                    maid.status.yotogiSkill.Remove(i);
            }
        }

        private void UnlockAllYotogiSkills(Maid maid, bool max)
        {
            foreach (var skillList in Skill.skill_data_list)
            foreach (var skillPair in skillList)
            {
                YotogiSkillData skill = maid.status.yotogiSkill.Get(skillPair.Key) ?? maid.status.yotogiSkill.Add(skillPair.Value);
                if (max)
                    skill.expSystem.SetLevel(skill.expSystem.GetMaxLevel());
            }

            foreach (var skillList in Skill.Old.skill_data_list)
            foreach (var skillPair in skillList)
            {
                YotogiSkillData skill = maid.status.yotogiSkill.Get(skillPair.Key) ?? maid.status.yotogiSkill.Add(skillPair.Value);
                if (max)
                    skill.expSystem.SetLevel(skill.expSystem.GetMaxLevel());
            }

            // Run skill fixing in case there were invalid skills prior to running this unlock
            FixYotogiSkills(maid);
        }

        private void UnlockAllJobClass(Maid maid, bool max)
        {
            foreach (JobClass.Data data in JobClass.GetAllDatas(true))
            {
                var job = maid.status.jobClass.Get(data.id) ?? maid.status.jobClass.Add(data, true);
                if (max)
                    job?.expSystem.SetLevel(job.expSystem.GetMaxLevel());
            }
        }

        private void UnlockAllYotogiClass(Maid maid, bool max)
        {
            foreach (YotogiClass.Data data in YotogiClass.GetAllDatas(true))
            {
                var job = maid.status.yotogiClass.Get(data.id) ?? maid.status.yotogiClass.Add(data, true);
                if (max)
                    job?.expSystem.SetLevel(job.expSystem.GetMaxLevel());
            }
        }

        private void MaxAll(Maid maid)
        {
            GloballyUnlocked = true;
            MaidStatus.Status status = maid.status;

            foreach (var setterInfo in maidSetters)
            {
                if (maxBlacklist.Contains(setterInfo.Key))
                    continue;

                object val = null;

                if (setterInfo.Value.GetParameters()[0].ParameterType == typeof(int))
                    val = 99999;
                else if (setterInfo.Value.GetParameters()[0].ParameterType == typeof(long))
                    val = 99999L;

                if (setterInfo.Key.Equals("studyRate", StringComparison.InvariantCultureIgnoreCase))
                    val = 0;

                if (val != null)
                    setterInfo.Value.Invoke(status, new[] {val});
            }

            foreach (YotogiSkillData yotogiSkillData in status.yotogiSkill.datas.GetValueArray())
                yotogiSkillData.expSystem.SetLevel(yotogiSkillData.expSystem.GetMaxLevel());

            foreach (WorkData workData in status.workDatas.GetValueArray())
            {
                workData.level = 10;
                workData.playCount = 9999;
            }

            GloballyUnlocked = false;
        }
    }
}