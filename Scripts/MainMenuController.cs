using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Повісь на Canvas головного меню. Кнопки викликають ці методи через onClick
/// (це вже підключено автоматично інструментом створення меню).
/// </summary>
public class MainMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public string gameSceneName = "SampleScene";

    public void OnPlay()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnOpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnCloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OnQuit()
    {
        Debug.Log("Вихід із гри (в редакторі це просто повідомлення - працює по-справжньому лише в зібраній грі).");
        Application.Quit();
    }
}
