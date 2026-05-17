using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button exitButton;

    [Header("Scenes")]
    public string introSceneName = "IntroScene";

    [Header("Optional")]
    public TextMeshProUGUI titleText;

    void Start()
    {
        // Ensure HouseManager exists
        if (HouseManager.Instance == null)
        {
            GameObject hm = new GameObject("HouseManager");
            hm.AddComponent<HouseManager>();
        }

        // Default to Ruby house
        HouseManager.Instance.SelectHouse("Ruby");

        // Wire buttons
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogError("Start Button is not assigned!");
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(QuitGame);
        }
        else
        {
            Debug.LogError("Exit Button is not assigned!");
        }

        // Set default match duration
        PlayerPrefs.SetFloat("MatchDuration", 300f);
        PlayerPrefs.Save();

        Debug.Log("Main Menu loaded!");
    }

    public void StartGame()
    {
        Debug.Log("Starting intro video...");
        SceneManager.LoadScene(introSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}