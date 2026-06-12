using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartApp()

    {
        SceneManager.LoadScene("MotorScene1");
    }

    public void BackMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToSelection()
    {
        SceneManager.LoadScene("MainMenuSelection");
    }

    public void GoToTutorial()
    {
        SceneManager.LoadScene("MainMenuTutorial");
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}
