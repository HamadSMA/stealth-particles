using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private ScoreCalculator scoreCalculator;

    [SerializeField]
    private GameObject briefingPanel;

    [SerializeField]
    private GameObject hudPanel;

    [SerializeField]
    private GameObject successPanel;

    [SerializeField]
    private GameObject failPanel;

    [SerializeField]
    private TMP_Text levelNameText;

    [SerializeField]
    private TMP_Text objectiveText;

    [SerializeField]
    private TMP_Text timeBudgetText;

    [SerializeField]
    private TMP_Text timerText;

    [SerializeField]
    private Button startButton;

    [SerializeField]
    private TMP_Text rankText;

    [SerializeField]
    private TMP_Text successScoreText;

    [SerializeField]
    private TMP_Text successTimeText;

    [SerializeField]
    private Button replayButton;

    [SerializeField]
    private Button nextButton;

    [SerializeField]
    private Button menuButton;

    [SerializeField]
    private TMP_Text failTimeText;

    [SerializeField]
    private Button retryButton;

    [SerializeField]
    private Button quitButton;

    private float timeBudget;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = Object.FindFirstObjectByType<GameManager>();
        }

        if (scoreCalculator == null)
        {
            scoreCalculator = Object.FindFirstObjectByType<ScoreCalculator>();
        }

        PopulateBriefing();
        ShowState(GameState.Briefing);
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        GameEvents.OnTimerUpdated += HandleTimerUpdated;

        AddClick(startButton, StartLevel);
        AddClick(replayButton, ReloadLevel);
        AddClick(nextButton, NextLevel);
        AddClick(menuButton, OpenMenu);
        AddClick(retryButton, ReloadLevel);
        AddClick(quitButton, Quit);
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        GameEvents.OnTimerUpdated -= HandleTimerUpdated;

        RemoveClick(startButton, StartLevel);
        RemoveClick(replayButton, ReloadLevel);
        RemoveClick(nextButton, NextLevel);
        RemoveClick(menuButton, OpenMenu);
        RemoveClick(retryButton, ReloadLevel);
        RemoveClick(quitButton, Quit);
    }

    public void StartLevel()
    {
        if (gameManager != null)
        {
            gameManager.StartLevel();
        }
    }

    public void ReloadLevel()
    {
        SceneLoader.ReloadCurrent();
    }

    public void NextLevel()
    {
        SceneLoader.LoadNext();
    }

    public void OpenMenu()
    {
        SceneLoader.LoadByName("MainMenu");
    }

    public void Quit()
    {
        SceneLoader.LoadByName("MainMenu");
    }

    private void HandleGameStateChanged(GameState state)
    {
        ShowState(state);

        if (state == GameState.Success)
        {
            PopulateSuccess();
        }
        else if (state == GameState.Fail)
        {
            PopulateFail();
        }
    }

    private void ShowState(GameState state)
    {
        if (briefingPanel != null)
        {
            briefingPanel.SetActive(state == GameState.Briefing);
        }

        if (hudPanel != null)
        {
            hudPanel.SetActive(state == GameState.Playing);
        }

        if (successPanel != null)
        {
            successPanel.SetActive(state == GameState.Success);
        }

        if (failPanel != null)
        {
            failPanel.SetActive(state == GameState.Fail);
        }
    }

    private void PopulateBriefing()
    {
        if (gameManager == null)
        {
            return;
        }

        LevelConfig config = gameManager.LevelConfig;
        if (config == null)
        {
            return;
        }

        timeBudget = config.timeBudget;

        if (levelNameText != null)
        {
            levelNameText.text = config.levelName;
        }

        if (objectiveText != null)
        {
            objectiveText.text = config.objectiveText;
        }

        if (timeBudgetText != null)
        {
            timeBudgetText.text = "TIME LIMIT   " + Mathf.RoundToInt(timeBudget) + "s";
        }
    }

    private void PopulateSuccess()
    {
        if (scoreCalculator != null)
        {
            Rank rank = scoreCalculator.LastRank;

            if (rankText != null)
            {
                rankText.text = rank == Rank.None ? "-" : rank.ToString();
                rankText.color = RankColor(rank);
            }

            if (successScoreText != null)
            {
                successScoreText.text = "SCORE   " + scoreCalculator.LastScore;
            }

            if (successTimeText != null)
            {
                successTimeText.text = "TIME   " + FormatTime(scoreCalculator.LastTime);
            }
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(SceneLoader.HasNext());
        }
    }

    private void PopulateFail()
    {
        if (failTimeText != null)
        {
            float time = scoreCalculator != null ? scoreCalculator.LastTime : 0f;
            failTimeText.text = "TIME   " + FormatTime(time);
        }
    }

    private void HandleTimerUpdated(float elapsed)
    {
        if (timerText == null)
        {
            return;
        }

        float remaining = Mathf.Max(0f, timeBudget - elapsed);
        timerText.text = "TIME   " + remaining.ToString("0.0") + "s";
    }

    private static Color RankColor(Rank rank)
    {
        switch (rank)
        {
            case Rank.S:
                return new Color(1f, 0.2f, 0.6f);
            case Rank.A:
                return new Color(1f, 0.84f, 0.2f);
            case Rank.B:
                return new Color(0.3f, 0.9f, 1f);
            case Rank.C:
                return Color.white;
            default:
                return new Color(0.6f, 0.6f, 0.7f);
        }
    }

    private static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        float remainder = seconds - minutes * 60f;
        return minutes.ToString("00") + ":" + remainder.ToString("00.0");
    }

    private static void AddClick(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void RemoveClick(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }
}
