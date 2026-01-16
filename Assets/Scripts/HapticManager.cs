using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public static class HapticManager
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _TriggerLightHaptic();
    
    [DllImport("__Internal")]
    private static extern void _TriggerMediumHaptic();
    
    [DllImport("__Internal")]
    private static extern void _TriggerHeavyHaptic();
#endif

    public static void Light()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _TriggerLightHaptic();
#endif
    }
    
    public static void Medium()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _TriggerMediumHaptic();
#endif
    }
    
    public static void Heavy()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _TriggerHeavyHaptic();
#endif
    }

    public static IEnumerator PlayHapticsInARowRoutine(HapticType type, int count)
    {
#if UNITY_IOS && !UNITY_EDITOR
        for (var i = 0; i < count; i++)
        {
            switch (type)
            {
                case HapticType.Light:
                    Light();
                    yield return new WaitForSeconds(0.1f); // Задержка между haptics
                    break;
                case HapticType.Medium:
                    Medium();
                    yield return new WaitForSeconds(0.15f);
                    break;
                case HapticType.Heavy:
                    Heavy();
                    yield return new WaitForSeconds(0.2f);
                    break;
            }
        }
#endif
        yield break;
    }

    public enum HapticType
    {
        Light,
        Medium,
        Heavy
    }
}