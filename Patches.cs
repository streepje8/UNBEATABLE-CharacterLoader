using System.Collections.Generic;
using Arcade.Unlockables;
using HarmonyLib;
using JetBrains.Annotations;

namespace Streep.UNBEATABLE.CharacterLoader
{
    public static class Patches
    {
        [UsedImplicitly]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RhythmCharacterSelector), "InstantiatePlayer")]
        static void PatchAwake(RhythmCharacterSelector __instance, string character)
        {
            if (CharacterMod.Current) CharacterMod.Current.Customize(__instance, character);
        }

        [UsedImplicitly]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterIndex), "AddCharacters")]
        static void PatchCharacterIndex(ref List<CharacterIndex.Character> newCharacters)
        {
            if (CharacterMod.Current) CharacterMod.Current.AppendCustomCharacters(ref newCharacters);
            else Plugin.Logger.LogError("Tried to retrieve custom characters before they were loaded.");
        }
    }
}