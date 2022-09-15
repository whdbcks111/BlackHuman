using UnityEngine;
using System.Collections.Generic;

public static class YieldCache 
{
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new();
    private static Dictionary<float, WaitForSeconds> _waitForSecondsCache = new();
    private static Dictionary<float, WaitForSecondsRealtime> _waitForSecondsRealtimeCache = new();

    public static WaitForSeconds WaitForSeconds(float sec)
    {
        if(_waitForSecondsCache.ContainsKey(sec)) return _waitForSecondsCache[sec];
        else return (_waitForSecondsCache[sec] = new WaitForSeconds(sec));
    }

    public static WaitForSecondsRealtime WaitForSecondsRealtime(float sec)
    {
        if(_waitForSecondsCache.ContainsKey(sec)) return _waitForSecondsRealtimeCache[sec];
        else return (_waitForSecondsRealtimeCache[sec] = new WaitForSecondsRealtime(sec));
    }
}