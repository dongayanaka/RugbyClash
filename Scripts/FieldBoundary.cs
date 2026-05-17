// ============================================================
// FieldBoundary.cs  — STATIC UTILITY (no GameObject needed)
// Drop into Assets/Scripts — no scene setup required
// Adjust the four bounds constants to match your stadium size
// ============================================================
using UnityEngine;

public static class FieldBoundary
{
    // ── Edit these to match your actual pitch dimensions ───────
    public static float MinX = -55f;
    public static float MaxX = 55f;
    public static float MinZ = -35f;
    public static float MaxZ = 35f;

    // ── Clamp a world position inside the pitch ────────────────
    public static Vector3 Clamp(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, MinX, MaxX);
        pos.z = Mathf.Clamp(pos.z, MinZ, MaxZ);
        return pos;
    }

    public static bool IsOutOfBounds(Vector3 pos)
        => pos.x < MinX || pos.x > MaxX || pos.z < MinZ || pos.z > MaxZ;
}
