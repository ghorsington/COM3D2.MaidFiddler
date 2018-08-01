using System;
using System.Collections.Generic;
using MaidStatus;
using Yotogis;

namespace COM3D2.MaidFiddler.Core.Hooks
{
    public static class MiscHooks
    {
        private static YotogiSkillSystem enabledSkillSystem;
        public static bool EnableYotogiSkills { get; set; } = true;
        public static bool EnableAllCommands { get; set; } = true;

        public static bool PrefixCreateDatas(out Dictionary<int, YotogiSkillListManager.Data> result,
                                             Status status,
                                             bool specialConditionCheck,
                                             Skill.Data.SpecialConditionType type)
        {
            result = new Dictionary<int, YotogiSkillListManager.Data>();
            if (!EnableYotogiSkills)
                return false;

            var skillSystem = enabledSkillSystem ?? CreateDummySkillSystem();

            foreach (var skillDatas in Skill.skill_data_list)
            foreach (var idSkillPair in skillDatas)
            {
                Skill.Data skill = idSkillPair.Value;
                
                YotogiSkillData skillData = skillSystem.Get(skill.id);
                if (skillData == null)
                {
                    skillData = new YotogiSkillData {data = skill, oldData = skill.oldData};
                    skillData.expSystem.SetExreienceList(new List<int>(skill.skill_exp_table));
                }

                var data = new YotogiSkillListManager.Data
                {
                        skillData = skill,
                        conditionDatas = new[] {new KeyValuePair<string, bool>("なし", true)},
                        maidStatusSkillData = skillData
                };
                result.Add(skill.id, data);
            }

            return true;
        }

        public static bool PrefixCreateDatasOld(out Dictionary<int, YotogiSkillListManager.Data> result, Status status)
        {
            result = new Dictionary<int, YotogiSkillListManager.Data>();
            if (!EnableYotogiSkills)
                return false;

            var skillSystem = enabledSkillSystem ?? CreateDummySkillSystem();

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
                        conditionDatas = new[] {new KeyValuePair<string, bool>("なし", true)},
                        maidStatusSkillData = skillData
                };
                result.Add(skill.id, data);
            }

            return true;
        }

        public static bool GetYotogiSkill(out YotogiSkillSystem skillData)
        {
            skillData = null;
            if (!EnableYotogiSkills || GameMainHooks.IsDeserializing)
                return false;
            skillData = enabledSkillSystem ?? CreateDummySkillSystem();
            return true;
        }

        private static YotogiSkillSystem CreateDummySkillSystem()
        {
            enabledSkillSystem = new YotogiSkillSystem(null);

            foreach (var skills in Skill.skill_data_list)
            foreach (var skillDataPair in skills)
            {
                YotogiSkillData data = enabledSkillSystem.Add(skillDataPair.Value);
                data.expSystem.SetLevel(data.expSystem.GetMaxLevel());
                data.playCount = 9999;
            }

            foreach (var skills in Skill.Old.skill_data_list)
            foreach (var skillDataPair in skills)
            {
                YotogiSkillData data = enabledSkillSystem.Add(skillDataPair.Value);
                data.expSystem.SetLevel(data.expSystem.GetMaxLevel());
                data.playCount = 9999;
            }

            return enabledSkillSystem;
        }

        public static bool CheckCommandEnabled(out bool result)
        {
            result = true;
            return EnableAllCommands;
        }
    }
}