// ============================================================
// HouseManager.cs
// DontDestroyOnLoad singleton — carries team + match settings
// ============================================================
using UnityEngine;

public class HouseManager : MonoBehaviour
{
    public static HouseManager Instance { get; private set; }

    [System.Serializable]
    public class HouseData
    {
        public string houseName;
        public Color primaryColor;
        public Color secondaryColor;
        public int teamIndex; // 0 = Ruby, 1 = Sapphire
    }

    public HouseData selectedHouse { get; private set; }
    public float matchDuration { get; private set; } = 300f; // default 5 min

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SelectHouse("Ruby");
    }

    // ── Team selection ────────────────────────────────────────
    public void SelectHouse(string houseName)
    {
        selectedHouse = new HouseData { houseName = houseName };

        switch (houseName)
        {
            case "Ruby":
                selectedHouse.primaryColor = new Color(0.72f, 0.07f, 0.07f);
                selectedHouse.secondaryColor = new Color(0.95f, 0.40f, 0.40f);
                selectedHouse.teamIndex = 0;
                break;
            case "Sapphire":
                selectedHouse.primaryColor = new Color(0.08f, 0.22f, 0.80f);
                selectedHouse.secondaryColor = new Color(0.40f, 0.62f, 1.00f);
                selectedHouse.teamIndex = 1;
                break;
            default:
                selectedHouse.primaryColor = Color.white;
                selectedHouse.secondaryColor = Color.grey;
                selectedHouse.teamIndex = 0;
                break;
        }
        Debug.Log($"[HouseManager] Team selected: {houseName}");
    }

    // ── Match duration ────────────────────────────────────────
    public void SetMatchDuration(float seconds)
    {
        matchDuration = seconds;
        Debug.Log($"[HouseManager] Match duration: {seconds / 60f} min");
    }
}
