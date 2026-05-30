using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class ResultsScreen : MonoBehaviour
{
    [SerializeField]
    private ScoreCalculator scoreCalculator;

    [SerializeField]
    private GameObject panel;

    [SerializeField]
    private GameObject successGroup;

    [SerializeField]
    private GameObject failGroup;

    [SerializeField]
    private TMP_Text rankText;

    [SerializeField]
    private TMP_Text scoreText;

    [SerializeField]
    private TMP_Text timeText;

    [SerializeField]
    private TMP_Text failTimeText;

    [SerializeField]
    private Button replayButton;

    [SerializeField]
    private Button menuButton;

    private void Awake()
    {
        if (replayButton != null)
        {
            replayButton.onClick.AddListener(Replay);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OpenMenu);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        Hide();
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Success:
                ShowSuccess();
                break;
            case GameState.Fail:
                ShowFail();
                break;
            default:
                Hide();
                break;
        }
    }

    private void ShowSuccess()
    {
        if (successGroup != null)
        {
            successGroup.SetActive(true);
        }

        if (failGroup != null)
        {
            failGroup.SetActive(false);
        }

        if (rankText != null)
        {
            rankText.text = scoreCalculator != null ? scoreCalculator.LastRank.ToString() : "-";
        }

        if (scoreText != null)
        {
            scoreText.text = "SCORE   " + (scoreCalculator != null ? scoreCalculator.LastScore : 0);
        }

        if (timeText != null)
        {
            timeText.text = "TIME   " + FormatTime(scoreCalculator != null ? scoreCalculator.LastTime : 0f);
        }

        Show();
    }

    private void ShowFail()
    {
        if (successGroup != null)
        {
            successGroup.SetActive(false);
        }

        if (failGroup != null)
        {
            failGroup.SetActive(true);
        }

        if (failTimeText != null)
        {
            failTimeText.text = "TIME   " + FormatTime(scoreCalculator != null ? scoreCalculator.LastTime : 0f);
        }

        Show();
    }

    private void Show()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void Replay()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    private void OpenMenu()
    {
        Debug.Log("menu not implemented yet");
    }

    private string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        float remaining = seconds - (minutes * 60f);
        return minutes.ToString("00") + ":" + remaining.ToString("00.00");
    }
}
