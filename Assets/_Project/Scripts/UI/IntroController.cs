using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class IntroController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text scrollText;

    [SerializeField]
    private RectTransform canvasRect;

    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private float scrollSpeed = 220f;

    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    private RectTransform textRect;
    private float endY;
    private bool loading;

    private void Start()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }

        if (scrollText == null || canvasRect == null)
        {
            return;
        }

        textRect = scrollText.rectTransform;
        scrollText.ForceMeshUpdate();
        float textHeight = scrollText.preferredHeight;
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

        float canvasHeight = canvasRect.rect.height;
        float startY = -(canvasHeight * 0.5f + textHeight * 0.5f);
        endY = canvasHeight * 0.5f + textHeight * 0.5f;

        Vector2 p = textRect.anchoredPosition;
        textRect.anchoredPosition = new Vector2(p.x, startY);
    }

    private void Update()
    {
        if (loading)
        {
            return;
        }

        if (WasTapped())
        {
            LoadMainMenu();
            return;
        }

        if (textRect == null)
        {
            return;
        }

        Vector2 p = textRect.anchoredPosition;
        p.y += scrollSpeed * Time.deltaTime;
        textRect.anchoredPosition = p;

        if (p.y >= endY)
        {
            LoadMainMenu();
        }
    }

    private static bool WasTapped()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            return true;
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private void LoadMainMenu()
    {
        if (loading)
        {
            return;
        }

        loading = true;

        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        SceneLoader.LoadByName(mainMenuSceneName);
    }
}
