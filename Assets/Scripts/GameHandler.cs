using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.Events;

public class GameHandler : MonoBehaviour
{
    public static GameHandler instance;
    public event Action OnPlayerDied;
    public event Action<int> ScoreChanged;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _floorMap;
    [SerializeField] private Tilemap _wallMap;
    public Tilemap FloorMap => _floorMap;
    public Tilemap WallMap => _wallMap;
    public AStarPathfinder Pathfinder { get; private set; }

    [Header("DungeonGeneration")]
    [SerializeField] private SimpleRandomWalkData _walkData;
    [SerializeField] private WallGenerationParameters _wallParameters;
    [SerializeField] private SecretRoomParameters _secretRoomParameters;
    [SerializeField] private DungeonData _dungeonParameters;

    private TilemapVisualizer _tilemapVisualizer;
    private RoomFirstDungeonGenerator _generator;

    [Header("Spawning")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _enemySpawnerPrefab;
    [SerializeField] private int _maxSpawners;
    [SerializeField] private int _maxAliveAtSameTime;
    [SerializeField] private int _entitiesToSpawn;
    private GameObject[] _enemySpawners;
    private List<Vector3>[] _spawnerPatrolPoints;

    [Header("Player Data")]
    [SerializeField] private List<ElementData> _elementDatas = new List<ElementData>();

    [Header("EnemyGos & Data")]
    [SerializeField] private bool _alwaysChase = false; // for wave based shooter?
    [SerializeField] private List<EnemyData> _spooderData;
    [SerializeField] private GameObject[] _spooderGos;

    [Header("UI")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _manaSlider;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private Image _bulletCooldownOverlay;
    [SerializeField] private Image _fireBallCooldownOverlay;

    public Camera cam;

    private PlayerController _player;
    private List<IUpdateable> _updateables = new List<IUpdateable>();
    private PlayerHUD _hud;
    private int _score;

    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; // disable vsync so targetFrameRate takes effect
    }

    private void Start()
    {
        instance = this;
        _tilemapVisualizer = new TilemapVisualizer(_floorMap, _wallMap);
        _generator = new RoomFirstDungeonGenerator(_tilemapVisualizer, _walkData, _dungeonParameters, _wallParameters, _secretRoomParameters);
        _generator.GenerateDungeon();

        Pathfinder = new AStarPathfinder(_floorMap, _wallMap);

        cam = FindAnyObjectByType<Camera>();
        SpawnPlayerAndSpawners();
    }

    private void Update()
    {
        for (int i = 0; i < _updateables.Count; i++)
            _updateables[i].Update();

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
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

    public void ModifyScore(int amount)
    {
        _score += amount;
        ScoreChanged?.Invoke(_score);
    }

    private void SpawnPlayerAndSpawners()
    {
        var roomCenters = _generator.RoomCenters;
        var roomFloors = _generator.RoomFloors;

        if (roomCenters == null || roomCenters.Count == 0)
        {
            Debug.Log("Dungeon has no rooms");
            return;
        }

        // Spawn player in middle room

        Vector2 avg = Vector2.zero;
        foreach (var c in roomCenters)
            avg += c;
        avg /= roomCenters.Count;

        int playerRoomIdx = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < roomCenters.Count; i++)
        {
            float d = (roomCenters[i] - avg).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                playerRoomIdx = i;
            }
        }
        Vector2Int playerCell = GetAnyFloorInRoom(roomFloors[playerRoomIdx], roomCenters[playerRoomIdx]);
        Vector3 playerWorld = FloorPosToWorld(playerCell);
        GameObject playerGo = Instantiate(_playerPrefab, playerWorld, Quaternion.identity);
        _player = new PlayerController(playerGo, _elementDatas);

        // Spawn spawners

        // Collect all non-player rooms
        List<int> roomIdcs = new List<int>();
        for (int i = 0; i < roomCenters.Count; i++)
        {
            if (i == playerRoomIdx)
                continue;
            roomIdcs.Add(i);
        }

        _enemySpawners = new GameObject[roomIdcs.Count];
        _spawnerPatrolPoints = new List<Vector3>[roomIdcs.Count];

        // prep spawnerGOs
        for (int r = 0; r < roomIdcs.Count; r++)
        {
            int roomIdx = roomIdcs[r];
            var roomFloorCells = roomFloors[roomIdx];

            List<Vector3> patrolPoints = new List<Vector3>();
            if (roomFloorCells != null)
            {
                for (int i = 0; i < roomFloorCells.Count; i++)
                {
                    patrolPoints.Add(FloorPosToWorld(roomFloorCells[i]));
                }
            }

            Vector2Int spawnerCell = GetAnyFloorInRoom(roomFloors[roomIdx], roomCenters[roomIdx]);
            Vector3 spawnerWorld = FloorPosToWorld(spawnerCell);

            GameObject spawnerGO = Instantiate(_enemySpawnerPrefab, spawnerWorld, Quaternion.identity);
            _enemySpawners[r] = spawnerGO;
            _spawnerPatrolPoints[r] = patrolPoints;
        }
        SpawnSpooderSpawner();

        _hud = new PlayerHUD(_healthSlider, _manaSlider, _scoreText, _bulletCooldownOverlay, _fireBallCooldownOverlay, _player, _player.FireballAbility, _player.ShootBulletAbility);
    }

    // Picks a random floor tile from the room, or falls back to the room center if empty.
    private Vector2Int GetAnyFloorInRoom(IList<Vector2Int> roomFloor, Vector2Int fallback)
    {
        if (roomFloor != null && roomFloor.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, roomFloor.Count);
            return roomFloor[idx];
        }
        return fallback;
    }

    // Converts dungeon grid coords (Vector2Int) to a world position.
    private Vector3 FloorPosToWorld(Vector2Int pos)
    {
        return new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
    }

    public void SpawnSpooderSpawner()
    {
        for (int i = 0; i < _enemySpawners.Length; i++)
        {
            GameObject spawner = _enemySpawners[i];
            List<Vector3> patrolPoints = _spawnerPatrolPoints[i];

            var spawnerGO = new SpooderSpawner(spawner, _spooderGos, spawner.transform, _player.gameobject.transform, _spooderData, patrolPoints, _maxAliveAtSameTime, _entitiesToSpawn);
        }
    }

    public void PlayerDied()
    {
        Debug.Log("PlayerDied");
        OnPlayerDied?.Invoke();
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
}
