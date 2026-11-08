using UnityEngine;
using UnityEngine.SceneManagement;

public class Events : MonoBehaviour
{
    public void Menu()
    {
        SceneManager.LoadScene(0);
    }

    public void Level()
    {
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void NextLevel()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadNextLevel();
        }
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex <= LevelManager.TotalLevels)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void LoadNextLevel()
    {
        NextLevel();
    }

    public void LevelSelect()
    {
        LevelManager.OpenLevelSelectOnMenuLoad = true;
        SceneManager.LoadScene(0);
    }

    public void LoadLevelSelect()
    {
        LevelSelect();
    }

    public void MainMenu()
    {
        Menu();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadLevel(int levelNumber)
    {
        LevelManager.LoadLevel(levelNumber);
    }
}
