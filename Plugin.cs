using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Streep.UNBEATABLE.CharacterLoader
{
    [BepInPlugin(UnbeatableCharacterModInfo.PLUGIN_GUID, UnbeatableCharacterModInfo.PLUGIN_NAME, UnbeatableCharacterModInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger;

        private void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("UNBEATABLE Character Loader");
            Logger.LogInfo($"Plugin {UnbeatableCharacterModInfo.PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(UnbeatableCharacterModInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
            Logger.LogInfo("Applied patches.");
            BezosInjector.QueueInjection<CharacterMod>();
            Logger.LogInfo("Start Bezos Injector...");
            BezosInjector.InjectAsync(Logger);
        }

        private void OnApplicationQuit()
        {
            BepInEx.Logging.Logger.Sources.Remove(Logger);
        }
    }
}