using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("gameManager")]
    private GameManager _gameManager;

    [SerializeField]
    [FormerlySerializedAs("briefingPanel")]
    private GameObject _briefingPanel;

    [SerializeField]
    [FormerlySerializedAs("hudPanel")]
    private GameObject _hudPanel;

    [SerializeField]
    [FormerlySerializedAs("successPanel")]
    private GameObject _successPanel;

    [SerializeField]
    [FormerlySerializedAs("failPanel")]
    private GameObject _failPanel;

    [SerializeField]
    [FormerlySerializedAs("levelNameText")]
    private TMP_Text _levelNameText;

    [SerializeField]
    [FormerlySerializedAs("objectiveText")]
    private TMP_Text _objectiveText;

    [SerializeField]
    [FormerlySerializedAs("timeBudgetText")]
    private TMP_Text _timeBudgetText;

    [SerializeField]
    [FormerlySerializedAs("timerText")]
    private TMP_Text _timerText;

    [SerializeField]
    [FormerlySerializedAs("startButton")]
    private Button _startButton;

    [SerializeField]
    [FormerlySerializedAs("rankText")]
    private TMP_Text _rankText;

    [SerializeField]
    [FormerlySerializedAs("successScoreText")]
    private TMP_Text _successScoreText;

    [SerializeField]
    [FormerlySerializedAs("successTimeText")]
    private TMP_Text _successTimeText;

    [SerializeField]
    [FormerlySerializedAs("replayButton")]
    private Button _replayButton;

    [SerializeField]
    [FormerlySerializedAs("nextButton")]
    private Button _nextButton;

    [SerializeField]
    [FormerlySerializedAs("menuButton")]
    private Button _menuButton;

    [SerializeField]
    [FormerlySerializedAs("failTimeText")]
    private TMP_Text _failTimeText;

    [SerializeField]
    [FormerlySerializedAs("retryButton")]
    private Button _retryButton;

    [SerializeField]
    [FormerlySerializedAs("quitButton")]
    private Button _quitButton;

    private float _timeBudget;

    private void Awake()
    {
        if (_gameManager == null)
        {
            _gameManager = Object.FindFirstObjectByType<GameManager>();
        }

        PopulateBriefing();
        ShowState(GameState.Briefing);
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        GameEvents.OnTimerUpdated += HandleTimerUpdated;

        AddClick(_startButton, StartLevel);
        AddClick(_replayButton, ReloadLevel);
        AddClick(_nextButton, NextLevel);
        AddClick(_menuButton, OpenMenu);
        AddClick(_retryButton, ReloadLevel);
        AddClick(_quitButton, Quit);
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        GameEvents.OnTimerUpdated -= HandleTimerUpdated;

        RemoveClick(_startButton, StartLevel);
        RemoveClick(_replayButton, ReloadLevel);
        RemoveClick(_nextButton, NextLevel);
        RemoveClick(_menuButton, OpenMenu);
        RemoveClick(_retryButton, ReloadLevel);
        RemoveClick(_quitButton, Quit);
    }

    public void StartLevel()
    {
        if (_gameManager != null)
        {
            _gameManager.StartLevel();
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
        if (_briefingPanel != null)
        {
            _briefingPanel.SetActive(state == GameState.Briefing);
        }

        if (_hudPanel != null)
        {
            _hudPanel.SetActive(state == GameState.Playing);
        }

        if (_successPanel != null)
        {
            _successPanel.SetActive(state == GameState.Success);
        }

        if (_failPanel != null)
        {
            _failPanel.SetActive(state == GameState.Fail);
        }
    }

    private void PopulateBriefing()
    {
        if (_gameManager == null)
        {
            return;
        }

        LevelConfig config = _gameManager.LevelConfig;
        if (config == null)
        {
            return;
        }

        _timeBudget = config.TimeBudget;

        if (_levelNameText != null)
        {
            _levelNameText.text = config.LevelName;
        }

        if (_objectiveText != null)
        {
            _objectiveText.text = config.ObjectiveText;
        }

        if (_timeBudgetText != null)
        {
            _timeBudgetText.text = "TIME LIMIT   " + Mathf.RoundToInt(_timeBudget) + "s";
        }
    }

    private void PopulateSuccess()
    {
        if (_gameManager != null)
        {
            Rank rank = _gameManager.LastRank;

            if (_rankText != null)
            {
                _rankText.text = rank == Rank.None ? "-" : rank.ToString();
                _rankText.color = RankColor(rank);
            }

            if (_successScoreText != null)
            {
                _successScoreText.text = "SCORE   " + _gameManager.LastScore;
            }

            if (_successTimeText != null)
            {
                _successTimeText.text = "TIME   " + FormatTime(_gameManager.LastTime);
            }
        }

        if (_nextButton != null)
        {
            _nextButton.gameObject.SetActive(SceneLoader.HasNext());
        }
    }

    private void PopulateFail()
    {
        if (_failTimeText != null)
        {
            float time = _gameManager != null ? _gameManager.LastTime : 0f;
            _failTimeText.text = "TIME   " + FormatTime(time);
        }
    }

    private void HandleTimerUpdated(float elapsed)
    {
        if (_timerText == null)
        {
            return;
        }

        float remaining = Mathf.Max(0f, _timeBudget - elapsed);
        _timerText.text = "TIME   " + remaining.ToString("0.0") + "s";
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
