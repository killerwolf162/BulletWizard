using UnityEngine;

[CreateAssetMenu(fileName = "DefaultEnemyData", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    //all enemy data stored for easy enemy modification
    public string enemyName;
    public float moveSpeed;
    public int damage;
    public int health;
}
