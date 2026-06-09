using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    private const float DefaultFadeDuration = 0.4f;

    public static SceneLoader Instance { get; private set; }

    private Image _fadeImage;
    private bool _transitioning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject go = new GameObject("SceneLoader");
        go.AddComponent<SceneLoader>();
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

        BuildCanvas();
        SetAlpha(1f);
        _fadeImage.raycastTarget = true;
        StartCoroutine(InitialFadeIn());
    }

    public void Transition(string sceneName, float fadeDuration = DefaultFadeDuration)
    {
        if (_transitioning)
        {
            return;
        }

        StartCoroutine(TransitionRoutine(sceneName, fadeDuration));
    }

    public void Reload()
    {
        Transition(SceneManager.GetActiveScene().name);
    }

    public static void LoadByName(string sceneName)
    {
        if (Instance != null)
        {
            Instance.Transition(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public static void ReloadCurrent()
    {
        if (Instance != null)
        {
            Instance.Reload();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public static bool HasNext()
    {
        return SceneManager.GetActiveScene().buildIndex + 1
            < SceneManager.sceneCountInBuildSettings;
    }

    public static void LoadByIndex(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            return;
        }

        string sceneName = Path.GetFileNameWithoutExtension(
            SceneUtility.GetScenePathByBuildIndex(buildIndex)
        );

        if (Instance != null)
        {
            Instance.Transition(sceneName);
        }
        else
        {
            SceneManager.LoadScene(buildIndex);
        }
    }

    public static void LoadNext()
    {
        if (HasNext())
        {
            LoadByIndex(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void BuildCanvas()
    {
        GameObject canvasGo = new GameObject("FadeCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject imageGo = new GameObject("FadeImage");
        imageGo.transform.SetParent(canvasGo.transform, false);

        _fadeImage = imageGo.AddComponent<Image>();
        _fadeImage.color = new Color(0f, 0f, 0f, 0f);
        _fadeImage.raycastTarget = true;

        RectTransform rect = _fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private IEnumerator InitialFadeIn()
    {
        yield return Fade(1f, 0f, DefaultFadeDuration);
        _fadeImage.raycastTarget = false;
    }

    private IEnumerator TransitionRoutine(string sceneName, float fadeDuration)
    {
        _transitioning = true;
        _fadeImage.raycastTarget = true;

        yield return Fade(_fadeImage.color.a, 1f, fadeDuration);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            yield return null;
        }

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        yield return Fade(1f, 0f, fadeDuration);

        _fadeImage.raycastTarget = false;
        _transitioning = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetAlpha(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        Color c = _fadeImage.color;
        c.a = Mathf.Clamp01(alpha);
        _fadeImage.color = c;
    }
}
