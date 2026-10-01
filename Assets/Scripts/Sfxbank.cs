using UnityEngine;

public enum SfxId
{
    BaseClick,          // Clicking a cup base
    TrayFull,           // Clicked a base but all 4 slots are full
    IngredientClick,    // Ingredient click that counted
    IngredientRejected, // Ingredient click that did nothing
    CupPickUp,          // Dragging a cup
    CupPlaced,          // Dropped into a slot
    Trashed,            // Trash can drop
    DrinkFinished,      // Brewing/serving success
    BrewFailed,         // Recipe match failed / wrong order
    ButtonClick,        // Menu buttons
    BoardWhoosh,        // Board movement
    BrewingLoop,        // Machine brewing liquid/steam
    GameOverNightmare,  // Nightmare attack audio
    ShiftWinChime,      // 06:00 victory chime
    BoardClose          // Board exit sound
}

public class SfxBank : MonoBehaviour
{
    public static SfxBank Instance { get; private set; }

    [System.Serializable]
    public class SfxAudioItem
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Master Volume Controls")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.8f;
    [Range(0f, 1f)] public float ambienceVolume = 0.5f;

    [Header("Music & Ambience Audio Tracks")]
    public AudioClip bgmClip;
    public AudioClip ambienceClip;

    [Header("Individual SFX Audio Settings")]
    public SfxAudioItem baseClick;
    public SfxAudioItem trayFull;
    public SfxAudioItem ingredientClick;
    public SfxAudioItem ingredientRejected;
    public SfxAudioItem cupPickUp;
    public SfxAudioItem cupPlaced;
    public SfxAudioItem trashed;
    public SfxAudioItem drinkFinished;
    public SfxAudioItem brewFailed;
    public SfxAudioItem buttonClick;
    public SfxAudioItem boardWhoosh;
    public SfxAudioItem brewingLoop;
    public SfxAudioItem gameOverNightmare;
    public SfxAudioItem shiftWinChime;
    public SfxAudioItem boardClose;

    // AudioSources
    private AudioSource sfxSource;
    private AudioSource bgmSource;
    private AudioSource ambienceSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Setup AudioSource channels
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.loop = true;
        ambienceSource.playOnAwake = false;
    }

    void Start()
    {
        PlayBGM();
        PlayAmbience();
    }

    void Update()
    {
        // Live update looping audio volumes if adjusted in Inspector during runtime
        if (bgmSource != null)
            bgmSource.volume = bgmVolume * masterVolume;

        if (ambienceSource != null)
            ambienceSource.volume = ambienceVolume * masterVolume;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // --- SFX METHODS ---
    public static void Play(SfxId id)
    {
        if (Instance == null) return;

        SfxAudioItem item = Instance.ItemFor(id);
        if (item != null && item.clip != null)
        {
            float finalVol = item.volume * Instance.masterVolume;
            Instance.sfxSource.PlayOneShot(item.clip, finalVol);
        }
    }

    // --- BGM & AMBIENCE METHODS ---
    public static void PlayBGM()
    {
        if (Instance == null || Instance.bgmClip == null) return;
        Instance.bgmSource.clip = Instance.bgmClip;
        Instance.bgmSource.volume = Instance.bgmVolume * Instance.masterVolume;
        Instance.bgmSource.Play();
    }

    public static void StopBGM()
    {
        if (Instance == null) return;
        Instance.bgmSource.Stop();
    }

    public static void PlayAmbience()
    {
        if (Instance == null || Instance.ambienceClip == null) return;
        Instance.ambienceSource.clip = Instance.ambienceClip;
        Instance.ambienceSource.volume = Instance.ambienceVolume * Instance.masterVolume;
        Instance.ambienceSource.Play();
    }

    public static void StopAmbience()
    {
        if (Instance == null) return;
        Instance.ambienceSource.Stop();
    }

    SfxAudioItem ItemFor(SfxId id)
    {
        switch (id)
        {
            case SfxId.BaseClick:          return baseClick;
            case SfxId.TrayFull:           return trayFull;
            case SfxId.IngredientClick:    return ingredientClick;
            case SfxId.IngredientRejected: return ingredientRejected;
            case SfxId.CupPickUp:          return cupPickUp;
            case SfxId.CupPlaced:          return cupPlaced;
            case SfxId.Trashed:            return trashed;
            case SfxId.DrinkFinished:      return drinkFinished;
            case SfxId.BrewFailed:         return brewFailed;
            case SfxId.ButtonClick:        return buttonClick;
            case SfxId.BoardWhoosh:        return boardWhoosh;
            case SfxId.BrewingLoop:        return brewingLoop;
            case SfxId.GameOverNightmare:  return gameOverNightmare;
            case SfxId.ShiftWinChime:      return shiftWinChime;
            case SfxId.BoardClose:         return boardClose;
        }
        return null;
    }
}