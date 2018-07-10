using System;
using System.Collections.Generic;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private Dictionary<string, bool> stockMaids; // TODO: Add lock list

        private string selectedMaidGuid = null;
        private Maid selectedMaid = null;

        internal void InitMaidList()
        {
            stockMaids = new Dictionary<string, bool>();
            foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                stockMaids[maid.status.guid] = true;
            }
        }

        internal void AddMaid(Maid maid)
        {
            if(stockMaids != null)
                stockMaids[maid.status.guid] = true;
        }

        internal void RemoveMaid(Maid maid)
        {
            stockMaids?.Remove(maid.status.guid);
        }

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
    }
}
