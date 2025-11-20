using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GameHandler : MonoBehaviour
{
    public static GameHandler instance;

    [Header("DungeonGeneration")]
    [SerializeField] private Tilemap floorMap;
    [SerializeField] private Tilemap wallMap;
    [SerializeField] private SimpleRandomWalkData walkData;
    [SerializeField] private WallGenerationParameters wallParameters;
    [SerializeField] private SecretRoomParameters roomParameters;
    private TilemapVisualizer _tilemapVisualizer;
    private RoomFirstDungeonGenerator generator;

    public Camera cam;
    public GameObject _playerPrefab;
    public EnemyData[] _enemyDatas;
    public GameObject[] _enemyPrefabs;
    [SerializeField] private GameObject[] _enemySpawners;

    private ISceneObject _player;

    private List<IUpdateable> _updateables = new List<IUpdateable>();

    private void Start()
    {
        Application.targetFrameRate = 60;
        instance = this;
        _tilemapVisualizer = new TilemapVisualizer(floorMap, wallMap);
        generator = new RoomFirstDungeonGenerator(_tilemapVisualizer, walkData, wallParameters, roomParameters);
        generator.GenerateDungeon();

        cam = FindAnyObjectByType<Camera>();
        _player = new PlayerController(Instantiate(_playerPrefab));
        SpawnSpawns();
    }

    private void Update()
    {
        for (int i = 0; i < _updateables.Count; i++)
            _updateables[i].Update();

    }

    public void Subscribe(IUpdateable updateable)
    {
        if (!_updateables.Contains(updateable))
            _updateables.Add(updateable);
    }

    public void UnSubscribe(IUpdateable updateable)
    {
        if (_updateables.Contains(updateable))
        {
            _updateables.Remove(updateable);
        }
    }

    public GameObject InstantiateNew(GameObject gameObject)
    {
        return Instantiate(gameObject);
    }

    public void DestroyObject(GameObject objectToDestroy)
    {
        Destroy(objectToDestroy);
    }

    public void TimedDestroyObject(GameObject objectToDestroy, float time)
    {
        Destroy(objectToDestroy, time);
    }

    public void SpawnSpawns()
    {
        foreach (GameObject spawner in _enemySpawners)
        {
            var spawnerGO = new EnemySpawner(spawner, spawner.transform, _player.gameobject.transform);
        }
    }
}
