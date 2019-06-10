using System;
using System.Collections.Generic;
using System.Linq;
using MaidStatus;
using Yotogis;

namespace COM3D2.MaidFiddler.Core.Hooks
{
    public static class MiscHooks
    {
        //private static YotogiSkillSystem enabledSkillSystem;
        public static bool EnableAllCommands { get; set; }
        public static bool EnableAllScenarios { get; set; }
        public static bool EnableAllScheduleItems { get; set; }
        public static bool EnableAllStagesVisible { get; set; }
        public static bool EnableYotogiSkills { get; set; }
        public static bool EnableAllDances { get; set; }
        public static bool DisplayNTRSkills { get; set; }
        public static bool EnableAllFreeModeItems { get; set; }

        public static event EventHandler DummySkillTreeCreationStart;
        public static event EventHandler DummySkillTreeCreationEnd;

        public static void OnSceneFreeModeSelectAwake(SceneFreeModeSelectManager mgr)
        {
            if (!EnableAllFreeModeItems)
                return;

            var root = mgr.gameObject.transform.parent.gameObject;
            var buttonObj = UTY.GetChildObject(root, "MenuSelect/Menu/FreeModeMenuButton/夜伽");
            buttonObj.SetActive(true);
            var button = buttonObj.GetComponent<UIButton>();
            button.isEnabled = true;
        }

        public static bool GetLifeModeScenarioExecuteCount(out int result)
        {
            result = 9999;
            return EnableAllFreeModeItems;
        }

        public static bool GetFreeModeItemEnabled(out bool result)
        {
            result = true;
            return EnableAllFreeModeItems;
        }

        public static bool GetAllDanceRelease(out bool result)
        {
            result = true;
            return EnableAllDances;
        }

        public static bool IsStageYotogiPlayable(out bool result)
        {
            result = true;
            return EnableAllStagesVisible;
        }

        public static bool WorkIdReset()
        {
            return EnableAllScheduleItems;
        }

        public static bool ToggleWork(out bool result)
        {
            result = true;
            return EnableAllScheduleItems || (EnableAllFreeModeItems && SceneFreeModeSelectManager.IsFreeMode);
        }

        public static bool ScenarioCheckPlayableCondition(out bool result, ref List<Maid> eventMaids)
        {
            result = true;

            if (EnableAllScenarios)
            {
                eventMaids.Clear();
                var list = eventMaids;
                eventMaids.AddRange(GameMain.Instance.CharacterMgr.GetStockMaidList().Where(m => !list.Contains(m)));
            }

            return EnableAllScenarios;
        }

        public static bool GetIsScenarioPlayable(out bool result, ref List<Maid> eventMaids)
        {
            return ScenarioCheckPlayableCondition(out result, ref eventMaids);
        }

        public static bool GetNTRLockPostfix(bool ntrLocked)
        {
            return !DisplayNTRSkills && ntrLocked;
        }

        public static bool PrefixCreateDatas(out Dictionary<int, YotogiSkillListManager.Data> result,
                                             Status status,
                                             bool specialConditionCheck,
                                             Skill.Data.SpecialConditionType type)
        {
            result = new Dictionary<int, YotogiSkillListManager.Data>();
            if (!EnableYotogiSkills)
                return false;

            YotogiSkillSystem skillSystem = CreateDummySkillSystem(status);

            foreach (var skillDatas in Skill.skill_data_list)
            foreach (var idSkillPair in skillDatas)
            {
                Skill.Data skill = idSkillPair.Value;

                if (!skill.IsExecPersonal(status.personal) || specialConditionCheck && skill.specialConditionType != type)
                    continue;

                YotogiSkillData skillData = skillSystem.Get(skill.id);
                if (skillData == null)
                {
                    skillData = new YotogiSkillData {data = skill, oldData = skill.oldData};
                    skillData.expSystem.SetExreienceList(new List<int>(skill.skill_exp_table));
                }

                var data = new YotogiSkillListManager.Data
                {
                        skillData = skill,
                        conditionDatas = new KeyValuePair<string[], bool>[0],
                        maidStatusSkillData = skillData
                };
                result.Add(skill.id, data);
            }

            return true;
        }

        public static bool IsExecStage(out bool result)
        {
            result = true;
            return EnableYotogiSkills;
        }

        public static bool PrefixCreateDatasOld(out Dictionary<int, YotogiSkillListManager.Data> result, Status status)
        {
            result = new Dictionary<int, YotogiSkillListManager.Data>();
            if (!EnableYotogiSkills)
                return false;

            YotogiSkillSystem skillSystem = CreateDummySkillSystem(status);

            foreach (var skillDatas in Skill.Old.skill_data_list)
            foreach (var idSkillPair in skillDatas)
            {
                Skill.Old.Data skill = idSkillPair.Value;

                YotogiSkillData skillData = skillSystem.Get(skill.id);
                if (skillData == null)
                {
                    skillData = new YotogiSkillData {oldData = skill};
                    skillData.expSystem.SetExreienceList(new List<int>(skill.skill_exp_table));
                }

                var data = new YotogiSkillListManager.Data
                {
                        skillDataOld = skill,
                        conditionDatas = new KeyValuePair<string[], bool>[0],
                        maidStatusSkillData = skillData
                };
                result.Add(skill.id, data);
            }

            return true;
        }

        public static bool GetYotogiSkill(Status status, out YotogiSkillSystem skillData)
        {
            skillData = null;
            if (!EnableYotogiSkills || GameMainHooks.IsDeserializing)
                return false;
            skillData = CreateDummySkillSystem(status);
            return true;
        }

        public static bool CheckCommandEnabled(out bool result)
        {
            result = true;
            return EnableAllCommands;
        }

        private static YotogiSkillSystem CreateDummySkillSystem(Status status)
        {
            DummySkillTreeCreationStart?.Invoke(null, EventArgs.Empty);
            YotogiSkillSystem enabledSkillSystem = new YotogiSkillSystem(status);

            foreach (var skills in Skill.skill_data_list)
            foreach (var skillDataPair in skills)
            {
                if (!skillDataPair.Value.IsExecPersonal(status.personal))
                    continue;
                YotogiSkillData data = enabledSkillSystem.Get(skillDataPair.Value.id) ?? enabledSkillSystem.Add(skillDataPair.Value);
                data.expSystem.SetLevel(data.expSystem.GetMaxLevel());
                data.playCount = 9999;
            }

            foreach (var skills in Skill.Old.skill_data_list)
            foreach (var skillDataPair in skills)
            {
                YotogiSkillData data = enabledSkillSystem.Get(skillDataPair.Value.id) ?? enabledSkillSystem.Add(skillDataPair.Value);
                data.expSystem.SetLevel(data.expSystem.GetMaxLevel());
                data.playCount = 9999;
            }

            DummySkillTreeCreationEnd?.Invoke(null, EventArgs.Empty);
            return enabledSkillSystem;
        }
    }
}