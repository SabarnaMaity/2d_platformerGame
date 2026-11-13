using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Level Buttons (10 Buttons)")]
    [SerializeField] private Button[] levelButtons;

    [Header("Back Button")]
    [SerializeField] private Button backButton;

    [Header("Visual Colors")]
    [SerializeField] private Color unlockedColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
    [SerializeField] private Color completedColor = new Color(0.9f, 1f, 0.8f, 1f);

    private void Start()
    {
        // Auto-find buttons if not manually assigned in inspector
        if ((levelButtons == null || levelButtons.Length < 10) && levelSelectPanel != null)
        {
            Button[] foundButtons = levelSelectPanel.GetComponentsInChildren<Button>(true);
            var btnList = new System.Collections.Generic.List<Button>();
            foreach (Button b in foundButtons)
            {
                if (b.gameObject.name.StartsWith("LevelButton_") || b.gameObject.name.StartsWith("Btn_Level") || b.gameObject.name.StartsWith("Level"))
                {
                    btnList.Add(b);
                }
            }
            if (btnList.Count > 0)
            {
                // Sort numerically based on integer in button name
                btnList.Sort((a, b) => ExtractLevelNumber(a.name).CompareTo(ExtractLevelNumber(b.name)));
                levelButtons = btnList.ToArray();
            }
        }

        // Auto-find and wire up BackButton on levelSelectPanel
        if (backButton == null && levelSelectPanel != null)
        {
            Button[] allBtns = levelSelectPanel.GetComponentsInChildren<Button>(true);
            foreach (Button b in allBtns)
            {
                if (b.gameObject.name.ToLower().Contains("back"))
                {
                    backButton = b;
                    break;
                }
            }
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(CloseLevelSelect);
            backButton.onClick.AddListener(CloseLevelSelect);
        }

        SetupLevelButtons();

        // Check if we should immediately open Level Selection (e.g. from in-game "Level Select" button)
        if (LevelManager.OpenLevelSelectOnMenuLoad)
        {
            LevelManager.OpenLevelSelectOnMenuLoad = false;
            OpenLevelSelect();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void Update()
    {
        // Support Android back key / PC Escape key to return to Main Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (levelSelectPanel != null && levelSelectPanel.activeSelf)
            {
                CloseLevelSelect();
            }
        }
    }

    public void SetupLevelButtons()
    {
        if (levelButtons == null) return;

        int unlockedLevel = LevelManager.GetUnlockedLevel();

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNum = i + 1;
            Button btn = levelButtons[i];
            if (btn == null) continue;

            bool isUnlocked = LevelManager.IsLevelUnlocked(levelNum);
            bool isCompleted = LevelManager.IsLevelCompleted(levelNum);

            btn.interactable = isUnlocked;

            // Target graphic color
            Image img = btn.targetGraphic as Image;
            if (img != null)
            {
                if (!isUnlocked)
                {
                    img.color = lockedColor;
                }
                else if (isCompleted)
                {
                    img.color = completedColor;
                }
                else
                {
                    img.color = unlockedColor;
                }
            }

            // Update child texts if present
            TMP_Text tmp = btn.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                if (isCompleted)
                {
                    tmp.text = $"Level {levelNum}\n<size=65%><color=#66FF66>CLEARED</color></size>";
                }
                else if (isUnlocked)
                {
                    tmp.text = $"Level {levelNum}";
                }
                else
                {
                    tmp.text = $"Level {levelNum}\n<size=65%><color=#888888>LOCKED</color></size>";
                }
            }

            // Remove previous listeners and add dynamic listener
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnLevelButtonClicked(levelNum));
        }
    }

    public void OnLevelButtonClicked(int levelNumber)
    {
        LevelManager.LoadLevel(levelNumber);
    }

    public void OpenLevelSelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
        SetupLevelButtons();
    }

    public void ShowMainMenu()
    {
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        ShowMainMenu();
    }

    public void ResetProgress()
    {
        LevelManager.ResetProgress();
        SetupLevelButtons();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private int ExtractLevelNumber(string name)
    {
        string numStr = System.Text.RegularExpressions.Regex.Match(name, @"\d+").Value;
        if (int.TryParse(numStr, out int num))
        {
            return num;
        }
        return 0;
    }
}
