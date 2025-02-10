using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class Vibrator
{
    [DllImport("__Internal")]
    private static extern void vibrate(int ms);



    public static void Vibrate(int timeInMs)
    {
        
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
