using System.Collections.Generic;
using UnityEngine;
public class EnemySpawner : ISceneObject
{
    public GameObject gameobject { get; private set; }
    public List<EnemyBehaviour> ownedEnemies = new List<EnemyBehaviour>();

    private Transform _spawnPos;
    private Transform _playerPos;
    private int _maxEnemies = 1;

    private GameHandler _instance;

    //constructor
    public EnemySpawner(GameObject spawner, Transform spawnPos, Transform playerPos)
    {
        this.gameobject = spawner;
        _spawnPos = spawnPos;
        _playerPos = playerPos;
        Start();
    }

    public virtual void Start()
    {
        _instance = GameHandler.instance;
        _instance.Subscribe(this);
    }

    public virtual void Update()
    {
        //every so many seconds spawn an enemy
        if (Time.frameCount % 120 == 0) // spawns enemy every 2 seconds
            SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        // Instantiate the enemy from a prefab
        if(ownedEnemies.Count < _maxEnemies)
        {
            int idx = 0;
            int data = 0;
            int type = 0;
            var RNG = Random.Range(0, 100);

            if (RNG < 50)
            {
                idx = 0; 
                type = (int)ElementalTypes.Normal;
            }               
            else if (RNG > 50 && RNG < 75)
            {
                idx = 1;
                type = (int)ElementalTypes.Fire;
            }
            else if (RNG > 75)
            {
                idx = 2;
                type = (int)ElementalTypes.Ice;
            }

            GameObject newEnemyGo = GameHandler.Instantiate(_instance._enemyPrefabs[idx]);
            var _pos = _spawnPos.position;
            EnemyBehaviour enemy = new EnemyBehaviour((ElementalTypes)type, this, newEnemyGo, _playerPos, _instance._enemyDatas[data], _spawnPos.position);
            ownedEnemies.Add(enemy);
        }       
    }
} 
