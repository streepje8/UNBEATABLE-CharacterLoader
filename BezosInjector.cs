using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BepInEx.Logging;
using UnityEngine;

namespace Streep.UNBEATABLE.CharacterLoader;

public class BezosInjector
{
    private static HashSet<Type> TypesToInject { get; } = new();
    private static bool injected = false;
    public static async void InjectAsync(ManualLogSource logger)
    {
        try
        {
            logger.LogInfo("Bezos Injecting...");
            while (JeffBezosController.instance == null) await Task.Delay(500);
            var injectedObj = new GameObject("BezosInjected");
            injectedObj.transform.SetParent(JeffBezosController.instance.transform);
            foreach (var type in TypesToInject) injectedObj.AddComponent(type);
            injected = true;
            logger.LogInfo("Bezos Inject Successful!");
        }
        catch (Exception e) { logger.LogError(e); }
    }

    public static void QueueInjection<T>() where T : MonoBehaviour
    {
        if (injected) throw new Exception("Injection already finished!");
        TypesToInject.Add(typeof(T));
    }
}