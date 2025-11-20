using System.Collections.Generic;
using UnityEngine;
public class EnemySpawner : ISceneObject
{
    public GameObject gameobject { get; private set; }
    public List<EnemyBehaviour> ownedEnemies = new List<EnemyBehaviour>();

    private GameObject _enemyPrefab;
    private Transform _spawnPos;
    private EnemyData _data;
    private Transform _playerPos;
    private int maxEnemies = 1;


    //constructor
    public EnemySpawner(GameObject spawner, GameObject enemyPrefab, Transform spawnPos, EnemyData data, Transform playerPos)
    {
        this.gameobject = gameobject;
        _spawnPos = spawnPos;
        _enemyPrefab = enemyPrefab;
        _data = data;
        _playerPos = playerPos;
        Start();
    }

    public virtual void Start()
    {
        GameHandler.instance.Subscribe(this);
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
        if(ownedEnemies.Count < maxEnemies)
        {
            GameObject newEnemyGo = GameHandler.Instantiate(_enemyPrefab);
            var _pos = _spawnPos.position;
            EnemyBehaviour enemy = new EnemyBehaviour(this, newEnemyGo, _playerPos, _data, _spawnPos.position);
            ownedEnemies.Add(enemy);
        }       
    }
} 
