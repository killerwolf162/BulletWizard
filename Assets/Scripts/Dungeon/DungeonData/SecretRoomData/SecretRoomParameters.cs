using UnityEngine;

[CreateAssetMenu(fileName = "SecretRoomParameters", menuName = "PCG/SecretRoomParameters")]
public class SecretRoomParameters : ScriptableObject
{
    [Header("Secret Rooms")]
    public bool enableSecretRooms = false;
    public bool keepEntryWallSealed = true;

    [Range(0, 1f)] public float chancePerDmgedWall = 0.05f;
    [Min(1)] public int maxCount = 1;
    [Min(2)] public int minWidth = 4;
    [Min(2)] public int minHeight = 4;
    [Min(2)] public int maxWidth = 8;
    [Min(2)] public int maxHeight = 8;
    [Tooltip("Free tiles around room")]
    [Min(1)] public int padding = 1;

    [Header("Secret Rooms - Corridor")]
    [Min(1)] public int corridorMinLength = 2;
    [Min(1)] public int corridorMaxLength = 5;
    [Min(1)] public int corridorWidth = 1;

    private void OnValidate()
    {
        minWidth = Mathf.Max(2, minWidth);
        minHeight = Mathf.Max(2, minHeight);
        maxWidth = Mathf.Max(minWidth, maxWidth);
        maxHeight = Mathf.Max(minHeight, maxHeight);
        maxCount = Mathf.Max(0, maxCount);

        corridorMinLength = Mathf.Max(1, corridorMinLength);
        corridorMaxLength = Mathf.Max(corridorMinLength, corridorMaxLength);
        corridorWidth = Mathf.Max(1, corridorWidth);
    } 
}