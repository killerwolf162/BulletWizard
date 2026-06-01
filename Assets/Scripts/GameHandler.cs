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
    public event Action<AbstractItem> OnItemCollected;

    public RunData runData;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _floorMap;
    [SerializeField] private Tilemap _wallMap;
    public Tilemap FloorMap => _floorMap;
    public Tilemap WallMap => _wallMap;
    public AStarPathfinder Pathfinder { get; private set; }

    [Header("DungeonGenerationConfig")]
    [SerializeField] private SimpleRandomWalkData _walkData;
    [SerializeField] private WallGenerationParameters _wallParameters;
    [SerializeField] private SecretRoomParameters _secretRoomParameters;
    [SerializeField] private DungeonData _baseDungeonParameters;
    [SerializeField] private LevelScalingConfig _levelScalingConfig;

    private int minRoomWidth;
    private int minRoomHeight;
    private int minRoomCount;
    private int maxRoomCount;
    private int maxRoomWidth;
    private int maxRoomHeight;

    private int dungeonWidth;
    private int dungeonHeight;

    private TilemapVisualizer _tilemapVisualizer;
    private RoomFirstDungeonGenerator _generator;

    [Header("Spawning")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _enemySpawnerPrefab;
    [SerializeField] private GameObject _staircasePrefab;
    [SerializeField] private GameObject _itemChestPrefab;
    [SerializeField] private GameObject _openedItemChestPrefab;
    [SerializeField] private int _maxSpawners;
    [SerializeField] private int _maxAliveAtSameTime;
    [SerializeField] private int _entitiesToSpawn;
    private GameObject[] _enemySpawners;
    private List<Vector3>[] _spawnerPatrolPoints;
    private int _playerRoomIdx;

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

    [Header("item pop-up ")]
    public bool IsUIActive { get; set; } = false;
    [SerializeField] private GameObject _itemPopupPanel;
    [SerializeField] private TMP_Text _popupItemName;
    [SerializeField] private TMP_Text _popupItemDescription;
    [SerializeField] private TMP_Text _popupStatChange;

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
        runData.currentLevel = 1;
        SetBaseParameters();
        var dungeonParams = new DungeonData();
        ApplyDungeonParameters(dungeonParams);
        ApplyLevelScaling(runData.currentLevel);
        _generator = new RoomFirstDungeonGenerator(_tilemapVisualizer, _walkData, dungeonParams, _wallParameters, _secretRoomParameters);
        _generator.GenerateDungeon();

        Pathfinder = new AStarPathfinder(_floorMap, _wallMap);

        cam = FindAnyObjectByType<Camera>();
        SpawnEntities();
        ItemPool.Initialize();
        SpawnExitAndItems();
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

    private void SaveRunData()
    {
        runData = _player.SaveRunData();
        runData.score = _score;
        runData.currentLevel++;
    }

    public void ModifyScore(int amount)
    {
        _score += amount;
        ScoreChanged?.Invoke(_score);
    }

    private void SetBaseParameters()
    {
        minRoomWidth = _baseDungeonParameters.minRoomWidth;
        minRoomHeight = _baseDungeonParameters.minRoomHeight;
        minRoomCount = _baseDungeonParameters.minRoomCount;
        maxRoomCount = _baseDungeonParameters.maxRoomCount;
        maxRoomWidth = _baseDungeonParameters.maxRoomWidth;
        maxRoomHeight = _baseDungeonParameters.maxRoomHeight;

        dungeonWidth = _baseDungeonParameters.dungeonWidth;
        dungeonHeight = _baseDungeonParameters.dungeonHeight;
    }

    private void ApplyDungeonParameters(DungeonData data)
    {
        data.minRoomWidth = minRoomWidth;
        data.minRoomHeight = minRoomHeight;
        data.minRoomCount = minRoomCount;
        data.maxRoomCount = maxRoomCount;
        data.maxRoomWidth = maxRoomWidth;
        data.maxRoomHeight = maxRoomHeight;

        data.dungeonWidth = dungeonWidth;
        data.dungeonHeight = dungeonHeight;
    }

    private void ApplyLevelScaling(int level)
    {
        _entitiesToSpawn = Mathf.RoundToInt(_levelScalingConfig.BaseEnemyCount * Mathf.Pow(_levelScalingConfig.EnemyCountMultiplyerPerLevel, level));

        float healthMod = _levelScalingConfig.EnemyHealthScaling.Evaluate(level);
        float roomSizeMod = _levelScalingConfig.RoomSizeScaling.Evaluate(level);
        float dungeonSizeMod = _levelScalingConfig.DungeonSizeScaling.Evaluate(level);

        minRoomWidth = Mathf.RoundToInt(minRoomWidth * roomSizeMod);
        minRoomHeight = Mathf.RoundToInt(minRoomHeight * roomSizeMod);
        minRoomCount = Mathf.RoundToInt(minRoomCount * roomSizeMod);
        maxRoomCount = Mathf.RoundToInt(maxRoomCount * roomSizeMod);
        maxRoomWidth = Mathf.RoundToInt(maxRoomWidth * roomSizeMod);
        maxRoomHeight = Mathf.RoundToInt(maxRoomHeight * roomSizeMod);

        dungeonWidth = Mathf.RoundToInt(dungeonWidth * dungeonSizeMod);
        dungeonHeight = Mathf.RoundToInt(dungeonHeight * dungeonSizeMod);
    }

    private void SpawnEntities()
    {
        var roomCenters = _generator.RoomCenters;
        var roomFloors = _generator.RoomFloors;

        if (roomCenters == null || roomCenters.Count == 0)
        {
            Debug.Log("Dungeon has no rooms");
            return;
        }

        SpawnPlayer(roomCenters, roomFloors);
        SpawnSpawners(roomCenters, roomFloors);
    }

    private void SpawnPlayer(IReadOnlyList<Vector2Int> roomCenters, IReadOnlyList<List<Vector2Int>> roomFloors)
    {
        // Find center room
        Vector2 avg = Vector2.zero;
        foreach (var c in roomCenters) avg += c;
        avg /= roomCenters.Count;

        // Set room idx
        _playerRoomIdx = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < roomCenters.Count; i++)
        {
            float d = (roomCenters[i] - avg).sqrMagnitude;
            if (d < bestDist) { bestDist = d; _playerRoomIdx = i; }
        }

        // Spawn player + HUD
        Vector2Int cell = GetAnyFloorInRoom(roomFloors[_playerRoomIdx], roomCenters[_playerRoomIdx]);
        GameObject playerGo = Instantiate(_playerPrefab, FloorPosToWorld(cell), Quaternion.identity);
        _player = new PlayerController(playerGo, _elementDatas);
        _hud = new PlayerHUD(
            _healthSlider, _manaSlider, _scoreText,
            _bulletCooldownOverlay, _fireBallCooldownOverlay,
            _player, _player.FireballAbility, _player.ShootBulletAbility,
            _itemPopupPanel, _popupItemName, _popupItemDescription, _popupStatChange
            );
    }

    private void SpawnSpawners(IReadOnlyList<Vector2Int> roomCenters, IReadOnlyList<List<Vector2Int>> roomFloors)
    {
        // Every room except the player room
        var spawnRoomIndices = new List<int>();
        for (int i = 0; i < roomCenters.Count; i++)
        {
            if (i != _playerRoomIdx) spawnRoomIndices.Add(i);
        }

        _enemySpawners = new GameObject[spawnRoomIndices.Count];
        _spawnerPatrolPoints = new List<Vector3>[spawnRoomIndices.Count];

        for (int r = 0; r < spawnRoomIndices.Count; r++)
        {
            int roomIdx = spawnRoomIndices[r];
            var roomFloorCells = roomFloors[roomIdx];

            var patrolPoints = new List<Vector3>(roomFloorCells?.Count ?? 0);
            if (roomFloorCells != null)
                foreach (var cell in roomFloorCells)
                    patrolPoints.Add(FloorPosToWorld(cell));

            Vector2Int spawnerCell = GetAnyFloorInRoom(roomFloorCells, roomCenters[roomIdx]);
            GameObject spawnerGO = Instantiate(_enemySpawnerPrefab, FloorPosToWorld(spawnerCell),
                                                 Quaternion.identity);

            _enemySpawners[r] = spawnerGO;
            _spawnerPatrolPoints[r] = patrolPoints;
        }
        SpawnSpooderSpawner();
    }

    private void SpawnExitAndItems()
    {
        var secretRooms = _generator.SecretRooms;

        if (secretRooms.Count > 0)
        {
            SecretRoom exitRoom = secretRooms[0];
            Vector3 stairWorldPos = FloorPosToWorld(exitRoom.Center);
            var staircaseGo = Instantiate(_staircasePrefab, stairWorldPos, Quaternion.identity);
            var staircase = new StairCase(staircaseGo);

            for (int i = 1; i < secretRooms.Count; i++)
            {
                SecretRoom itemRoom = secretRooms[i];
                Vector3 chestWorldPos = FloorPosToWorld(itemRoom.Center);
                var chestGO = Instantiate(_itemChestPrefab, chestWorldPos, Quaternion.identity);
                var itemChest = new ItemChest(chestGO);
                itemChest.ChestOpened += OnChestOpened;
            }
        }
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

            var spawnerGO = new SpooderSpawner(spawner, _spooderGos, spawner.transform, _player._gameObject.transform, _spooderData, patrolPoints, _maxAliveAtSameTime, _entitiesToSpawn);
        }
    }

    public void PlayerDied()
    {
        Debug.Log("PlayerDied");
        OnPlayerDied?.Invoke();
    }

    public void OnChestOpened(ItemChest chest, AbstractItem item)
    {
        Vector3 position = chest._gameObject.transform.position;

        Destroy(chest._gameObject);
        Instantiate(_openedItemChestPrefab, position, Quaternion.identity);

        if (item != null)
            OnItemCollected?.Invoke(item);
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

    public void ResumeTime()
    {
        Time.timeScale = 1;
    }

    public void StopTime()
    {
        Time.timeScale = 0;
    }
}
