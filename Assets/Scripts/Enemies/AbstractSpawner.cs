using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSpawner : ISceneObject
{
    public virtual GameObject _gameObject { get; }

    protected readonly List<EnemyBehaviour> _ownedEnemies = new List<EnemyBehaviour>();
    public IReadOnlyList<EnemyBehaviour> OwnedEnemies => _ownedEnemies;

    public virtual IReadOnlyList<Vector3> PatrolPoints => null;

    public void RegisterEnemy(EnemyBehaviour enemy)
    {
        if (enemy == null) return;
        if (!_ownedEnemies.Contains(enemy))
            _ownedEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyBehaviour enemy)
    {
        if (enemy == null) return;
        _ownedEnemies.Remove(enemy);
    }

    public abstract void Start();
    public abstract void Update();
    public abstract void PrepareToSpawn();
    public abstract void Spawn(int type);
}
