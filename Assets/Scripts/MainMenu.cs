using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameScene = "Game";

    void Start() { Time.timeScale = 1f; }

    public void Play()
    {
        AudioManager.Play(Sfx.Click);
        SceneManager.LoadScene(gameScene);
    }

    public void Quit()
    {
        AudioManager.Play(Sfx.Click);
        Application.Quit();
    }

    public void ResetSave()
    {
        AudioManager.Play(Sfx.Click);
        GameManager.ResetSave();
    }
}
