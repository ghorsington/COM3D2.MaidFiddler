using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using COM3D2.MaidFiddler.Core.Hooks;
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
            return GameMain.Instance.CharacterMgr.GetStockMaidList()
                           .ToDictionary(m => m.status.guid, m => m.status.fullNameEnStyle);
        }

        public string[] GetMaidParameterList() => maidSetters.Keys.ToArray();

        public object GetMaidProperty(string maidId, string propertyName)
        {
            if (!maidGetters.TryGetValue(propertyName, out MethodInfo getter))
                throw new ArgumentException($"No such property: {propertyName}", nameof(propertyName));

            string id = maidId.ToLower(CultureInfo.InvariantCulture);

            Maid[] maids = GameMain.Instance.CharacterMgr.GetStockMaidList()
                                   .Where(m => m.status.guid.ToLower(CultureInfo.InvariantCulture).StartsWith(id))
                                   .ToArray();

            if (maids.Length == 0)
                throw new ArgumentException($"No such maid with ID: {maidId}", nameof(maidId));
            if (maids.Length > 1)
                throw new
                        ArgumentException($"Found multiple maids whose ID starts the same:\n\n{string.Join("\n", maids.Select(m => $"{m.status.fullNameEnStyle}; ID: {m.status.guid}").ToArray())}\nPlease give a more specific ID!");

            return getter.Invoke(maids[0].status, new object[0]);
        }

        public void SetMaidProperty(string maidId, string propertyName, object value)
        {
            if (!maidSetters.TryGetValue(propertyName, out MethodInfo setter))
                throw new ArgumentException($"No such property: {propertyName}", nameof(propertyName));
            Type paramType = setter.GetParameters()[0].ParameterType;

            object val;
            try
            {
                val = Convert.ChangeType(value, paramType);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Cannot convert value to {paramType.FullName}.", e);
            }

            string id = maidId.ToLower(CultureInfo.InvariantCulture);

            Maid[] maids = GameMain.Instance.CharacterMgr.GetStockMaidList()
                                   .Where(m => m.status.guid.ToLower(CultureInfo.InvariantCulture).StartsWith(id))
                                   .ToArray();

            if (maids.Length == 0)
                throw new ArgumentException($"No such maid with ID: {maidId}", nameof(maidId));
            if (maids.Length > 1)
                throw new
                        ArgumentException($"Found multiple maids whose ID starts the same:\n\n{string.Join("\n", maids.Select(m => $"{m.status.fullNameEnStyle}; ID: {m.status.guid}").ToArray())}\nPlease give a more specific ID!");

            setter.Invoke(maids[0].status, new[] {val});
        }

        public Dict GetMaidData(string maidId)
        {
            string id = maidId.ToLower(CultureInfo.InvariantCulture);

            Maid[] maids = GameMain.Instance.CharacterMgr.GetStockMaidList()
                                   .Where(m => m.status.guid.ToLower(CultureInfo.InvariantCulture).StartsWith(id))
                                   .ToArray();

            if (maids.Length == 0)
                throw new ArgumentException($"No such maid with ID: {maidId}", nameof(maidId));
            if (maids.Length > 1)
                throw new
                        ArgumentException($"Found multiple maids whose ID starts the same:\n\n{string.Join("\n", maids.Select(m => $"{m.status.fullNameEnStyle}; ID: {m.status.guid}").ToArray())}\nPlease give a more specific ID!");

            return ReadMaidData(maids[0]);
        }

        private Dict ReadMaidData(Maid maid)
        {
            Dict result = new Dict();

            Dict props = new Dict();
            result["set_properties"] = props;

            Dict enumProps = new Dict();
            result["enum_props"] = enumProps;

            foreach (KeyValuePair<string, MethodInfo> getter in maidGetters)
                if (maidSetters.ContainsKey(getter.Key))
                    if(getter.Value.ReturnType.IsEnum)
                        enumProps[getter.Key] = (int) getter.Value.Invoke(maid.status, new object[0]);
                    else
                        props[getter.Key] = getter.Value.Invoke(maid.status, new object[0]);

            Dict basicInfo = new Dict();
            result["basic_properties"] = basicInfo;

            basicInfo["cur_seikeiken"] = (int) maid.status.seikeiken;
            basicInfo["init_seikeiken"] = (int) maid.status.initSeikeiken;
            basicInfo["contract"] = (int) maid.status.contract;
            basicInfo["personal"] = maid.status.personal.id;
            basicInfo["current_job_class_id"] = maid.status.selectedJobClass.data.id;
            basicInfo["current_yotogi_class_id"] = maid.status.selectedYotogiClass.data.id;
            basicInfo["first_name_call"] = maid.status.isFirstNameCall;
            basicInfo["is_leader"] = maid.status.leader;
            basicInfo["active_noon_work_id"] = maid.status.noonWorkId;
            basicInfo["active_night_work_id"] = maid.status.nightWorkId;
            basicInfo["profile_comment"] = maid.status.profileComment;
            
            Dict bonusProps = new Dict();
            result["bonus_properties"] = bonusProps;

            foreach (FieldInfo fieldInfo in maid.status.bonusStatus.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                bonusProps[fieldInfo.Name] = fieldInfo.GetValue(maid.status.bonusStatus);

            Dict jobData = new Dict();
            result["job_class_data"] = jobData;

            foreach (int id in maid.status.jobClass.datas.GetKeyArray())
            {
                ClassData<JobClass.Data> classData = maid.status.jobClass.datas[id];
                jobData[classData.data.id.ToString()] = new Dict
                {
                        ["level"] = classData.level,
                        ["cur_exp"] = classData.cur_exp
                };
            }

            Dict yotogiData = new Dict();
            result["yotogi_class_data"] = yotogiData;

            foreach (int id in maid.status.yotogiClass.datas.GetKeyArray())
            {
                ClassData<YotogiClass.Data> classData = maid.status.yotogiClass.datas[id];
                yotogiData[classData.data.id.ToString()] = new Dict
                {
                        ["level"] = classData.level,
                        ["cur_exp"] = classData.cur_exp
                };
            }

            Dict yotogiSkills = new Dict();
            result["yotogi_skill_data"] = yotogiSkills;

            foreach (int id in maid.status.yotogiSkill.datas.GetKeyArray())
            {
                YotogiSkillData yotogiSkill = maid.status.yotogiSkill.datas[id];
                yotogiSkills[yotogiSkill.data.id.ToString()] = new Dict
                {
                        ["level"] = yotogiSkill.level,
                        ["cur_exp"] = yotogiSkill.currentExp,
                        ["play_count"] = yotogiSkill.playCount
                };
            }

            result["feature_ids"] = maid.status.features.GetValueArray().Select(f => f.id).ToArray();
            result["propensity_ids"] = maid.status.propensitys.GetValueArray().Select(f => f.id).ToArray();

            Texture2D thum = maid.GetThumIcon();
            result["maid_thumbnail"] = thum != null ? thum.EncodeToPNG() : null;

            result["guid"] = maid.status.guid;

            return result;
        }

        private void OnPropertyChange(object sender, MaidStatusChangeEventArgs args)
        {
            if (IsDeserializing)
                return;

            if (args.Status.guid != selectedMaidGuid)
                return;

            object value = maidGetters[args.PropertyName].Invoke(args.Status, new object[0]);

            Emit("maid_prop_changed",
                 new Dict
                 {
                         ["guid"] = args.Status.guid,
                         ["property_name"] = args.PropertyName,
                         ["value"] = value
                 });
        }

        private void InitMaidStatus()
        {
            maidSetters = new Dictionary<string, MethodInfo>();
            maidGetters = new Dictionary<string, MethodInfo>();

            PropertyInfo[] props = typeof(Status).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo propertyInfo in props)
            {
                MethodInfo get = propertyInfo.GetGetMethod();
                MethodInfo set = propertyInfo.GetSetMethod();

                if (get != null
                    && (get.ReturnType.IsPrimitive || get.ReturnType == typeof(string) || get.ReturnType.IsEnum))
                    maidGetters.Add(propertyInfo.Name, get);

                if (set != null)
                    maidSetters.Add(propertyInfo.Name, set);
            }

            MaidStatusHooks.PropertyChanged += OnPropertyChange;

            MaidStatusHooks.ThumbnailChanged += (sender, args) =>
            {
                if (IsDeserializing)
                    return;

                Emit("maid_thumbnail_changed",
                     new Dict
                     {
                             ["guid"] = args.Maid.status.guid,
                             ["thumb"] = args.Maid.GetThumIcon().EncodeToPNG()
                     });
            };
        }
    }
}