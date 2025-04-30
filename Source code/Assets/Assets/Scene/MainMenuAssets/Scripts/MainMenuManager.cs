using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    public Animator animator;

    public void StartGame()
    {
       animator.SetBool("Start", true);
      
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void AStarSceneOpen()
    {
        SceneManager.LoadScene("AStarGame");
    }
    public void UserSceneOpen()
    {
        SceneManager.LoadScene("UserMode");
    }
}
 