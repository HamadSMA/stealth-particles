using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Serializable]
    public class LevelEntry
    {
        public LevelConfig config;
        public string sceneName;
        public Button button;
        public TMP_Text nameText;
        public TMP_Text rankText;
        public TMP_Text scoreText;
        public TMP_Text lockedText;
        public TMP_Text tapToPlayText;
    }

    [SerializeField]
    private Button playButton;

    [SerializeField]
    private LevelEntry[] levels;

    private void OnEnable()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayHighestUnlocked);
        }

        for (int i = 0; i < LevelCount; i++)
        {
            LevelEntry entry = levels[i];
            if (entry == null || entry.button == null)
            {
                continue;
            }

            string scene = entry.sceneName;
            entry.button.onClick.AddListener(delegate { LoadLevel(scene); });
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayHighestUnlocked);
        }

        for (int i = 0; i < LevelCount; i++)
        {
            if (levels[i] != null && levels[i].button != null)
            {
                levels[i].button.onClick.RemoveAllListeners();
            }
        }
    }

    private int LevelCount
    {
        get { return levels != null ? levels.Length : 0; }
    }

    public void Refresh()
    {
        for (int i = 0; i < LevelCount; i++)
        {
            LevelEntry entry = levels[i];
            if (entry == null)
            {
                continue;
            }

            int n = i + 1;
            bool unlocked = ProgressionManager.IsUnlocked(n);
            Rank rank = ProgressionManager.GetBestRank(n);
            int score = ProgressionManager.GetBestScore(n);

            if (entry.nameText != null)
            {
                entry.nameText.text = entry.config != null ? entry.config.levelName : "LEVEL " + n;
            }

            if (entry.button != null)
            {
                entry.button.interactable = unlocked;

                Image background = entry.button.GetComponent<Image>();
                if (background != null)
                {
                    background.color = unlocked
                        ? new Color(0.11f, 0.19f, 0.46f, 0.96f)
                        : new Color(0.02f, 0.03f, 0.07f, 0.92f);
                }

                Outline outline = entry.button.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = unlocked
                        ? new Color(0.4f, 0.88f, 1f, 1f)
                        : new Color(0.18f, 0.26f, 0.45f, 0.3f);
                }
            }

            if (entry.lockedText != null)
            {
                entry.lockedText.gameObject.SetActive(!unlocked);
                if (!unlocked)
                {
                    entry.lockedText.text = "LOCKED";
                }
            }

            if (entry.rankText != null)
            {
                entry.rankText.gameObject.SetActive(unlocked);
                if (unlocked)
                {
                    entry.rankText.text = rank == Rank.None ? "-" : rank.ToString();
                    entry.rankText.color = RankColor(rank);
                }
            }

            if (entry.scoreText != null)
            {
                bool showScore = unlocked && score > 0;
                entry.scoreText.gameObject.SetActive(showScore);
                if (showScore)
                {
                    entry.scoreText.text = "SCORE  " + score;
                }
            }

            if (entry.tapToPlayText != null)
            {
                entry.tapToPlayText.gameObject.SetActive(unlocked);
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

        if (highest - 1 < LevelCount && levels[highest - 1] != null)
        {
            LoadLevel(levels[highest - 1].sceneName);
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
