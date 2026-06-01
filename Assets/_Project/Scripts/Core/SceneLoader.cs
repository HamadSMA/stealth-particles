using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void ReloadCurrent()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void LoadByIndex(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            return;
        }

        SceneManager.LoadScene(buildIndex);
    }

    public static void LoadByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static bool HasNext()
    {
        return SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;
    }

    public static void LoadNext()
    {
        if (HasNext())
        {
            LoadByIndex(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
