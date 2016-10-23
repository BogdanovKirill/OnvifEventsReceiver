using System;

namespace Common.Util
{
    public static class TimeUtil
    {
        public static bool IsTimeOver(int previousTicks, int interval)
        {
            if (Math.Abs(Environment.TickCount - previousTicks) > interval)
                return true;

            return false;
        }
    }
}
