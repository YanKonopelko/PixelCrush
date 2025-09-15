using System;
using UnityEngine;


public class TimeUtils
{
    public static long InGameLocalNow()
    {
        // Time.time
        long milliseconds = (long)(Time.time * 1000);
        return milliseconds;
    }

    public static long LocalNow()
    {
         long dateticks = DateTime.Now.Ticks;
        long datemilliseconds = dateticks / TimeSpan.TicksPerMillisecond;
        return datemilliseconds;
    }

}
