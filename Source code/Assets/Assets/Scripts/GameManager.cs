using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Tilemap groundTilemap;
    public bool loss = false;

    [Header("UI Elements")]
    public TextMeshProUGUI rescuedCounterText; 

    private int rescuedHostages = 0;
    public GameObject LooseScreen;
    public Image LooseScreenImage;

    private void Awake()
    {
        
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Initialize UI
        if (rescuedCounterText != null)
            rescuedCounterText.text = "Rescued: 0";
    }

    public void HostageRescued()
    {
        rescuedHostages++;
        if (rescuedCounterText != null)
            rescuedCounterText.text = "Rescued: " + rescuedHostages;

        Debug.Log("Rescued Hostages: " + rescuedHostages);
    }
    public void mainmenu()
    {
        SceneManager.LoadScene("MainMenu");
        
    }
}
