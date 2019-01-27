using System.Collections.Generic;
using MessagePack;

namespace COM3D2.MaidFiddler.Common.Data
{
    [MessagePackObject]
    public class NamedData
    {
        [Key(0)]
        public int ID { get; set; }

        [Key(1)]
        public string UniqueName { get; set; }
    }

    [MessagePackObject]
    public class WorkData
    {
        [Key(0)]
        public int ID { get; set; }

        [Key(1)]
        public string Name { get; set; }

        [Key(2)]
        public int TaskType { get; set; }
    }

    [MessagePackObject]
    public class GameInfo
    {
        [Key(0)]
        public List<NamedData> Features { get; set; }

        [Key(1)]
        public List<NamedData> Propensities { get; set; }

        [Key(2)]
        public Dictionary<string, int> RelationValues { get; set; }

        [Key(3)]
        public Dictionary<string, int> SeikeikenValues { get; set; }

        [Key(4)]
        public Dictionary<string, int> ContractValues { get; set; }

        [Key(5)]
        public List<NamedData> JobClasses { get; set; }

        [Key(6)]
        public List<NamedData> YotogiClasses { get; set; }

        [Key(7)]
        public List<NamedData> Personalities { get; set; }

        [Key(8)]
        public List<NamedData> YotogiSkills { get; set; }

        [Key(9)]
        public Dictionary<string, string> LockableMaidStats { get; set; }

        [Key(10)]
        public List<string> BonusMaidStats { get; set; }

        [Key(11)]
        public Dictionary<string, string> LockableClubStats { get; set; }
    }
}
