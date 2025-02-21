using System;
using System.Runtime.InteropServices;
using UnityEngine;
using YG;

public static class Vibrator
{
    [DllImport("__Internal")]
    private static extern void vibrate(int ms);



    public static void Vibrate(int timeInMs)
    {
        if(!PlayerData.Instance.VibrationEnable || YG2.envir.device != YG2.Device.Mobile) return;
    #if !UNITY_EDITOR && UNITY_WEBGL

    try
    {
        vibrate(timeInMs);
    }
    catch(Exception e){
        Debug.Log(e);
    }
    #else
            Debug.Log($"Simulate vibration {timeInMs}");
    #endif
        }
}
