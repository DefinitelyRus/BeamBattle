using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreen : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Game"); 
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game..."); 
        Application.Quit(); 
    }
}
