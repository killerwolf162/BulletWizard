using UnityEngine;

[CreateAssetMenu(fileName = "WallGenerationParameters", menuName = "PCG/WallGenerationParameters")]
public class WallGenerationParameters : ScriptableObject
{
    [Header("Wall variant settings")]
    [Range(0f, 1f)] public float variantPercent = 0.20f;  // exact quota
    [Range(0f, 1f)] public float damagedPercent = 0.35f;  // Perlin threshold (approx.)

    [Header("Noise")]
    [Min(0.001f)] public float damagedNoiseScale = 0.08f; // lower = bigger patches
    public bool randomizePerlinOffsetEachRun = true;
    public Vector2 perlinOffset = new Vector2(123.4f, 567.8f); // used if randomizePerlinOffsetEachRun = false

    [Header("Rules")]
    public bool exclusive = false; // if true, Variant wins and removes Damaged overlap
    [Tooltip("Minimum Chebyshev distance between VARIANT walls")]
    [Min(2)] public int variantMinChebyshevDistance = 4;


    private void OnValidate()
    {
        variantPercent = Mathf.Clamp01(variantPercent);
        damagedPercent = Mathf.Clamp01(damagedPercent);
        damagedNoiseScale = Mathf.Max(0.0001f, damagedNoiseScale);
        variantMinChebyshevDistance = Mathf.Max(1, variantMinChebyshevDistance);
    }
}
