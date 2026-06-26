using UnityEngine;

[CreateAssetMenu(menuName = "PCG/LevelScalingConfig")]
public class LevelScalingConfig : ScriptableObject
{
    public int BaseEnemyCount;
    public float EnemyCountMultiplyerPerLevel;
    public AnimationCurve EnemyHealthScaling;
    public AnimationCurve RoomSizeScaling;
    public AnimationCurve DungeonSizeScaling;
}
