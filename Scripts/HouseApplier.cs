// ============================================================
// HouseApplier.cs
// Attach to: Player AND each Teammate GameObject
// Reads HouseManager and applies correct team suit material
// ============================================================
using UnityEngine;

public class HouseApplier : MonoBehaviour
{
    [Header("Team Materials (drag from Assets/TeamSuits/Materials)")]
    public Material rubyMaterial;
    public Material sapphireMaterial;

    [Header("Renderer")]
    [Tooltip("Leave null to auto-detect SkinnedMeshRenderer")]
    public Renderer targetRenderer;

    [Tooltip("Which material slot to replace (usually 0)")]
    public int materialIndex = 0;

    // ─────────────────────────────────────────────────────────
    void Start() => ApplyTeamMaterial();

    // ─────────────────────────────────────────────────────────
    public void ApplyTeamMaterial()
    {
        if (HouseManager.Instance == null || HouseManager.Instance.selectedHouse == null)
        {
            Debug.LogWarning("[HouseApplier] HouseManager not found.");
            return;
        }

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();
        if (targetRenderer == null)
        {
            Debug.LogError("[HouseApplier] No Renderer found on " + gameObject.name);
            return;
        }

        Material chosen = HouseManager.Instance.selectedHouse.teamIndex == 0
            ? rubyMaterial
            : sapphireMaterial;

        if (chosen == null)
        {
            Debug.LogWarning("[HouseApplier] Target material not assigned.");
            return;
        }

        // Instance the material so we don't modify the shared asset
        Material[] mats = targetRenderer.materials;
        if (materialIndex < mats.Length)
        {
            mats[materialIndex] = new Material(chosen);
            targetRenderer.materials = mats;
        }

        Debug.Log($"[HouseApplier] {gameObject.name} → {HouseManager.Instance.selectedHouse.houseName} suit");
    }
}
