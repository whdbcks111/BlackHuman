using UnityEngine;
using System.Collections.Generic;

public static class YieldCache 
{
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new();
    private static Dictionary<float, WaitForSeconds> _waitForSecondsCache = new();

    public static WaitForSeconds WaitForSeconds(float sec)
    {
        if(_waitForSecondsCache.ContainsKey(sec)) return _waitForSecondsCache[sec];
        else return (_waitForSecondsCache[sec] = new WaitForSeconds(sec));
    }
}