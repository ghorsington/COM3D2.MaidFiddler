using System;
using MaidStatus;
using Yotogis;

namespace COM3D2.MaidFiddler.Core.Hooks
{
    public class YotogiSkillEventArgs : EventArgs
    {
        public string Event { get; internal set; }
        public Maid Maid { get; internal set; }
        public int SkillId { get; internal set; }
    }

    public static class YotogiSkillHooks
    {
        public static event EventHandler<YotogiSkillEventArgs> SkillInfoChanged;

        public static void OnYotogiSkillAdd(YotogiSkillSystem skillSystem, Skill.Data data)
        {
            if (skillSystem.status.maid == null)
                return;

            SkillInfoChanged?.Invoke(null, new YotogiSkillEventArgs {Maid = skillSystem.status.maid, Event = "add", SkillId = data.id});
        }

        public static void OnYotogiSkillOldAdd(YotogiSkillSystem skillSystem, Skill.Old.Data data)
        {
            if (skillSystem.status.maid == null)
                return;

            SkillInfoChanged?.Invoke(null, new YotogiSkillEventArgs {Maid = skillSystem.status.maid, Event = "add", SkillId = data.id});
        }

        public static void OnYotogiSkillRemove(YotogiSkillSystem skillSystem, int id)
        {
            if (skillSystem.status.maid == null)
                return;

            SkillInfoChanged?.Invoke(null, new YotogiSkillEventArgs {Maid = skillSystem.status.maid, Event = "remove", SkillId = id});
        }
    }
}