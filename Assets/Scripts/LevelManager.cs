using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public const int TotalLevels = 10;
    private const string UNLOCKED_LEVEL_KEY = "UnlockedLevel";
    private const string LEVEL_COMPLETED_KEY_PREFIX = "LevelCompleted_";
    private const string LEVEL_COINS_KEY_PREFIX = "LevelCoins_";

    public static bool OpenLevelSelectOnMenuLoad = false;

    public static int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
    }

    public static bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1) return true;
        return levelNumber <= GetUnlockedLevel();
    }

    public static void UnlockLevel(int levelNumber)
    {
        if (levelNumber > TotalLevels) return;

        int currentUnlocked = GetUnlockedLevel();
        if (levelNumber > currentUnlocked)
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, levelNumber);
            PlayerPrefs.Save();
            Debug.Log($"Level {levelNumber} unlocked!");
        }
    }

    public static bool IsLevelCompleted(int levelNumber)
    {
        return PlayerPrefs.GetInt(LEVEL_COMPLETED_KEY_PREFIX + levelNumber, 0) == 1;
    }

    public static int GetBestCoins(int levelNumber)
    {
        return PlayerPrefs.GetInt(LEVEL_COINS_KEY_PREFIX + levelNumber, 0);
    }

    public static void MarkLevelCompleted(int levelNumber, int coinsCollected = 0)
    {
        PlayerPrefs.SetInt(LEVEL_COMPLETED_KEY_PREFIX + levelNumber, 1);

        int bestCoins = GetBestCoins(levelNumber);
        if (coinsCollected > bestCoins)
        {
            PlayerPrefs.SetInt(LEVEL_COINS_KEY_PREFIX + levelNumber, coinsCollected);
        }

        if (levelNumber < TotalLevels)
        {
            UnlockLevel(levelNumber + 1);
        }

        PlayerPrefs.Save();
        Debug.Log($"Level {levelNumber} completed with {coinsCollected} coins!");
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UNLOCKED_LEVEL_KEY);
        for (int i = 1; i <= TotalLevels; i++)
        {
            PlayerPrefs.DeleteKey(LEVEL_COMPLETED_KEY_PREFIX + i);
            PlayerPrefs.DeleteKey(LEVEL_COINS_KEY_PREFIX + i);
        }
        PlayerPrefs.Save();
        Debug.Log("Progress reset: Level 1 unlocked, other levels locked.");
    }

    public static string GetSceneNameForLevel(int levelNumber)
    {
        if (levelNumber == 1) return "Level";
        return "Level" + levelNumber;
    }

    public static void LoadLevel(int levelNumber)
    {
        if (IsLevelUnlocked(levelNumber))
        {
            string sceneName = GetSceneNameForLevel(levelNumber);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"Level {levelNumber} is locked!");
        }
    }
}

