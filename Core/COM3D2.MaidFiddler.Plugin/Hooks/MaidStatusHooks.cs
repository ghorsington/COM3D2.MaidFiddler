using System;
using MaidStatus;

namespace COM3D2.MaidFiddler.Core.Hooks
{
    public class MaidStatusChangeEventArgs : EventArgs
    {
        public MaidStatus.Status Status { get; internal set; }
        public string PropertyName { get; internal set; }
        public bool HasValue { get; internal set; } = false;
        public object Value { get; internal set; } = null;
    }

    public class MaidEventArgs : EventArgs
    {
        public Maid Maid { get; internal set; }
    }

    public class MaidStatusSetEventArgs : EventArgs
    {
        public Maid Maid { get; internal set; }
        public string PropertyName { get; internal set; }
        public bool Block { get; set; } = false;
    }

    public class PropFeatureChangeEventArgs : EventArgs
    {
        public Maid Maid { get; internal set; }
        public string Type { get; internal set; }
        public bool Selected { get; internal set; }
        public int ID { get; internal set; }
    }

    public class WorkDataChangeEventArgs : EventArgs
    {
        public Maid Maid { get; internal set; }
        public int ID { get; internal set; }
        public int Level { get; internal set; }
        public uint PlayCount { get; internal set; }
    }

    public static class MaidStatusHooks
    {
        public static event EventHandler<MaidStatusSetEventArgs> ProprtyShouldChange;
        public static event EventHandler<MaidStatusChangeEventArgs> PropertyChanged;
        public static event EventHandler<MaidEventArgs> ThumbnailChanged;
        public static event EventHandler<PropFeatureChangeEventArgs> PropFeatureChanged;
        public static event EventHandler<WorkDataChangeEventArgs> WorkDataChanged;

        public static bool OnPropertySetPrefix(string propName, MaidStatus.Status status)
        {
            MaidStatusSetEventArgs args = new MaidStatusSetEventArgs
            {
                    Maid = status.maid,
                    PropertyName = propName
            };

            ProprtyShouldChange?.Invoke(null, args);

            return args.Block;
        }

        public static void OnPropertySetPostfix(string propName, MaidStatus.Status status)
        {
            MaidStatusChangeEventArgs args = new MaidStatusChangeEventArgs
            {
               Status = status,
               PropertyName = propName
            };

            PropertyChanged?.Invoke(null, args);
        }

        public static void OnThumShot(Maid maid)
        {
            ThumbnailChanged?.Invoke(null, new MaidEventArgs
            {
                Maid = maid
            });
        }

        public static void OnFeatureAdd(MaidStatus.Status status, MaidStatus.Feature.Data data)
        {
            PropFeatureChanged?.Invoke(null, new PropFeatureChangeEventArgs
            {
                    Selected = true,
                    ID = data.id,
                    Maid = status.maid,
                    Type = "feature"
            });
        }

        public static void OnFeatureRemove(MaidStatus.Status status, MaidStatus.Feature.Data data)
        {
            PropFeatureChanged?.Invoke(null, new PropFeatureChangeEventArgs
            {
                    Selected = false,
                    ID = data.id,
                    Maid = status.maid,
                    Type = "feature"
            });
        }

        public static void OnPropensityAdd(MaidStatus.Status status, MaidStatus.Propensity.Data data)
        {
            PropFeatureChanged?.Invoke(null, new PropFeatureChangeEventArgs
            {
                    Selected = true,
                    ID = data.id,
                    Maid = status.maid,
                    Type = "propensity"
            });
        }

        public static void OnPropensityRemove(MaidStatus.Status status, MaidStatus.Propensity.Data data)
        {
            PropFeatureChanged?.Invoke(null, new PropFeatureChangeEventArgs
            {
                    Selected = false,
                    ID = data.id,
                    Maid = status.maid,
                    Type = "propensity"
            });
        }

        public static void OnJobClassChange(MaidStatus.Status status, MaidStatus.JobClass.Data data)
        {
            PropertyChanged?.Invoke(null, new MaidStatusChangeEventArgs
            {
                    Status = status,
                    PropertyName = "current_job_class_id",
                    HasValue = true,
                    Value = status.selectedJobClass.data.id
            });
        }

        public static void OnYotogiClassChange(MaidStatus.Status status, MaidStatus.YotogiClass.Data data)
        {
            PropertyChanged?.Invoke(null, new MaidStatusChangeEventArgs
            {
                    Status = status,
                    PropertyName = "current_yotogi_class_id",
                    HasValue = true,
                    Value = status.selectedYotogiClass.data.id
            });
        }

        public static void OnPersonalSet(MaidStatus.Status status, MaidStatus.Personal.Data data)
        {
            PropertyChanged?.Invoke(null, new MaidStatusChangeEventArgs
            {
                    Status = status,
                    PropertyName = "personal",
                    HasValue = true,
                    Value = status.personal.id
            });
        }

        public static void OnSetSeikeiken(string type, MaidStatus.Status status, bool value)
        {
            PropertyChanged?.Invoke(null, new MaidStatusChangeEventArgs
            {
                    Status = status,
                    PropertyName = "cur_seikeiken",
                    HasValue = true,
                    Value = (int) status.seikeiken
            });
        }

        public static void OnWorkDataUpdate(MaidStatus.Status status, int id)
        {
            if (status.maid == null)
                return;

            WorkData data = status.workDatas[id];

            WorkDataChanged?.Invoke(null, new WorkDataChangeEventArgs
            {
                    ID = id,
                    Maid = status.maid,
                    Level = data.level,
                    PlayCount = data.playCount
            });
        }

        public static void OnBonusStatusChange(MaidStatus.Status status)
        {

        }
    }
}
