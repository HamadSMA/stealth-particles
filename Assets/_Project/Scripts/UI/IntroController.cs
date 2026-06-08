using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using TMPro;

public class IntroController : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("scrollText")]
    private TMP_Text _scrollText;

    [SerializeField]
    [FormerlySerializedAs("canvasRect")]
    private RectTransform _canvasRect;

    [SerializeField]
    [FormerlySerializedAs("musicSource")]
    private AudioSource _musicSource;

    [SerializeField]
    [FormerlySerializedAs("scrollSpeed")]
    private float _scrollSpeed = 220f;

    [SerializeField]
    [FormerlySerializedAs("mainMenuSceneName")]
    private string _mainMenuSceneName = "MainMenu";

    private RectTransform _textRect;
    private float _endY;
    private bool _loading;

    private void Start()
    {
        if (_musicSource != null && _musicSource.clip != null)
        {
            _musicSource.loop = true;
            _musicSource.Play();
        }

        if (_scrollText == null || _canvasRect == null)
        {
            return;
        }

        _textRect = _scrollText.rectTransform;
        _scrollText.ForceMeshUpdate();
        float textHeight = _scrollText.preferredHeight;
        _textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

        float canvasHeight = _canvasRect.rect.height;
        float startY = -(canvasHeight * 0.5f + textHeight * 0.5f);
        _endY = canvasHeight * 0.5f + textHeight * 0.5f;

        Vector2 p = _textRect.anchoredPosition;
        _textRect.anchoredPosition = new Vector2(p.x, startY);
    }

    private void Update()
    {
        if (_loading)
        {
            return;
        }

        if (WasTapped())
        {
            LoadMainMenu();
            return;
        }

        if (_textRect == null)
        {
            return;
        }

        Vector2 p = _textRect.anchoredPosition;
        p.y += _scrollSpeed * Time.deltaTime;
        _textRect.anchoredPosition = p;

        if (p.y >= _endY)
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
        if (_loading)
        {
            return;
        }

        _loading = true;

        if (_musicSource != null && _musicSource.isPlaying)
        {
            _musicSource.Stop();
        }

        SceneLoader.LoadByName(_mainMenuSceneName);
    }
}
