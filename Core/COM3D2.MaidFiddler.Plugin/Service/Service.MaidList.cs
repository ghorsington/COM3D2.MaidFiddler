using System;
using System.Collections.Generic;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private Dictionary<string, bool> stockMaids; // TODO: Add lock list

        private string selectedMaidGuid = null;

        internal void InitMaidList()
        {
            stockMaids = new Dictionary<string, bool>();
            foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                stockMaids[maid.status.guid] = true;
            }
            Console.WriteLine($"Inited maid list! I have {stockMaids.Count} maids!");
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

            return guid != null ? ReadMaidData(GameMain.Instance.CharacterMgr.GetStockMaid(guid)) : null;
        }
    }
}
