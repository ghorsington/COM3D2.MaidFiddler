using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.MaidFiddler.Hooks
{
    public class MaidStatusChangeEventArgs : EventArgs
    {
        public MaidStatus.Status Status { get; internal set; }
        public string PropertyName { get; internal set; }
    }

    public static class MaidStatusHooks
    {
        public static event EventHandler<MaidStatusChangeEventArgs> PropertyChanged;

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

        }

        public static void OnYotogiClassChange(MaidStatus.Status status, MaidStatus.YotogiClass.Data data)
        {

        }

        public static void OnPersonalSet(MaidStatus.Status status, MaidStatus.Personal.Data data)
        {

        }

        public static void OnSetSeikeiken(string type, MaidStatus.Status status, bool value)
        {

        }

        public static void OnWorkDataUpdate(MaidStatus.Status status, int id)
        {

        }

        public static void OnBonusStatusChange(MaidStatus.Status status)
        {

        }
    }
}
