using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace COM3D2.MaidFiddler.Common.Data
{
    [Serializable]
    public class NamedData
    {
        public int ID { get; set; }
        public string UniqueName { get; set; }
    }

    [Serializable]
    public class WorkData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int TaskType { get; set; }
    }

    [Serializable]
    public class GameInfo
    {
        public List<NamedData> Features { get; set; }
        public List<NamedData> Propensities { get; set; }
        public Dictionary<string, int> RelationValues { get; set; }
        public Dictionary<string, int> SeikeikenValues { get; set; }
        public Dictionary<string, int> ContractValues { get; set; }
        public List<NamedData> JobClasses { get; set; }
        public List<NamedData> YotogiClasses { get; set; }
        public List<NamedData> Personalities { get; set; }
        public List<NamedData> YotogiSkills { get; set; }
        public Dictionary<string, string> LockableMaidStats { get; set; }
        public List<string> BonusMaidStats { get; set; }
        public Dictionary<string, string> LockableClubStats { get; set; }
    }
}
