using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COM3D2.MaidFiddler.Core.Hooks;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        public void SetUnlockRanges(bool value)
        {
            MathUtilHooks.UnlockRange = value;
        }
    }
}
