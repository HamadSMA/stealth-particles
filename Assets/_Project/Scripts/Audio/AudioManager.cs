using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    [FormerlySerializedAs("musicVolume")]
    private float _musicVolume = 1f;

    [SerializeField]
    [FormerlySerializedAs("sfxVolume")]
    private float _sfxVolume = 1f;

    private AudioSource _music;
    private AudioSource _sfx;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _music = gameObject.AddComponent<AudioSource>();
        _music.loop = true;
        _music.playOnAwake = false;
        _music.spatialBlend = 0f;

        _sfx = gameObject.AddComponent<AudioSource>();
        _sfx.loop = false;
        _sfx.playOnAwake = false;
        _sfx.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopMusic();
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Playing)
        {
            PlayMusic(CurrentLevelTrack());
        }
    }

    private static AudioClip CurrentLevelTrack()
    {
        GameManager manager = FindFirstObjectByType<GameManager>();
        if (manager != null && manager.LevelConfig != null)
        {
            return manager.LevelConfig.MusicTrack;
        }

        return null;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (_music == null)
        {
            return;
        }

        if (clip == null)
        {
            StopMusic();
            return;
        }

        if (_music.clip == clip && _music.isPlaying)
        {
            return;
        }

        _music.clip = clip;
        _music.loop = true;
        _music.volume = _musicVolume;
        _music.Play();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (_sfx == null || clip == null)
        {
            return;
        }

        _sfx.PlayOneShot(clip, _sfxVolume);
    }

    public void PlaySfx(AudioClip clip, float volumeScale)
    {
        if (_sfx == null || clip == null)
        {
            return;
        }

        _sfx.PlayOneShot(clip, _sfxVolume * Mathf.Clamp01(volumeScale));
    }

    public void StopMusic()
    {
        if (_music == null)
        {
            return;
        }

        _music.Stop();
        _music.clip = null;
    }
}
