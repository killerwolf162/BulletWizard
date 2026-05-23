using System.Collections.Generic;
using UnityEngine;
public class SpooderSpawner : AbstractSpawner
{

    private GameHandler _instance;

    public virtual GameObject gameobject { get; private set; }

    private GameObject[] _prefabs;
    private List<EnemyData> _enemyData = new List<EnemyData>();

    private Transform _spawnPos;
    private Transform _playerPos;
    private int _maxSpooders = 2;
    private bool _alwaysChase;

    public override IReadOnlyList<Vector3> PatrolPoints => _patrolPoints;
    private List<Vector3> _patrolPoints;
    public SpooderSpawner(GameObject spawner, GameObject[] prefabs, Transform spawnPos, Transform playerPos, List<EnemyData> data, List<Vector3> patrolPoints, bool alwaysChase)
    {
        this.gameobject = spawner;
        _prefabs = prefabs;
        _spawnPos = spawnPos;
        _playerPos = playerPos;
        _enemyData.AddRange(data);
        _patrolPoints = patrolPoints;
        _alwaysChase = alwaysChase;

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
        EnemyData enemyData;
        if (type == (int)ElementalTypes.Normal)
            enemyData = _enemyData[0];
        else
            enemyData = _enemyData[1];

        EnemyBehaviour enemy = new EnemyBehaviour((ElementalTypes)type, this, newEnemyGo, _playerPos, enemyData, _spawnPos.position, _alwaysChase);
        RegisterEnemy(enemy);
    }
} 
