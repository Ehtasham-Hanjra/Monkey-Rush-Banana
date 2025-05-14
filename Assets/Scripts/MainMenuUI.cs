using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnStartButton()
    {
        // Reset both game and level state before loading
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameState();
        }
        SceneManager.LoadScene("Game");
    }
}
