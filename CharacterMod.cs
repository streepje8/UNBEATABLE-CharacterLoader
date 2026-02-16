using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arcade.Unlockables;
using Cysharp.Threading.Tasks;
using Rhythm;
using UnityEngine;

namespace Streep.UNBEATABLE.CharacterLoader;

public class CharacterMod : MonoBehaviour
{
    public static CharacterMod Current;
    public Dictionary<string, UnbeatableCharacterFile> Characters = new Dictionary<string, UnbeatableCharacterFile>();

    private void Awake()
    {
        Current = this;
        AsyncDispatch(CharacterModAsync);
    }

    public static async void AsyncDispatch(Func<UniTask> func)
    {
        try
        {
            await func();
        }
        catch (Exception e)
        {
            await UniTask.SwitchToMainThread();
            Plugin.Logger.LogError(e);
        }
    }

    public async UniTask CharacterModAsync()
    {
        await LoadCharacters();
    }

    private async UniTask<bool> LoadCharacters()
    {
        Plugin.Logger.LogInfo("Loading characters...");
        var charactersFolder = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, ".."), "Characters"));
        Plugin.Logger.LogInfo($"Searching for characters in {charactersFolder}...");
        if (!Directory.Exists(charactersFolder))
        {
            Directory.CreateDirectory(charactersFolder);
            Plugin.Logger.LogError("No characters found!");
            return false;
        }

        var characterFiles = Directory.GetFiles(charactersFolder, "*.ubcharacter");
        if (characterFiles.Length < 1)
        {
            Plugin.Logger.LogError("No characters found!");
            return false;
        }

        foreach (var characterFile in characterFiles)
        {
            Plugin.Logger.LogInfo($"Loading character in file {characterFile}");
            try
            {
                var file = await UnbeatableCharacterFile.Load(characterFile);
                Characters.Add(file.CharacterInfo.Name, file);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to load character file, error: {ex}");
            }
        }

        return true;
    }

    public void Customize(RhythmCharacterSelector selector, string characterName)
    {
        if (!Characters.TryGetValue(characterName, out var character))
        {
            Plugin.Logger.LogInfo("Player does not have custom character selected. Customization skipped.");
            return;
        }

        var animator = selector.player.currentAnimator;

        Plugin.Logger.LogInfo($"Applying character customization for '{character.CharacterInfo.Name}'...");
        if (!character.Assets.TryGetValue(character.AnimatorName, out var controllerRaw) ||
            controllerRaw is not RuntimeAnimatorController controller)
        {
            Plugin.Logger.LogError("Character has invalid/missing animation controller!");
            return;
        }

        if (!character.Assets.TryGetValue(character.PrefabName, out var prefabRaw) ||
            prefabRaw is not GameObject prefab)
        {
            Plugin.Logger.LogError("Character has invalid/missing custom asset prefab!");
            return;
        }

        var customContent = Instantiate(prefab, animator.transform);
        customContent.gameObject.name = customContent.gameObject.name.Replace("(Clone)", string.Empty);
        var customContentManager = animator.GetComponent<CustomContentManager>();
        if (!customContentManager) customContentManager = animator.gameObject.AddComponent<CustomContentManager>();
        customContentManager.Initialize(customContent, animator);
        if (!customContent)
        {
            Plugin.Logger.LogError("Failed to load custom content on rhythm animator!");
            return;
        }

        var inGameAnimator = animator.GetComponent<Animator>();
        if (!inGameAnimator)
        {
            Plugin.Logger.LogError("Failed to locate the in game animator!");
            return;
        }

        if (character.CharacterInfo.OverwriteAllSpriteRendererMaterials)
        {
            var spriteRenderer = inGameAnimator.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                foreach (var childRenderer in customContent.GetComponentsInChildren<SpriteRenderer>())
                {
                    childRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
                }
            }
            else Plugin.Logger.LogWarning("Failed to override materials if requested!");
        }

        inGameAnimator.runtimeAnimatorController = controller;

        animator.Default = new RhythmPlayerAnimator.ActionStateAnim()
        {
            Intro = character.CharacterInfo.DefaultAnimationBindings.Intro,
            IntroDuration = character.CharacterInfo.DefaultAnimationBindings.IntroDuration,

            AirBlock = character.CharacterInfo.DefaultAnimationBindings.AirBlock,
            GroundBlock = character.CharacterInfo.DefaultAnimationBindings.GroundBlock,
            Idle = character.CharacterInfo.DefaultAnimationBindings.Idle,
            SideSwitch = character.CharacterInfo.DefaultAnimationBindings.SideSwitch,

            HighAttacks = character.CharacterInfo.DefaultAnimationBindings.HighAttacks,
            LowAttacks = character.CharacterInfo.DefaultAnimationBindings.LowAttacks,
            Jumps = character.CharacterInfo.DefaultAnimationBindings.Jumps,
            Lands = character.CharacterInfo.DefaultAnimationBindings.Lands,
            Hurts = character.CharacterInfo.DefaultAnimationBindings.Hurts,

            HasSlams = character.CharacterInfo.DefaultAnimationBindings.HasSlams,
            IgnoreSlams = character.CharacterInfo.DefaultAnimationBindings.IgnoreSlams,
            SlamAttacks = character.CharacterInfo.DefaultAnimationBindings.SlamAttacks
        };
        animator.Brawl = new RhythmPlayerAnimator.ActionStateAnim()
        {
            Intro = character.CharacterInfo.BrawlAnimationBindings.Intro,
            IntroDuration = character.CharacterInfo.BrawlAnimationBindings.IntroDuration,

            AirBlock = character.CharacterInfo.BrawlAnimationBindings.AirBlock,
            GroundBlock = character.CharacterInfo.BrawlAnimationBindings.GroundBlock,
            Idle = character.CharacterInfo.BrawlAnimationBindings.Idle,
            SideSwitch = character.CharacterInfo.BrawlAnimationBindings.SideSwitch,

            HighAttacks = character.CharacterInfo.BrawlAnimationBindings.HighAttacks,
            LowAttacks = character.CharacterInfo.BrawlAnimationBindings.LowAttacks,
            Jumps = character.CharacterInfo.BrawlAnimationBindings.Jumps,
            Lands = character.CharacterInfo.BrawlAnimationBindings.Lands,
            Hurts = character.CharacterInfo.BrawlAnimationBindings.Hurts,

            HasSlams = character.CharacterInfo.BrawlAnimationBindings.HasSlams,
            IgnoreSlams = character.CharacterInfo.BrawlAnimationBindings.IgnoreSlams,
            SlamAttacks = character.CharacterInfo.BrawlAnimationBindings.SlamAttacks
        };
        animator.Running = new RhythmPlayerAnimator.ActionStateAnim()
        {
            Intro = character.CharacterInfo.RunningAnimationBindings.Intro,
            IntroDuration = character.CharacterInfo.RunningAnimationBindings.IntroDuration,

            AirBlock = character.CharacterInfo.RunningAnimationBindings.AirBlock,
            GroundBlock = character.CharacterInfo.RunningAnimationBindings.GroundBlock,
            Idle = character.CharacterInfo.RunningAnimationBindings.Idle,
            SideSwitch = character.CharacterInfo.RunningAnimationBindings.SideSwitch,

            HighAttacks = character.CharacterInfo.RunningAnimationBindings.HighAttacks,
            LowAttacks = character.CharacterInfo.RunningAnimationBindings.LowAttacks,
            Jumps = character.CharacterInfo.RunningAnimationBindings.Jumps,
            Lands = character.CharacterInfo.RunningAnimationBindings.Lands,
            Hurts = character.CharacterInfo.RunningAnimationBindings.Hurts,

            HasSlams = character.CharacterInfo.RunningAnimationBindings.HasSlams,
            IgnoreSlams = character.CharacterInfo.RunningAnimationBindings.IgnoreSlams,
            SlamAttacks = character.CharacterInfo.RunningAnimationBindings.SlamAttacks
        };
        animator.Falling = new RhythmPlayerAnimator.ActionStateAnim()
        {
            Intro = character.CharacterInfo.FallingAnimationBindings.Intro,
            IntroDuration = character.CharacterInfo.FallingAnimationBindings.IntroDuration,

            AirBlock = character.CharacterInfo.FallingAnimationBindings.AirBlock,
            GroundBlock = character.CharacterInfo.FallingAnimationBindings.GroundBlock,
            Idle = character.CharacterInfo.FallingAnimationBindings.Idle,
            SideSwitch = character.CharacterInfo.FallingAnimationBindings.SideSwitch,

            HighAttacks = character.CharacterInfo.FallingAnimationBindings.HighAttacks,
            LowAttacks = character.CharacterInfo.FallingAnimationBindings.LowAttacks,
            Jumps = character.CharacterInfo.FallingAnimationBindings.Jumps,
            Lands = character.CharacterInfo.FallingAnimationBindings.Lands,
            Hurts = character.CharacterInfo.FallingAnimationBindings.Hurts,

            HasSlams = character.CharacterInfo.FallingAnimationBindings.HasSlams,
            IgnoreSlams = character.CharacterInfo.FallingAnimationBindings.IgnoreSlams,
            SlamAttacks = character.CharacterInfo.FallingAnimationBindings.SlamAttacks
        };

        animator.Awake();
        foreach (var controllerAnimationClip in controller.animationClips)
        {
            Plugin.Logger.LogInfo($"With animation {controllerAnimationClip.name}");
        }

        animator._sprite.enabled = false;
        Plugin.Logger.LogInfo($"Applied character {character.CharacterInfo.Name}!");
    }

    public void AppendCustomCharacters(ref List<CharacterIndex.Character> characters)
    {
        var beat = characters.FirstOrDefault(x => x.name.ToLowerInvariant().Contains("beat"));
        if (beat == null)
        {
            Plugin.Logger.LogError("Could not add custom characters. Beat == null?");
            return;
        }
        foreach (var (cname, _) in Characters)
        {
            characters.Add(new CharacterIndex.Character()
            {
                name = cname,
                prefab = beat.prefab,
                assistPrefab = beat.assistPrefab,
                bgPrefab = null
            });
        }
    }
}