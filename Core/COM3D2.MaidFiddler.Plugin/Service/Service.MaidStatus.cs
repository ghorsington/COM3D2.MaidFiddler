using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using COM3D2.MaidFiddler.Core.Hooks;
using COM3D2.MaidFiddler.Core.Utils;
using MaidStatus;
using UnityEngine;
using Dict = System.Collections.Generic.Dictionary<string, object>;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private Dictionary<string, MethodInfo> maidGetters;
        private Dictionary<string, MethodInfo> maidSetters;

        public Dictionary<string, string> GetMaidList()
        {
            return GameMain.Instance.CharacterMgr.GetStockMaidList().ToDictionary(m => m.status.guid, m => m.status.fullNameEnStyle);
        }

        public List<Dict> GetAllStockMaids()
        {
            return GameMain.Instance.CharacterMgr.GetStockMaidList().Select(ReadMaidData).ToList();
        }

        public List<Dict> GetAllStockMaidsBasic()
        {
            return GameMain.Instance.CharacterMgr.GetStockMaidList().Select(ReadBasicMaidData).ToList();
        }

        public string[] GetMaidParameterList()
        {
            return maidSetters.Keys.ToArray();
        }

        public object GetMaidProperty(string maidId, string propertyName)
        {
            if (!maidGetters.TryGetValue(propertyName, out MethodInfo getter))
                throw new ArgumentException($"No such property: {propertyName}", nameof(propertyName));

            return getter.Invoke(GetMaid(maidId).status, new object[0]);
        }

        public void SetPersonal(string maidId, int personalId)
        {
            SetPersonal(GetMaid(maidId), personalId);
        }

        public void SetCurrentJobClass(string maidId, object classId)
        {
            SetCurrentJobClass(GetMaid(maidId), classId);
        }

        public void SetCurrentYotogiClass(string maidId, object classId)
        {
            SetCurrentYotogiClass(GetMaid(maidId), classId);
        }

        public void SetContract(string maidId, int contract)
        {
            SetContract(GetMaid(maidId), contract);
        }

        public void SetCurSeikeiken(string maidId, int seikeiken)
        {
            SetCurSeikeiken(GetMaid(maidId), seikeiken);
        }

        public void SetInitSeikeiken(string maidId, int seikeiken)
        {
            SetInitSeikeiken(GetMaid(maidId), seikeiken);
        }

        public void SetMaidProperty(string maidId, string propertyName, object value)
        {
            SetMaidProperty(GetMaid(maidId), propertyName, value);
        }

        public bool TogglePropertyLock(string maidId, string propertyName, bool value)
        {
            if (!maidLockList.TryGetValue(maidId, out var locks))
                return false;

            locks[propertyName] = value;
            return locks[propertyName];
        }

        public void TogglePropensity(string maidId, object propensityId, bool toggle)
        {
            TogglePropensity(GetMaid(maidId), propensityId, toggle);
        }

        public void ToggleFeature(string maidId, object propensityId, bool toggle)
        {
            ToggleFeature(GetMaid(maidId), propensityId, toggle);
        }

        public Dict GetMaidData(string maidId)
        {
            string id = maidId.ToLower(CultureInfo.InvariantCulture);

            var maids = GameMain.Instance.CharacterMgr.GetStockMaidList()
                                .Where(m => m.status.guid.ToLower(CultureInfo.InvariantCulture).StartsWith(id)).ToArray();

            if (maids.Length == 0)
                throw new ArgumentException($"No such maid with ID: {maidId}", nameof(maidId));
            if (maids.Length > 1)
                throw new
                        ArgumentException($"Found multiple maids whose ID starts the same:\n\n{string.Join("\n", maids.Select(m => $"{m.status.fullNameEnStyle}; ID: {m.status.guid}").ToArray())}\nPlease give a more specific ID!");

            return ReadMaidData(maids[0]);
        }

        public void TogglePropensity(Maid maid, object propensityId, bool toggle)
        {
            int id = Convert.ToInt32(propensityId);

            if (toggle)
                maid.status.AddPropensity(id);
            else
                maid.status.RemovePropensity(id);
        }

        public void ToggleFeature(Maid maid, object propensityId, bool toggle)
        {
            int id = Convert.ToInt32(propensityId);

            if (toggle)
                maid.status.AddFeature(id);
            else
                maid.status.RemoveFeature(id);
        }

        public void SetWorkDataLevel(string maidId, object id, object level)
        {
            SetWorkDataLevel(GetMaid(maidId), id, level);
        }

        public void SetWorkPlayCount(string maidId, object id, object playCount)
        {
            SetWorkPlayCount(GetMaid(maidId), id, playCount);
        }

        public void SetNoonWork(string maidId, object id)
        {
            SetNoonWork(GetMaid(maidId), id);
        }

        public void SetNightWork(string maidId, object id)
        {
            SetNightWork(GetMaid(maidId), id);
        }

        private void SetCurrentYotogiClass(Maid maid, object classId)
        {
            int id = Convert.ToInt32(classId);

            YotogiClass.Data data = YotogiClass.GetData(id);

            if (!maid.status.yotogiClass.Contains(data.id))
                maid.status.yotogiClass.Add(data, true);

            maid.status.ChangeYotogiClass(data);
        }

        private void SetPersonal(Maid maid, int personalId)
        {
            maid.status.SetPersonal(personalId);
        }

        private void SetCurrentJobClass(Maid maid, object classId)
        {
            int id = Convert.ToInt32(classId);

            JobClass.Data data = JobClass.GetData(id);

            if (!maid.status.jobClass.Contains(data.id))
                maid.status.jobClass.Add(data, true);

            maid.status.ChangeJobClass(id);
        }

        private void SetContract(Maid maid, int contract)
        {
            maid.status.contract = (Contract) contract;
        }

        private void SetCurSeikeiken(Maid maid, int seikeiken)
        {
            maid.status.seikeiken = (Seikeiken) seikeiken;
        }

        private void SetInitSeikeiken(Maid maid, int seikeiken)
        {
            maid.status.initSeikeiken = (Seikeiken) seikeiken;
        }

        private void SetWorkDataLevel(Maid maid, object id, object level)
        {
            int workId = Convert.ToInt32(id);
            int workLevel = Convert.ToInt32(level);

            maid.status.SetWorkDataLevel(workId, workLevel);
        }

        private void SetWorkPlayCount(Maid maid, object id, object playCount)
        {
            int workId = Convert.ToInt32(id);
            uint workPlayCount = Convert.ToUInt32(playCount);

            uint baseCount = 0;
            if (maid.status.workDatas.ContainsKey(workId))
                baseCount = maid.status.workDatas[workId].playCount;

            maid.status.AddWorkDataPlayCount(workId, (int) (workPlayCount - baseCount));
        }

        private void SetNoonWork(Maid maid, object id)
        {
            int workId = Convert.ToInt32(id);

            maid.status.noonWorkId = workId;
        }

        private void SetNightWork(Maid maid, object id)
        {
            int workId = Convert.ToInt32(id);

            maid.status.nightWorkId = workId;
        }

        private void ToggleYotogiSkill(Maid maid, object id, bool state)
        {
            int skillId = Convert.ToInt32(id);

            Debugger.WriteLine($"Toggling skill {skillId} to state {state}");

            if (state)
                maid.status.yotogiSkill.Add(skillId);
            else
                maid.status.yotogiSkill.Remove(skillId);
        }

        private void SetYotogiSkillLevel(Maid maid, object id, object level)
        {
            int skillId = Convert.ToInt32(id);
            int skillLevel = Convert.ToInt32(level);

            YotogiSkillData skill = maid.status.yotogiSkill.Get(skillId) ?? maid.status.yotogiSkill.Add(skillId);
            skill.expSystem.SetLevel(skillLevel);
        }

        private void SetYotogiSkillExp(Maid maid, object id, object exp)
        {
            int skillId = Convert.ToInt32(id);
            int skillExp = Convert.ToInt32(exp);

            YotogiSkillData skill = maid.status.yotogiSkill.Get(skillId) ?? maid.status.yotogiSkill.Add(skillId);
            skill.expSystem.SetTotalExp(skillExp);
        }

        private void SetYotogiSkillPlayCount(Maid maid, object id, object playCount)
        {
            int skillId = Convert.ToInt32(id);
            uint skillPlayCount = Convert.ToUInt32(playCount);

            YotogiSkillData skill = maid.status.yotogiSkill.Get(skillId) ?? maid.status.yotogiSkill.Add(skillId);
            skill.playCount = skillPlayCount;
        }

        private void SetMaidProperty(Maid maid, string propertyName, object value)
        {
            if (!maidSetters.TryGetValue(propertyName, out MethodInfo setter))
                throw new ArgumentException($"No such property: {propertyName}", nameof(propertyName));
            Type paramType = setter.GetParameters()[0].ParameterType;

            object val;
            if (paramType.IsEnum)
                val = Enum.ToObject(paramType, (int) value);
            else
                try
                {
                    val = Convert.ChangeType(value, paramType);
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"Cannot convert value to {paramType.FullName}.", e);
                }

            try
            {
                var locks = maidLockList[maid.status.guid];
                bool prev = locks[propertyName];
                locks[propertyName] = false;
                setter.Invoke(maid.status, new[] {val});
                locks[propertyName] = prev;
            }
            catch (Exception e)
            {
                Debugger.WriteLine(LogLevel.Error, e.InnerException.ToString());
            }
        }

        private Maid GetMaid(string maidId)
        {
            string id = maidId.ToLower(CultureInfo.InvariantCulture);

            var maids = GameMain.Instance.CharacterMgr.GetStockMaidList()
                                .Where(m => m.status.guid.ToLower(CultureInfo.InvariantCulture).StartsWith(id)).ToArray();

            if (maids.Length == 0)
                throw new ArgumentException($"No such maid with ID: {maidId}", nameof(maidId));
            if (maids.Length > 1)
                throw new
                        ArgumentException($"Found multiple maids whose ID starts the same:\n\n{string.Join("\n", maids.Select(m => $"{m.status.fullNameEnStyle}; ID: {m.status.guid}").ToArray())}\nPlease give a more specific ID!");

            return maids[0];
        }

        private Dict ReadBasicMaidData(Maid maid)
        {
            if (maid == null)
                return null;

            var result = new Dict
            {
                    ["guid"] = maid.status.guid,
                    ["firstName"] = maid.status.firstName,
                    ["lastName"] = maid.status.lastName,
                    ["thumbnail"] = maid.GetThumIcon()?.EncodeToPNG()
            };

            return result;
        }

        private Dict ReadMaidData(Maid maid)
        {
            if (maid == null || maid.status == null)
                return null;

            var result = new Dict();

            var props = new Dict();
            result["properties"] = props;

            foreach (var getter in maidGetters)
                if (maidSetters.ContainsKey(getter.Key))
                    if (getter.Value.ReturnType.IsEnum)
                        props[getter.Key] = (int) getter.Value.Invoke(maid.status, new object[0]);
                    else
                        props[getter.Key] = getter.Value.Invoke(maid.status, new object[0]);

            props["cur_seikeiken"] = (int) maid.status.seikeiken;
            props["init_seikeiken"] = (int) maid.status.initSeikeiken;
            props["contract"] = (int) maid.status.contract;
            props["personal"] = maid.status.personal?.id ?? 0;
            props["current_job_class_id"] = maid.status.selectedJobClass?.data?.id ?? 0;
            props["current_yotogi_class_id"] = maid.status.selectedYotogiClass?.data?.id ?? 0;
            props["first_name_call"] = maid.status.isFirstNameCall;
            props["is_leader"] = maid.status.leader;
            props["active_noon_work_id"] = maid.status.noonWorkId;
            props["active_night_work_id"] = maid.status.nightWorkId;
            props["profile_comment"] = maid.status.profileComment;

            var workLevels = new Dictionary<int, object>();
            var workPlayCounts = new Dictionary<int, object>();

            foreach (var data in maid.status.workDatas.GetValueArray())
            {
                workLevels[data.id] = data.level;
                workPlayCounts[data.id] = data.playCount;
            }

            result["work_levels"] = workLevels;
            result["work_play_counts"] = workPlayCounts;

            var propLocks = new Dict();
            result["prop_locks"] = propLocks;

            foreach (var propLock in maidLockList[maid.status.guid])
                propLocks[propLock.Key] = propLock.Value;

            var bonusProps = new Dict();
            result["bonus_properties"] = bonusProps;

            foreach (FieldInfo fieldInfo in maid.status.bonusStatus.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                bonusProps[fieldInfo.Name] = fieldInfo.GetValue(maid.status.bonusStatus);

            var jobData = new Dict();
            result["job_class_data"] = jobData;

            foreach (var classData in maid.status.jobClass.datas.GetValueArray())
                jobData[classData.data.id.ToString()] = new Dict {["level"] = classData.level, ["cur_exp"] = classData.cur_exp};

            var yotogiData = new Dict();
            result["yotogi_class_data"] = yotogiData;

            foreach (var classData in maid.status.yotogiClass.datas.GetValueArray())
                yotogiData[classData.data.id.ToString()] = new Dict {["level"] = classData.level, ["cur_exp"] = classData.cur_exp};

            var yotogiSkills = new Dictionary<int, object>();
            result["yotogi_skill_data"] = yotogiSkills;

            foreach (YotogiSkillData yotogiSkill in maid.status.yotogiSkill.datas.GetValueArray())
                yotogiSkills[yotogiSkill.data.id] = new Dict
                {
                        ["level"] = yotogiSkill.level,
                        ["cur_exp"] = yotogiSkill.currentExp,
                        ["play_count"] = yotogiSkill.playCount
                };

            foreach (YotogiSkillData yotogiSkill in maid.status.yotogiSkill.oldDatas.GetValueArray())
                yotogiSkills[yotogiSkill.oldData.id] = new Dict
                {
                        ["level"] = yotogiSkill.level,
                        ["cur_exp"] = yotogiSkill.currentExp,
                        ["play_count"] = yotogiSkill.playCount
                };

            result["feature_ids"] = maid.status.features.GetValueArray().Select(f => f.id).ToArray();
            result["propensity_ids"] = maid.status.propensitys.GetValueArray().Select(f => f.id).ToArray();

            Texture2D thum = maid.GetThumIcon();
            result["maid_thumbnail"] = thum?.EncodeToPNG();

            result["guid"] = maid.status.guid;

            return result;
        }

        private void OnPropertyChange(object sender, MaidStatusChangeEventArgs args)
        {
            if (IsDeserializing)
                return;

            if (string.IsNullOrEmpty(args.Status.guid)
                || !args.Status.guid.Equals(selectedMaidGuid, StringComparison.CurrentCultureIgnoreCase))
                return;

            object value;
            if (args.HasValue)
            {
                value = args.Value;
            }
            else
            {
                MethodInfo getter = maidGetters[args.PropertyName];
                value = getter.Invoke(args.Status, new object[0]);
                if (getter.ReturnType.IsEnum)
                    value = (int) value;
            }

            Emit("maid_prop_changed", new Dict {["guid"] = args.Status.guid, ["property_name"] = args.PropertyName, ["value"] = value});
        }

        private void CheckPropertyShouldChange(object sender, MaidStatusSetEventArgs args)
        {
            if (IsDeserializing || args.Maid == null || !maidLockList.TryGetValue(args.Maid.status.guid, out var lockList))
                return;
            args.Block = !GloballyUnlocked && lockList[args.PropertyName];
        }

        private void OnPropFeatureChanged(object sender, PropFeatureChangeEventArgs args)
        {
            if (IsDeserializing)
                return;

            if (args.Maid == null || string.IsNullOrEmpty(args.Maid.status.guid)
                                  || !args.Maid.status.guid.Equals(selectedMaidGuid, StringComparison.CurrentCultureIgnoreCase))
                return;

            Emit($"{args.Type}_changed", new Dict {["guid"] = args.Maid.status.guid, ["id"] = args.ID, ["selected"] = args.Selected});
        }

        private void OnMaidStatusHooksOnWorkDataChanged(object sender, WorkDataChangeEventArgs args)
        {
            if (IsDeserializing)
                return;
            if (string.IsNullOrEmpty(args.Maid.status.guid)
                || !args.Maid.status.guid.Equals(selectedMaidGuid, StringComparison.CurrentCultureIgnoreCase))
                return;

            Emit("work_data_changed",
                 new Dict {["guid"] = args.Maid.status.guid, ["id"] = args.ID, ["level"] = args.Level, ["play_count"] = args.PlayCount});
        }

        private void OnYotogiSkillHooksOnSkillInfoChanged(object sender, YotogiSkillEventArgs args)
        {
            if (IsDeserializing)
                return;
            if (string.IsNullOrEmpty(args.Maid.status.guid)
                || !args.Maid.status.guid.Equals(selectedMaidGuid, StringComparison.CurrentCultureIgnoreCase))
                return;

            Emit($"yotogi_skill_{args.Event}", new Dict {["guid"] = args.Maid.status.guid, ["skill_id"] = args.SkillId});
        }

        private void OnOldMaidDeserialized(object sender, OldMaidDeserializedEventArgs e)
        {
            maidLockList[e.Maid.status.guid] = maidLockList[e.OldGuid];
            maidLockList.Remove(e.OldGuid);

            Emit("old_maid_deserialized", new Dict
            {
                    ["old_guid"] = e.OldGuid,
                    ["new_guid"] = e.Maid.status.guid,
                    ["firstName"] = e.Maid.status.firstName,
                    ["lastName"] = e.Maid.status.lastName,
                    ["thumbnail"] = e.Maid.GetThumIcon()?.EncodeToPNG()
            });
        }

        private void InitMaidStatus()
        {
            maidSetters = new Dictionary<string, MethodInfo>();
            maidGetters = new Dictionary<string, MethodInfo>();

            var props = typeof(Status).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo propertyInfo in props)
            {
                MethodInfo get = propertyInfo.GetGetMethod();
                MethodInfo set = propertyInfo.GetSetMethod();

                if (get != null && (get.ReturnType.IsPrimitive || get.ReturnType == typeof(string) || get.ReturnType.IsEnum))
                    maidGetters.Add(propertyInfo.Name, get);

                if (set != null)
                    maidSetters.Add(propertyInfo.Name, set);
            }

            MaidStatusHooks.PropertyChanged += OnPropertyChange;
            MaidStatusHooks.WorkDataChanged += OnMaidStatusHooksOnWorkDataChanged;
            MaidStatusHooks.PropFeatureChanged += OnPropFeatureChanged;
            MaidStatusHooks.OldMaidDeserialized += OnOldMaidDeserialized;
            MaidStatusHooks.ProprtyShouldChange += CheckPropertyShouldChange;
            YotogiSkillHooks.SkillInfoChanged += OnYotogiSkillHooksOnSkillInfoChanged;

            MaidStatusHooks.ThumbnailChanged += (sender, args) =>
            {
                if (IsDeserializing)
                    return;

                Emit("maid_thumbnail_changed",
                     new Dict {["guid"] = args.Maid.status.guid, ["thumb"] = args.Maid.GetThumIcon().EncodeToPNG()});
            };
        }
    }
}