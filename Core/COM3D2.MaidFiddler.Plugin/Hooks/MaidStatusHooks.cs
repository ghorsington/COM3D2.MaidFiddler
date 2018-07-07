using System;

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

    public static class MaidStatusHooks
    {
        public static event EventHandler<MaidStatusChangeEventArgs> PropertyChanged;
        public static event EventHandler<MaidEventArgs> ThumbnailChanged;

        public static bool OnPropertySetPrefix(string propName, MaidStatus.Status status)
        {
            return false;
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

        }

        public static void OnFeatureRemove(MaidStatus.Status status, MaidStatus.Feature.Data data)
        {

        }

        public static void OnPropensityAdd(MaidStatus.Status status, MaidStatus.Propensity.Data data)
        {

        }

        public static void OnPropensityRemove(MaidStatus.Status status, MaidStatus.Propensity.Data data)
        {

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

        }

        public static void OnBonusStatusChange(MaidStatus.Status status)
        {

        }
    }
}
