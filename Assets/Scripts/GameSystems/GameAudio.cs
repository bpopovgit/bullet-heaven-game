using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameSfxId
{
    None,
    PlayerShoot,
    EnemyShoot,
    EnemyDeath,
    PlayerHit,
    PlayerDeath,
    XPGem,
    LevelUp,
    HealthPickup,
    MagnetPickup,
    BombPickup,
    EliteSpawn,
    EliteDefeated,
    UISelect
}

public class GameAudio : MonoBehaviour
{
    [Serializable]
    private class SfxDefinition
    {
        public GameSfxId id;
        public string resourcesPath;
        [Range(0f, 1f)] public float volume = 1f;
        public float pitchMin = 1f;
        public float pitchMax = 1f;

        [NonSerialized] public AudioClip[] cachedClips;
    }

    public static GameAudio Instance { get; private set; }

    [Header("Mix")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Playback")]
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField, Min(1)] private int sfxVoiceCount = 12;

    [Header("SFX Library")]
    [SerializeField] private SfxDefinition[] sfxDefinitions;

    private readonly Dictionary<GameSfxId, SfxDefinition> _definitions = new Dictionary<GameSfxId, SfxDefinition>();
    private AudioSource[] _sfxVoices;
    private int _nextVoiceIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        EnsureAudioVoices();
        EnsureDefaultDefinitions();
        RebuildLookup();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void Play(GameSfxId id)
    {
        if (id == GameSfxId.None)
            return;

        EnsureInstance().PlayInternal(id);
    }

    public static void PlayPlayerShoot() => Play(GameSfxId.PlayerShoot);
    public static void PlayEnemyShoot() => Play(GameSfxId.EnemyShoot);
    public static void PlayEnemyDeath() => Play(GameSfxId.EnemyDeath);
    public static void PlayPlayerHit() => Play(GameSfxId.PlayerHit);
    public static void PlayPlayerDeath() => Play(GameSfxId.PlayerDeath);
    public static void PlayXPGem() => Play(GameSfxId.XPGem);
    public static void PlayLevelUp() => Play(GameSfxId.LevelUp);
    public static void PlayEliteSpawn() => Play(GameSfxId.EliteSpawn);
    public static void PlayEliteDefeated() => Play(GameSfxId.EliteDefeated);
    public static void PlayUISelect() => Play(GameSfxId.UISelect);

    private static GameAudio EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        GameAudio existing = FindObjectOfType<GameAudio>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject("GameAudio");
        return go.AddComponent<GameAudio>();
    }

    private void EnsureAudioVoices()
    {
        AudioSource[] existingSources = GetComponents<AudioSource>();
        int targetCount = Mathf.Max(1, sfxVoiceCount);

        if (existingSources.Length < targetCount)
        {
            for (int i = existingSources.Length; i < targetCount; i++)
                gameObject.AddComponent<AudioSource>();

            existingSources = GetComponents<AudioSource>();
        }

        _sfxVoices = existingSources;

        foreach (AudioSource source in _sfxVoices)
        {
            if (source == null)
                continue;

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
        }
    }

    private void EnsureDefaultDefinitions()
    {
        if (sfxDefinitions != null && sfxDefinitions.Length > 0)
            return;

        sfxDefinitions = new[]
        {
            CreateDefault(GameSfxId.PlayerShoot, "Audio/SFX/PlayerShoot", 0.8f, 0.95f, 1.05f),
            CreateDefault(GameSfxId.EnemyShoot, "Audio/SFX/EnemyShoot", 0.7f, 0.95f, 1.05f),
            CreateDefault(GameSfxId.EnemyDeath, "Audio/SFX/EnemyDeath", 0.85f, 0.95f, 1.1f),
            CreateDefault(GameSfxId.PlayerHit, "Audio/SFX/PlayerHit", 0.9f, 0.95f, 1.05f),
            CreateDefault(GameSfxId.PlayerDeath, "Audio/SFX/PlayerDeath", 1f, 1f, 1f),
            CreateDefault(GameSfxId.XPGem, "Audio/SFX/XPGem", 0.65f, 0.98f, 1.05f),
            CreateDefault(GameSfxId.LevelUp, "Audio/SFX/LevelUp", 1f, 1f, 1f),
            CreateDefault(GameSfxId.HealthPickup, "Audio/SFX/HealthPickup", 0.85f, 0.98f, 1.02f),
            CreateDefault(GameSfxId.MagnetPickup, "Audio/SFX/MagnetPickup", 0.85f, 0.98f, 1.02f),
            CreateDefault(GameSfxId.BombPickup, "Audio/SFX/BombPickup", 0.95f, 0.98f, 1.02f),
            CreateDefault(GameSfxId.EliteSpawn, "Audio/SFX/EliteSpawn", 1f, 1f, 1f),
            CreateDefault(GameSfxId.EliteDefeated, "Audio/SFX/EliteDefeated", 1f, 1f, 1f),
            CreateDefault(GameSfxId.UISelect, "Audio/SFX/UISelect", 0.7f, 1f, 1f)
        };
    }

    private void RebuildLookup()
    {
        _definitions.Clear();

        if (sfxDefinitions == null)
            return;

        foreach (SfxDefinition definition in sfxDefinitions)
        {
            if (definition == null || definition.id == GameSfxId.None)
                continue;

            _definitions[definition.id] = definition;
        }
    }

    private void PlayInternal(GameSfxId id)
    {
        if (_sfxVoices == null || _sfxVoices.Length == 0)
            EnsureAudioVoices();

        if (!_definitions.TryGetValue(id, out SfxDefinition definition) || definition == null)
            return;

        AudioClip clip = GetRandomClip(definition);
        if (clip == null)
            return;

        AudioSource source = GetNextVoice();
        if (source == null)
            return;

        source.Stop();
        source.clip = clip;
        source.volume = Mathf.Clamp01(masterVolume) * Mathf.Clamp01(sfxVolume) * Mathf.Clamp01(definition.volume);
        source.pitch = GetPitch(definition);
        source.Play();
    }

    private AudioClip GetRandomClip(SfxDefinition definition)
    {
        if (definition.cachedClips == null || definition.cachedClips.Length == 0)
            definition.cachedClips = LoadClips(definition.resourcesPath);

        if (definition.cachedClips == null || definition.cachedClips.Length == 0)
            return null;

        int index = UnityEngine.Random.Range(0, definition.cachedClips.Length);
        return definition.cachedClips[index];
    }

    private static AudioClip[] LoadClips(string resourcesPath)
    {
        if (string.IsNullOrWhiteSpace(resourcesPath))
            return Array.Empty<AudioClip>();

        return Resources.LoadAll<AudioClip>(resourcesPath);
    }

    private static float GetPitch(SfxDefinition definition)
    {
        float min = Mathf.Max(0.1f, definition.pitchMin);
        float max = Mathf.Max(min, definition.pitchMax);
        return UnityEngine.Random.Range(min, max);
    }

    private static SfxDefinition CreateDefault(GameSfxId id, string resourcesPath, float volume, float pitchMin, float pitchMax)
    {
        return new SfxDefinition
        {
            id = id,
            resourcesPath = resourcesPath,
            volume = volume,
            pitchMin = pitchMin,
            pitchMax = pitchMax
        };
    }

    private AudioSource GetNextVoice()
    {
        if (_sfxVoices == null || _sfxVoices.Length == 0)
            return null;

        AudioSource source = _sfxVoices[_nextVoiceIndex % _sfxVoices.Length];
        _nextVoiceIndex = (_nextVoiceIndex + 1) % _sfxVoices.Length;
        return source;
    }
}
