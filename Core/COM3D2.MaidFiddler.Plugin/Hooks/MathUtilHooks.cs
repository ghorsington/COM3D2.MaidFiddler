namespace COM3D2.MaidFiddler.Core.Hooks
{
    public static class MathUtilHooks
    {
        public static bool UnlockRange { get; set; } = false;

        public static bool OnRound(out int result, int val)
        {
            bool ret = OnRound(out long res, val);
            result = (int) res;
            return ret;
        }

        public static bool OnRound(out long result, long val)
        {
            result = val;
            return UnlockRange;
        }

        public static bool OnRoundMinMax(out int res, int num, int min, int max)
        {
            bool ret = OnRoundMinMax(out long val, num, min, max);
            res = (int) val;
            return ret;
        }

        public static bool OnRoundMinMax(out long res, long num, long min, long max)
        {
            res = num;
            return UnlockRange;
        }

        public static bool OnConvertString(out string result, string str, int maxStrSize)
        {
            result = str;
            return UnlockRange;
        }
    }
}