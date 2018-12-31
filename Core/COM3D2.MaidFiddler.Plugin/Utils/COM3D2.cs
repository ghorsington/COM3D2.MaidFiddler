using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using COM3D2.MaidFiddler.Common.Data;
using MaidStatus;
using Schedule;
using Yotogis;
using WorkData = COM3D2.MaidFiddler.Common.Data.WorkData;

namespace COM3D2.MaidFiddler.Core.Utils
{
    public static class COM3D2
    {
        public static readonly GameInfo GameInfo;

        static COM3D2()
        {
            GameInfo = new GameInfo
            {
                BonusMaidStats = GetMaidBonusStatusInfo(),
                ContractValues = GetEnumInfo<Contract>(),
                Features = GetFeatureInfo(),
                JobClasses = GetJobClassInfo(),
                LockableClubStats = GetLockableClubStatusInfo(),
                LockableMaidStats = GetLockableMaidStatusValueInfo(),
                Personalities = GetPersonalInfo(),
                Propensities = GetPropensityInfo(),
                RelationValues = GetEnumInfo<Relation>(),
                SeikeikenValues = GetEnumInfo<Seikeiken>(),
                YotogiClasses = GetYotogiClassInfo(),
                YotogiSkills = GetYotogiSkillInfo()
            };
        }

        public static List<NamedData> GetFeatureInfo()
        {
            return Feature.GetAllDatas(true).Select(d => new NamedData {ID = d.id, UniqueName = d.uniqueName}).ToList();
        }

        public static Dictionary<string, string> GetLockableClubStatusInfo()
        {
            return typeof(Status).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                 .Where(p => p.CanWrite && !p.PropertyType.IsEnum
                                                        && (p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
                                 .ToDictionary(p => p.Name,
                                               p => p.PropertyType.FullName);
        }

        public static List<WorkData> GetWorkInfo()
        {
            return ScheduleCSVData
                   .AllData.Select(d => new WorkData {ID = d.Value.id, Name = d.Value.name, TaskType = (int) d.Value.type})
                   .ToList();
        }

        public static List<string> GetMaidBonusStatusInfo()
        {
            return typeof(BonusStatus).GetFields(BindingFlags.Instance | BindingFlags.Public).Select(f => f.Name).ToList();
        }

        public static Dictionary<string, string> GetLockableMaidStatusValueInfo()
        {
            return typeof(Status).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                            .Where(p => p.GetSetMethod() != null && !p.PropertyType.IsEnum
                                                                                 && (p.PropertyType.IsValueType
                                                                                     || p.PropertyType == typeof(string)))
                                            .ToDictionary(p => p.Name,
                                                          p =>
                                                          {
                                                              Type t = p.PropertyType;
                                                              if (t.IsFloat())
                                                                  return "double";
                                                              if (t.IsInteger())
                                                                  return "int";
                                                              return "string";
                                                          });
        }

        public static List<NamedData> GetYotogiSkillInfo()
        {
            return (from dataList in Skill.skill_data_list from data in dataList select new NamedData {ID = data.Value.id, UniqueName = data.Value.name}).ToList();
        }

        public static List<NamedData> GetPersonalInfo()
        {
            var datas = new List<NamedData>();
            foreach (var data in Personal.GetAllDatas(true))
                datas.Add(new NamedData { ID = data.id, UniqueName = data.uniqueName });
            return datas;
        }

        public static List<NamedData> GetYotogiClassInfo()
        {
            var datas = new List<NamedData>();
            foreach (var data in YotogiClass.GetAllDatas(true))
                datas.Add(new NamedData { ID = data.id, UniqueName = data.uniqueName });
            return datas;
        }

        public static List<NamedData> GetJobClassInfo()
        {
            var datas = new List<NamedData>();
            foreach (var data in JobClass.GetAllDatas(true))
                datas.Add(new NamedData { ID = data.id, UniqueName = data.uniqueName });
            return datas;
        }

        public static List<NamedData> GetPropensityInfo()
        {
            var datas = new List<NamedData>();
            foreach (Propensity.Data data in Propensity.GetAllDatas(true))
                datas.Add(new NamedData { ID = data.id, UniqueName = data.uniqueName});
            return datas;
        }

        public static Dictionary<string, int> GetEnumInfo<T>()
        {
            var result = new Dictionary<string, int>();
            foreach (var value in Enum.GetValues(typeof(T)))
                result[value.ToString()] = (int)value;
            return result;
        }
    }
}
