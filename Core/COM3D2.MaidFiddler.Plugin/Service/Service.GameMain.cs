using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using COM3D2.MaidFiddler.Core.Hooks;
using COM3D2.MaidFiddler.Core.Utils;
using MaidStatus;
using Schedule;
using Yotogis;
using Dict = System.Collections.Generic.Dictionary<string, object>;
using List = System.Collections.Generic.List<object>;
using Status = PlayerStatus.Status;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private bool IsDeserializing { get; set; }

        public Dict GetGameInfo()
        {
            var result = new Dict
            {
                    ["feature_list"] = GetFeatureInfo(),
                    ["propensity_list"] = GetPropensityInfo(),
                    ["relation"] = GetEnumInfo<Relation>(),
                    ["seikeiken"] = GetEnumInfo<Seikeiken>(),
                    ["contract"] = GetEnumInfo<Contract>(),
                    ["job_class_list"] = GetJobClassInfo(),
                    ["yotogi_class_list"] = GetYotogiClassInfo(),
                    ["personal_list"] = GetPersonalInfo(),
                    ["yotogi_skills"] = GetYotogiSkillInfo(),
                    ["maid_status_settable"] = GetLockableMaidStatusValueInfo(),
                    ["maid_bonus_status"] = GetMaidBonusStatusInfo(),
                    ["club_status_settable"] = GetLockableClubStatusInfo(),
                    ["work_data"] = GetWorkInfo()
            };

            return result;
        }

        private void InitGameMain()
        {
            GameMainHooks.DeserializeStarting += (sender, args) =>
            {
                IsDeserializing = true;
                SelectActiveMaid(null);
                maidLockList.Clear();
                Debugger.WriteLine(LogLevel.Info, "Deserialize start!");
                Emit("deserialize_start", new Dict());
            };

            GameMainHooks.DeserializeEnded += (sender, args) =>
            {
                Debugger.WriteLine(LogLevel.Info, "Deserialize end!");
                IsDeserializing = false;
                Emit("deserialize_done", new Dict {["success"] = args.Success});
                if (args.Success)
                    InitMaidList();
            };
        }

        private List GetLockableClubStatusInfo()
        {
            return typeof(Status).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                 .Where(p => p.CanWrite && p.PropertyType != typeof(bool)
                                                        && (p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
                                 .Select(p => p.Name).Cast<object>().ToList();
        }

        private List GetWorkInfo()
        {
            return ScheduleCSVData
                   .AllData.Select(d => new Dict {["id"] = d.Value.id, ["name"] = d.Value.name, ["work_type"] = d.Value.type})
                   .Cast<object>().ToList();
        }

        private List GetMaidBonusStatusInfo()
        {
            return typeof(BonusStatus).GetFields(BindingFlags.Instance | BindingFlags.Public).Select(f => f.Name).Cast<object>().ToList();
        }

        private Dict GetLockableMaidStatusValueInfo()
        {
            return typeof(MaidStatus.Status).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                            .Where(p => p.GetSetMethod() != null && !p.PropertyType.IsEnum
                                                                                 && (p.PropertyType.IsValueType
                                                                                     || p.PropertyType == typeof(string)))
                                            .ToDictionary(p => p.Name,
                                                          p =>
                                                          {
                                                              Type t = p.PropertyType;
                                                              if (t.IsFloat())
                                                                  return "double" as object;
                                                              if (t.IsInteger())
                                                                  return "int" as object;
                                                              return "string" as object;
                                                          });
        }

        private List GetYotogiSkillInfo()
        {
            var datas = new List();

            foreach (SortedDictionary<int, Skill.Data> dataList in Skill.skill_data_list)
            foreach (KeyValuePair<int, Skill.Data> data in dataList)
                datas.Add(new Dict {["id"] = data.Value.id, ["name"] = data.Value.name});

            return datas;
        }

        private List GetPersonalInfo()
        {
            var datas = new List();

            foreach (Personal.Data data in Personal.GetAllDatas(true))
                datas.Add(new Dict {["id"] = data.id, ["name"] = data.uniqueName});

            return datas;
        }

        private List GetYotogiClassInfo()
        {
            var datas = new List();

            foreach (YotogiClass.Data data in YotogiClass.GetAllDatas(true))
                datas.Add(new Dict {["id"] = data.id, ["name"] = data.uniqueName});

            return datas;
        }

        private List GetJobClassInfo()
        {
            var datas = new List();

            foreach (JobClass.Data data in JobClass.GetAllDatas(true))
                datas.Add(new Dict {["id"] = data.id, ["name"] = data.uniqueName});

            return datas;
        }

        private List GetPropensityInfo()
        {
            var datas = new List();

            foreach (Propensity.Data data in Propensity.GetAllDatas(true))
                datas.Add(new Dict {["id"] = data.id, ["name"] = data.uniqueName});

            return datas;
        }

        private List GetFeatureInfo()
        {
            var datas = new List();

            foreach (Feature.Data data in Feature.GetAllDatas(true))
                datas.Add(new Dict {["id"] = data.id, ["name"] = data.uniqueName});

            return datas;
        }

        private static Dict GetEnumInfo<T>()
        {
            var result = new Dict();
            foreach (object value in Enum.GetValues(typeof(T)))
                result[value.ToString()] = (int) value;
            return result;
        }
    }
}