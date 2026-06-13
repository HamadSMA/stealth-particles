using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Serializable]
    public class LevelEntry
    {
        [FormerlySerializedAs("config")]
        public LevelConfig Config;

        [FormerlySerializedAs("sceneName")]
        public string SceneName;

        [FormerlySerializedAs("button")]
        public Button Button;

        [FormerlySerializedAs("nameText")]
        public TMP_Text NameText;

        [FormerlySerializedAs("rankText")]
        public TMP_Text RankText;

        [FormerlySerializedAs("scoreText")]
        public TMP_Text ScoreText;

        [FormerlySerializedAs("lockedText")]
        public TMP_Text LockedText;

        [FormerlySerializedAs("tapToPlayText")]
        public TMP_Text TapToPlayText;
    }

    [SerializeField]
    [FormerlySerializedAs("playButton")]
    private Button _playButton;

    [SerializeField]
    [FormerlySerializedAs("levels")]
    private LevelEntry[] _levels;

    private void OnEnable()
    {
        if (_playButton != null)
        {
            _playButton.onClick.AddListener(PlayHighestUnlocked);
        }

        for (int i = 0; i < LevelCount; i++)
        {
            LevelEntry entry = _levels[i];
            if (entry == null || entry.Button == null)
            {
                continue;
            }

            string scene = entry.SceneName;
            entry.Button.onClick.AddListener(
                delegate
                {
                    LoadLevel(scene);
                }
            );
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (_playButton != null)
        {
            _playButton.onClick.RemoveListener(PlayHighestUnlocked);
        }

        for (int i = 0; i < LevelCount; i++)
        {
            if (_levels[i] != null && _levels[i].Button != null)
            {
                _levels[i].Button.onClick.RemoveAllListeners();
            }
        }
    }

    private int LevelCount
    {
        get { return _levels != null ? _levels.Length : 0; }
    }

    public void Refresh()
    {
        for (int i = 0; i < LevelCount; i++)
        {
            LevelEntry entry = _levels[i];
            if (entry == null)
            {
                continue;
            }

            int n = i + 1;
            bool unlocked = ProgressionManager.IsUnlocked(n);
            Rank rank = ProgressionManager.GetBestRank(n);
            int score = ProgressionManager.GetBestScore(n);

            if (entry.NameText != null)
            {
                entry.NameText.text = entry.Config != null ? entry.Config.LevelName : "LEVEL " + n;
            }

            if (entry.Button != null)
            {
                entry.Button.interactable = unlocked;

                Image background = entry.Button.GetComponent<Image>();
                if (background != null)
                {
                    background.color = unlocked
                        ? new Color(0.11f, 0.19f, 0.46f, 0.96f)
                        : new Color(0.02f, 0.03f, 0.07f, 0.92f);
                }

                Outline outline = entry.Button.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = unlocked
                        ? new Color(0.4f, 0.88f, 1f, 1f)
                        : new Color(0.18f, 0.26f, 0.45f, 0.3f);
                }
            }

            if (entry.LockedText != null)
            {
                entry.LockedText.gameObject.SetActive(!unlocked);
                if (!unlocked)
                {
                    entry.LockedText.text = "LOCKED";
                }
            }

            if (entry.RankText != null)
            {
                entry.RankText.gameObject.SetActive(unlocked);
                if (unlocked)
                {
                    entry.RankText.text = rank == Rank.None ? "-" : rank.ToString();
                    entry.RankText.color = RankColor(rank);
                }
            }

            if (entry.ScoreText != null)
            {
                bool showScore = unlocked && score > 0;
                entry.ScoreText.gameObject.SetActive(showScore);
                if (showScore)
                {
                    entry.ScoreText.text = "SCORE  " + score;
                }
            }

            if (entry.TapToPlayText != null)
            {
                entry.TapToPlayText.gameObject.SetActive(unlocked);
            }
        }
    }

    public void PlayHighestUnlocked()
    {
        int highest = 1;
        for (int n = 1; n <= LevelCount; n++)
        {
            if (ProgressionManager.IsUnlocked(n))
            {
                highest = n;
            }
        }

        if (highest - 1 < LevelCount && _levels[highest - 1] != null)
        {
            LoadLevel(_levels[highest - 1].SceneName);
        }
    }

    private void LoadLevel(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneLoader.LoadByName(sceneName);
        }
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
}
