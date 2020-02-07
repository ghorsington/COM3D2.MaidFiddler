using System.Collections.Generic;
using MaidStatus;

namespace COM3D2.MaidFiddler.Core.Hooks
{
    public static class Fixes
    {
        public static bool GetFeelingFix(out Feeling returnVal, ref Dictionary<string, int> flags_)
        {
            returnVal = flags_.TryGetValue("__vr_feeling", out var result) ? (Feeling)result : Feeling.Normal;
            return true;
        }
    }
}
