using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Streep.UNBEATABLE.CharacterLoader;

public class UnbeatableCharacterFile : IDisposable
{
    public UnbeatableCharacterInfo CharacterInfo { get; }
    public string PrefabName { get; }
    public string AnimatorName { get; }
    public Dictionary<string, Object> Assets { get; }

    private UnbeatableCharacterFile(UnbeatableCharacterInfo characterInfo, string prefabName, string animatorName, Dictionary<string, Object> assets)
    {
        CharacterInfo = characterInfo;
        PrefabName = prefabName;
        AnimatorName = animatorName;
        Assets = assets;
    }

    private static readonly string Version = "V1.0.0";
    private static readonly string Magic = "UNBEATABLE-CHARACTER-FILE";
    public static void Create(string filePath, string characterInfo, string prefabName, string animatorName, byte[] assetBundleData)
    {
        using var stream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite);
        using var writer = new BinaryWriter(stream);
        writer.Write(Magic);
        writer.Write(Version);
        writer.Write(characterInfo);
        writer.Write(prefabName);
        writer.Write(animatorName);
        writer.Write(assetBundleData.Length);
        writer.Write(assetBundleData);
        writer.Close();
    }

    public static async UniTask<UnbeatableCharacterFile> Load(string filePath)
    {
        await using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);
        var magic = reader.ReadString();
        if (!magic.Equals(Magic, StringComparison.Ordinal)) throw new Exception("File magic invalid, wrong file type?");
        var version = reader.ReadString();
        if(!version.Equals(Version, StringComparison.Ordinal)) throw new Exception("Character file is not compatible with this version of the mod!");
        var characterInfoJson = reader.ReadString();
        var characterInfo = JsonConvert.DeserializeObject<UnbeatableCharacterInfo>(characterInfoJson, new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        var prefabName = reader.ReadString();
        var animatorName = reader.ReadString();
        var bytes = reader.ReadInt32();
        var bundleRequest = AssetBundle.LoadFromMemoryAsync(reader.ReadBytes(bytes));
        while (!bundleRequest.isDone) await UniTask.Yield();
        var bundle = bundleRequest.assetBundle;
        var assets = new Dictionary<string, Object>();
        foreach (var asset in bundle.GetAllAssetNames())
        {
            var loadedAssetRequest = bundle.LoadAssetAsync(asset);
            while(!loadedAssetRequest.isDone) await UniTask.Yield();
            var loadedAsset = loadedAssetRequest.asset;
            assets[asset] = loadedAsset;
        }
        var unload = bundle.UnloadAsync(false);
        while(!unload.isDone) await UniTask.Yield();
        return new UnbeatableCharacterFile(characterInfo, prefabName, animatorName, assets);
    }
    
    public void Dispose()
    {
        foreach (var (_, asset) in Assets) Resources.UnloadAsset(asset);
    }
}

[Serializable]
public class UnbeatableCharacterInfo
{
    [field: SerializeField] [JsonProperty] public string Name { get; private set; } = "Beat";
    [field: SerializeField] [JsonProperty] public string Author { get; private set; } = "D-Cell";

    [field: SerializeField] [JsonProperty] public bool OverwriteAllSpriteRendererMaterials { get; private set; } = false;
    [field: SerializeField] [JsonProperty] public AnimationBindings DefaultAnimationBindings { get; private set; }
    [field: SerializeField] [JsonProperty] public AnimationBindings BrawlAnimationBindings { get; private set; }
    [field: SerializeField] [JsonProperty] public AnimationBindings RunningAnimationBindings { get; private set; }
    [field: SerializeField] [JsonProperty] public AnimationBindings FallingAnimationBindings { get; private set; }
}

[Serializable]
public enum AnimationType
{
    Intro,
    AirBlock,
    GroundBlock,
    Falling,
    Idle,
    SideSwitch,
    HighAttacks,
    LowAttacks,
    Jumps,
    Lands,
    Hurts,
    Slams
}

[Serializable]
public class AnimationBindings
{
    [field: SerializeField] [JsonProperty] public string Intro { get; private set; } = "Intro";

    [field: SerializeField] [JsonProperty] public float IntroDuration { get; private set; } = 2;

    [field: SerializeField] [JsonProperty] public string AirBlock { get; private set; } = "AirBlock";

    [field: SerializeField] [JsonProperty] public string GroundBlock { get; private set; } = "GroundBlock";

    [field: SerializeField] [JsonProperty] public string Idle { get; private set; } = "Idle";

    [field: SerializeField] [JsonProperty] public string SideSwitch { get; private set; } = "SideSwitch";

    [field: SerializeField] [JsonProperty] public string[] HighAttacks { get; private set; } = new[] { "AttackHigh1" };

    [field: SerializeField] [JsonProperty] public string[] LowAttacks { get; private set; } = new[] { "AttackLow1" };

    [field: SerializeField] [JsonProperty] public string[] Jumps { get; private set; } = new[] { "Jump1" };

    [field: SerializeField] [JsonProperty] public string[] Lands { get; private set; }  = new[] { "Land1" };

    [field: SerializeField] [JsonProperty] public string[] Hurts { get; private set; } = new[] { "Hurt1" };

    [field: SerializeField] [JsonProperty] public bool HasSlams { get; private set; } = false;

    [field: SerializeField] [JsonProperty] public bool IgnoreSlams { get; private set; } = false;

    [field: SerializeField] [JsonProperty] public string[] SlamAttacks { get; private set; } = Array.Empty<string>();
    
    public bool HasState(AnimationType animationType)
    {
        return animationType switch
        {
            AnimationType.Intro => !string.IsNullOrEmpty(Intro),
            AnimationType.AirBlock => !string.IsNullOrEmpty(AirBlock),
            AnimationType.GroundBlock => !string.IsNullOrEmpty(GroundBlock),
            AnimationType.Idle => !string.IsNullOrEmpty(Idle),
            AnimationType.SideSwitch => !string.IsNullOrEmpty(SideSwitch),
            AnimationType.HighAttacks => HighAttacks.Length != 0,
            AnimationType.LowAttacks => LowAttacks.Length != 0,
            AnimationType.Jumps => Jumps.Length != 0,
            AnimationType.Lands => Lands.Length != 0,
            AnimationType.Hurts => Hurts.Length != 0,
            AnimationType.Slams => HasSlams && SlamAttacks.Length != 0,
            _ => false
        };
    }
}