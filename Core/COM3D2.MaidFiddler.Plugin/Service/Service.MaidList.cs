using System.Collections.Generic;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private readonly Dictionary<string, Dictionary<string, bool>> maidLockList = new Dictionary<string, Dictionary<string, bool>>();
        private Maid selectedMaid;

        private string selectedMaidGuid;

        public Dictionary<string, object> SelectActiveMaid(string guid)
        {
            selectedMaidGuid = guid;

            if (guid == null)
            {
                selectedMaid = null;
                return null;
            }

            selectedMaid = GetMaid(guid);
            return ReadMaidData(selectedMaid);
        }

        public void SetMaidPropertyActive(string property, object value)
        {
            if (selectedMaid == null)
                return;

            SetMaidProperty(selectedMaid, property, value);
        }

        public void SetPersonalActive(int personalId)
        {
            if (selectedMaid == null)
                return;

            SetPersonal(selectedMaid, personalId);
        }

        public void SetCurrentJobClassActive(object classId)
        {
            if (selectedMaid == null)
                return;

            SetCurrentJobClass(selectedMaid, classId);
        }

        public void SetCurrentYotogiClassActive(object classId)
        {
            if (selectedMaid == null)
                return;

            SetCurrentYotogiClass(selectedMaid, classId);
        }

        public void SetContractActive(int contract)
        {
            if (selectedMaid == null)
                return;

            SetContract(selectedMaid, contract);
        }

        public void SetCurSeikeikenActive(int seikeiken)
        {
            if (selectedMaid == null)
                return;

            SetCurSeikeiken(selectedMaid, seikeiken);
        }

        public void SetInitSeikeikenActive(int seikeiken)
        {
            if (selectedMaid == null)
                return;

            SetInitSeikeiken(selectedMaid, seikeiken);
        }

        public bool ToggleActiveMaidLock(string propertyName, bool value)
        {
            if (selectedMaid == null)
                return false;

            return TogglePropertyLock(selectedMaidGuid, propertyName, value);
        }

        public void ToggleActiveMaidPropensity(object propensityId, bool toggle)
        {
            if (selectedMaid == null)
                return;

            TogglePropensity(selectedMaid, propensityId, toggle);
        }

        public void ToggleActiveMaidFeature(object propensityId, bool toggle)
        {
            if (selectedMaid == null)
                return;

            ToggleFeature(selectedMaid, propensityId, toggle);
        }

        public void SetWorkLevelActiveMaid(object id, object level)
        {
            if (selectedMaid == null)
                return;

            SetWorkDataLevel(selectedMaid, id, level);
        }

        public void SetWorkPlayCountActive(object id, object playCount)
        {
            if (selectedMaid == null)
                return;

            SetWorkPlayCount(selectedMaid, id, playCount);
        }

        public void SetNoonWorkActive(object id)
        {
            if (selectedMaid == null)
                return;

            SetNoonWork(selectedMaid, id);
        }

        public void SetNightWorkActive(object id)
        {
            if (selectedMaid == null)
                return;

            SetNightWork(selectedMaid, id);
        }

        public void ToggleYotogiSkillActive(object id, bool state)
        {
            if (selectedMaid == null)
                return;

            ToggleYotogiSkill(selectedMaid, id, state);
        }

        public void SetYotogiSkillLevelActive(object id, object level)
        {
            if (selectedMaid == null)
                return;

            SetYotogiSkillLevel(selectedMaid, id, level);
        }

        public void SetYotogiSkillExpActive(object id, object exp)
        {
            if (selectedMaid == null)
                return;

            SetYotogiSkillExp(selectedMaid, id, exp);
        }

        public void SetYotogiSkillPlayCountActive(object id, object playCount)
        {
            if (selectedMaid == null)
                return;

            SetYotogiSkillPlayCount(selectedMaid, id, playCount);
        }

        internal void InitMaidList()
        {
            maidLockList.Clear();
            foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                var dict = new Dictionary<string, bool>();
                maidLockList[maid.status.guid] = dict;

                foreach (string setter in maidSetters.Keys)
                    dict[setter] = false;
            }
        }

        internal void AddMaid(Maid maid)
        {
            if (maidLockList == null)
                return;
            var dict = new Dictionary<string, bool>();
            maidLockList[maid.status.guid] = dict;

            foreach (string setter in maidSetters.Keys)
                dict[setter] = false;
        }

        internal void RemoveMaid(Maid maid)
        {
            maidLockList?.Remove(maid.status.guid);
        }
    }
}