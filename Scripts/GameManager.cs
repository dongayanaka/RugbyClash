using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, GameOver }
    public static GameManager Instance { get; private set; }

    [Header("Match Settings")]
    public float matchDuration = 300f; // 5 minutes

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("References")]
    public PlayerController playerController;
    public BallController ballController;

    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform ballSpawnPoint;

    [Header("All AI to disable on game over")]
    public EnemyAI[] allEnemies;
    public TeammateAI[] allTeammates;

    private int playerScore = 0;
    private int aiScore = 0;
    private float timeRemaining;
    private GameState currentState = GameState.Playing;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Read duration from PlayerPrefs (set by MainMenu)
        if (PlayerPrefs.HasKey("MatchDuration"))
            matchDuration = PlayerPrefs.GetFloat("MatchDuration");

        timeRemaining = matchDuration;
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        // Auto-find if not assigned
        if (playerController == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
                playerController = p.GetComponent<PlayerController>();
        }
        if (ballController == null)
            ballController = FindAnyObjectByType<BallController>();

        // Auto-find all enemies and teammates
        if (allEnemies == null || allEnemies.Length == 0)
            allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        if (allTeammates == null || allTeammates.Length == 0)
            allTeammates = FindObjectsByType<TeammateAI>(FindObjectsSortMode.None);

        UpdateScoreUI();
        UpdateTimerUI();

        Debug.Log($"Match started! Duration: {matchDuration}s " +
            $"Enemies: {allEnemies.Length} Teammates: {allTeammates.Length}");
    }

    void Update()
    {
        if (currentState != GameState.Playing) return;

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Max(0f, timeRemaining);
        UpdateTimerUI();

        if (timeRemaining <= 0f)
            EndMatch();
    }

    public void AddScore(bool isPlayerTeam)
    {
        if (currentState != GameState.Playing) return;

        if (isPlayerTeam)
        {
            playerScore += 5;
            Debug.Log($"🏉 Player team scores! Total: {playerScore}");
        }
        else
        {
            aiScore += 5;
            Debug.Log($"🏉 AI team scores! Total: {aiScore}");
        }

        UpdateScoreUI();
        Invoke(nameof(ResetPositions), 2f);
    }

    void ResetPositions()
    {
        if (currentState != GameState.Playing) return;

        // Reset player
        if (playerController != null && playerSpawnPoint != null)
        {
            playerController.transform.position = playerSpawnPoint.position;
            playerController.transform.rotation = playerSpawnPoint.rotation;
            Rigidbody rb = playerController.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // Reset ball to center
        if (ballController != null && ballSpawnPoint != null)
            ballController.ResetToPosition(ballSpawnPoint.position);

        Debug.Log("Positions reset after try!");
    }

    void EndMatch()
    {
        currentState = GameState.GameOver;

        // Freeze all AI
        foreach (EnemyAI e in allEnemies)
            if (e != null) e.DisableAI();
        foreach (TeammateAI t in allTeammates)
            if (t != null) t.DisableAI();

        // Freeze player
        if (playerController != null)
            playerController.DisableMovement();

        // Show result
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        string houseName = "YOUR TEAM";
        if (HouseManager.Instance?.selectedHouse != null)
            houseName = HouseManager.Instance.selectedHouse.houseName
                .ToUpper();

        string result;
        if (playerScore > aiScore)
            result = $"🏆 {houseName} WINS!";
        else if (aiScore > playerScore)
            result = "🏆 ENEMY WINS!";
        else
            result = "🤝 IT'S A DRAW!";

        if (gameOverText != null)
            gameOverText.text =
                $"FINAL SCORE\n\n" +
                $"{houseName}: {playerScore}\n" +
                $"ENEMY: {aiScore}\n\n" +
                $"{result}";

        Debug.Log($"Match Over! Player:{playerScore} AI:{aiScore}");
    }

    void UpdateScoreUI()
    {
        if (scoreText == null) return;
        string name = HouseManager.Instance?.selectedHouse?.houseName
            ?? "TEAM";
        scoreText.text = $"{name.ToUpper()}: {playerScore}  |  " +
            $"ENEMY: {aiScore}";
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        int m = Mathf.FloorToInt(timeRemaining / 60f);
        int s = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{m:00}:{s:00}";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public bool IsPlaying() => currentState == GameState.Playing;
}