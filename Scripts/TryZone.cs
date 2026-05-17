using UnityEngine;

public class TryZone : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("TRUE = Player team scores here (AI end zone)")]
    public bool isPlayerScoreZone = true;

    void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.IsPlaying()) return;

        BallController ball = FindAnyObjectByType<BallController>();
        if (ball == null || !ball.isPossessed) return;

        if (isPlayerScoreZone)
        {
            // Player or teammate enters with ball = SCORE
            if (other.CompareTag("Player"))
            {
                if (ball.currentHolder == other.transform)
                {
                    Debug.Log("✅ PLAYER TEAM SCORED!");
                    GameManager.Instance.AddScore(true);
                }
            }
        }
        else
        {
            // AI enters with ball = AI SCORES
            if (other.CompareTag("AIOpponent"))
            {
                if (ball.currentHolder == other.transform)
                {
                    Debug.Log("✅ AI TEAM SCORED!");
                    GameManager.Instance.AddScore(false);
                }
            }
        }
    }
}