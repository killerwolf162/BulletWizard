using UnityEngine;

[CreateAssetMenu(fileName = "DungeonRoomParameters", menuName = "PCG/DungeonData")]
public class DungeonData : ScriptableObject
{
    [SerializeField] public int _minRoomWidth;
    [SerializeField] public int _minRoomHeight;
    [SerializeField] public int _minRoomCount;

    [SerializeField] public int _dungeonWidth;
    [SerializeField] public int _dungeonHeight;

    [SerializeField] public bool _randomWalkRooms = false;
    [Range(0, 10)] [SerializeField] public int _offset = 1;

    [Range(0.0f, 1f)] [SerializeField] public float _mediumCorridorPercent = 0.25f; // ~width 2
    [Range(0.0f, 1f)] [SerializeField] public float _largeCorridorPercent = 0.50f;  // ~width 3
}
