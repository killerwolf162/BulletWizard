using UnityEngine;

[CreateAssetMenu(fileName = "DungeonRoomParameters", menuName = "PCG/DungeonData")]
public class DungeonData : ScriptableObject
{
    [SerializeField] public int minRoomWidth;
    [SerializeField] public int minRoomHeight;
    [SerializeField] public int minRoomCount;
    [SerializeField] public int maxRoomCount;
    [SerializeField] public int maxRoomHeight;
    [SerializeField] public int maxRoomWidth;

    [SerializeField] public int dungeonWidth;
    [SerializeField] public int dungeonHeight;

    [SerializeField] public bool randomWalkRooms = false;
    [Range(0, 10)] [SerializeField] public int offset = 1;

    [Range(0.0f, 1f)] [SerializeField] public float mediumCorridorPercent = 0.25f; // ~width 2
    [Range(0.0f, 1f)] [SerializeField] public float largeCorridorPercent = 0.50f;  // ~width 3
}
