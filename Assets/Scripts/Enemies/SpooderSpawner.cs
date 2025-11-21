using System.Collections.Generic;
using UnityEngine;
public class SpooderSpawner : AbstractSpawner
{

    private GameHandler _instance;

    public virtual GameObject gameobject { get; private set; }

    private GameObject[] _prefabs;
    private EnemyData _enemyData;

    private Transform _spawnPos;
    private Transform _playerPos;
    private int _maxSpooders = 2;

    public override IReadOnlyList<Vector3> PatrolPoints => _patrolPoints;
    private List<Vector3> _patrolPoints;
    public SpooderSpawner(GameObject spawner, GameObject[] prefabs, Transform spawnPos, Transform playerPos, EnemyData data, List<Vector3> patrolPoints)
    {
        this.gameobject = spawner;
        _prefabs = prefabs;
        _spawnPos = spawnPos;
        _playerPos = playerPos;
        _enemyData = data;
        _patrolPoints = patrolPoints;

        Start();
    }

    public override void Start()
    {
        _instance = GameHandler.instance;
        _instance.Subscribe(this);
    }

    public override void Update()
    {
        if (Time.frameCount % 120 == 0) // spawns enemy every 2 seconds
            PrepareToSpawn();
    }

    public override void PrepareToSpawn()
    {
        if(OwnedEnemies.Count < _maxSpooders)
        {
            int type = 0;
            var RNG = Random.Range(0, 100);

            if (RNG < 50)
                type = (int)ElementalTypes.Normal;            
            else if (RNG > 50 && RNG < 75)
                type = (int)ElementalTypes.Fire;
            else if (RNG > 75)
                type = (int)ElementalTypes.Ice;

            Spawn(type);
        }       
    }

    public override void Spawn(int type)
    {
        GameObject newEnemyGo = GameHandler.instance.InstantiateNew(_prefabs[type]);
        EnemyBehaviour enemy = new EnemyBehaviour((ElementalTypes)type, this, newEnemyGo, _playerPos, _enemyData, _spawnPos.position);
        RegisterEnemy(enemy);
    }
} 
