using System;
using UnityEngine;


namespace EGS.Utils 
{
    public static class Mathematics
    {
        public static float Remap(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            value = Math.Min(Math.Max(value, fromLow), fromHigh);
            return (value - fromLow) / (fromHigh - fromLow) * (toHigh - toLow) + toLow;
        }
    }
}