using System;

namespace arriety.utils
{
    public static class Utils
    {
        public static long GetCurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }

}