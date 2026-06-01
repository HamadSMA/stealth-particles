using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    private float musicVolume = 1f;

    [SerializeField]
    private float sfxVolume = 1f;

    [SerializeField]
    private float fadeDuration = 0.7f;

    private AudioSource _music;
    private AudioSource _sfx;
    private Coroutine _fade;
    private AudioConfig _config;

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

        AudioConfig[] configs = Resources.LoadAll<AudioConfig>(string.Empty);
        _config = configs.Length > 0 ? configs[0] : null;
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                PlayMusic(CurrentLevelTrack());
                break;
            case GameState.Success:
                PlayResultMusic(_config != null ? _config.scoreMusic : null);
                break;
            case GameState.Fail:
                PlayResultMusic(_config != null ? _config.gameOverMusic : null);
                break;
        }
    }

    private void PlayResultMusic(AudioClip clip)
    {
        if (clip != null)
        {
            PlayMusic(clip);
        }
        else
        {
            FadeOutMusic();
        }
    }

    private static AudioClip CurrentLevelTrack()
    {
        GameManager manager = FindFirstObjectByType<GameManager>();
        if (manager != null && manager.LevelConfig != null)
        {
            return manager.LevelConfig.musicTrack;
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

        if (_fade != null)
        {
            StopCoroutine(_fade);
            _fade = null;
        }

        _music.clip = clip;
        _music.loop = true;
        _music.volume = musicVolume;
        _music.Play();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (_sfx == null || clip == null)
        {
            return;
        }

        _sfx.PlayOneShot(clip, sfxVolume);
    }

    public void PlaySfx(AudioClip clip, float volumeScale)
    {
        if (_sfx == null || clip == null)
        {
            return;
        }

        _sfx.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volumeScale));
    }

    public void StopMusic()
    {
        if (_music == null)
        {
            return;
        }

        if (_fade != null)
        {
            StopCoroutine(_fade);
            _fade = null;
        }

        _music.Stop();
        _music.clip = null;
    }

    private void FadeOutMusic()
    {
        if (_music == null || !_music.isPlaying)
        {
            return;
        }

        if (_fade != null)
        {
            StopCoroutine(_fade);
        }

        _fade = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float startVolume = _music.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration && _music.isPlaying)
        {
            elapsed += Time.unscaledDeltaTime;
            _music.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        _music.Stop();
        _music.clip = null;
        _music.volume = musicVolume;
        _fade = null;
    }
}
