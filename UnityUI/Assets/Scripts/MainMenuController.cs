using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Name deiner Haupt-Spiel-Szene (im Build Settings eintragen!)
    private const string GameSceneName = "GameScene";

    public void StartPassAndPlay()
    {
        GameSettings.Mode = GameMode.PassAndPlay;
        SceneManager.LoadScene(GameSceneName);
    }

    public void StartVsComputer()
    {
        GameSettings.Mode = GameMode.VsComputer;
        SceneManager.LoadScene(GameSceneName);
    }

    // Optional: Verbinde das mit einem UI-Slider (OnValueChanged, dynamischer float)
    public void SetDifficulty(float depth)
    {
        GameSettings.SearchDepth = (int)depth;
    }
}