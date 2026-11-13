using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }

    [SerializeField] private TMP_Text coinText;
    [SerializeField] private PlayerController playerController;

    private int coinCount = 0;
    private int gemCount = 0;
    private bool isGameOver = false;
    private bool isRespawning = false;
    private Vector3 playerPosition;

    // Level Complete
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TMP_Text leveCompletePanelTitle;
    [SerializeField] private TMP_Text levelCompleteCoins;
    [SerializeField] private GameObject nextLevelButton;

    private int totalCoins = 0;

    private void Awake()
    {
        _instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        UpdateGUI();
        if (UIManager.instance != null)
        {
            UIManager.instance.fadeFromBlack = true;
        }

        if (playerController == null)
        {
            playerController = Object.FindAnyObjectByType<PlayerController>();
        }

        if (playerController != null)
        {
            playerPosition = playerController.transform.position;
        }

        FindTotalPickups();

        // Reset Lives to 3 on starting a new level
        if (HealthManager.instance != null)
        {
            HealthManager.instance.ResetLives();
        }

        // If nextLevelButton wasn't explicitly assigned in the inspector, try to find it under levelCompletePanel
        if (nextLevelButton == null && levelCompletePanel != null)
        {
            Transform nextBtnTransform = levelCompletePanel.transform.Find("NextLevelButton");
            if (nextBtnTransform != null)
            {
                nextLevelButton = nextBtnTransform.gameObject;
            }
        }
    }

    public void IncrementCoinCount()
    {
        coinCount++;
        UpdateGUI();
    }

    public void IncrementGemCount()
    {
        gemCount++;
        UpdateGUI();
    }

    private void UpdateGUI()
    {
        if (coinText != null)
        {
            coinText.text = coinCount.ToString();
        }
    }

    public void Death()
    {
        // Avoid multiple life deductions from a single death event
        if (isRespawning || isGameOver) return;
        isRespawning = true;

        StartCoroutine(RespawnCoroutine());
    }

    public IEnumerator RespawnCoroutine()
    {
        Debug.Log("Player died - Processing life deduction and respawn");

        // 1. Temporarily disable player movement
        if (playerController != null)
        {
            playerController.SetMovementLocked(true);
            if (playerController.ImpactEffect != null)
            {
                playerController.ImpactEffect.transform.position = playerController.transform.position;
                playerController.ImpactEffect.Play();
            }
        }

        // 2. Reduce life count and update Heart UI immediately (cycles 3 -> 2 -> 1 -> 0 -> resets to 3)
        if (HealthManager.instance != null)
        {
            HealthManager.instance.LoseLife();
        }

        // 3. Briefly disable mobile controls and fade screen
        if (UIManager.instance != null)
        {
            UIManager.instance.DisableMobileControls();
            UIManager.instance.fadeToBlack = true;
        }

        yield return new WaitForSeconds(0.6f);

        // 4. Respawn player at checkpoint / starting position
        if (playerController != null)
        {
            playerController.transform.position = playerPosition;
            playerController.ResetMovementState();
        }

        yield return new WaitForSeconds(0.3f);

        // 5. Restore screen fade and mobile controls
        if (UIManager.instance != null)
        {
            UIManager.instance.fadeToBlack = false;
            UIManager.instance.fadeFromBlack = true;
            UIManager.instance.EnableMobileControls();
        }

        // 6. Restore normal player movement and input
        if (playerController != null)
        {
            playerController.SetMovementLocked(false);
        }

        // 7. Safety invulnerability buffer so immediate contact doesn't multi-kill
        yield return new WaitForSeconds(0.5f);
        isRespawning = false;
    }

    public void SetCheckpoint(Vector3 newCheckpointPosition)
    {
        playerPosition = newCheckpointPosition;
    }

    public void FindTotalPickups()
    {
        pickup[] pickups = Object.FindObjectsByType<pickup>(FindObjectsSortMode.None);
        foreach (pickup pickupObject in pickups)
        {
            if (pickupObject.pt == pickup.pickupType.coin)
            {
                totalCoins += 1;
            }
        }
    }

    public int GetCurrentLevelNumber()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        if (buildIndex >= 1 && buildIndex <= LevelManager.TotalLevels)
        {
            return buildIndex;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Level") return 1;
        if (sceneName.StartsWith("Level") && int.TryParse(sceneName.Substring(5), out int lvl))
        {
            return lvl;
        }

        return 1;
    }

    public void LevelComplete()
    {
        int currentLevel = GetCurrentLevelNumber();

        // Save progress and unlock next level
        LevelManager.MarkLevelCompleted(currentLevel, coinCount);

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            if (currentLevel >= LevelManager.TotalLevels)
            {
                if (leveCompletePanelTitle != null)
                {
                    leveCompletePanelTitle.text = "CONGRATULATIONS!\nALL LEVELS COMPLETED!";
                }

                if (nextLevelButton != null)
                {
                    nextLevelButton.SetActive(false);
                }
            }
            else
            {
                if (leveCompletePanelTitle != null)
                {
                    leveCompletePanelTitle.text = "LEVEL COMPLETE";
                }

                if (nextLevelButton != null)
                {
                    nextLevelButton.SetActive(true);
                }
            }

            if (levelCompleteCoins != null)
            {
                levelCompleteCoins.text = "COINS COLLECTED: " + coinCount.ToString() + " / " + totalCoins.ToString();
            }
        }

        // Fade back from black so UI is clearly visible
        if (UIManager.instance != null)
        {
            UIManager.instance.DisableMobileControls();
            UIManager.instance.fadeToBlack = false;
            UIManager.instance.fadeFromBlack = true;
        }
    }

    public void LoadNextLevel()
    {
        int currentLevel = GetCurrentLevelNumber();
        if (currentLevel < LevelManager.TotalLevels)
        {
            LevelManager.LoadLevel(currentLevel + 1);
        }
        else
        {
            LoadLevelSelect();
        }
    }

    public void LoadLevelSelect()
    {
        LevelManager.OpenLevelSelectOnMenuLoad = true;
        SceneManager.LoadScene(0);
    }

    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
