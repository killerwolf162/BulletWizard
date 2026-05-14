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
    public event Action onPlayerDied;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorMap;
    [SerializeField] private Tilemap wallMap;
    public Tilemap FloorMap => floorMap;
    public Tilemap WallMap => wallMap;
    public AStarPathfinder Pathfinder { get; private set; }

    [Header("DungeonGeneration")]
    [SerializeField] private SimpleRandomWalkData walkData;
    [SerializeField] private WallGenerationParameters wallParameters;
    [SerializeField] private SecretRoomParameters roomParameters;

    private TilemapVisualizer _tilemapVisualizer;
    private RoomFirstDungeonGenerator _generator;

    [Header("Spawning")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _enemySpawnerPrefab;
    [SerializeField] private int _maxSpawners;
    private GameObject[] _enemySpawners;
    private List<Vector3>[] _spawnerPatrolPoints;

    [Header("Player Data")]
    [SerializeField] private List<ElementData> _elementDatas = new List<ElementData>();

    [Header("EnemyGos & Data")]
    [SerializeField] private List<EnemyData> _spooderData;
    [SerializeField] private GameObject[] _spooderGos;

    [Header("UI")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private Image _bulletCooldownOverlay;
    [SerializeField] private Image _fireBallCooldownOverlay;

    public Camera cam;

    private ISceneObject _player;
    private List<IUpdateable> _updateables = new List<IUpdateable>();
    private PlayerHUD _hud;
    public int Score => _score;
    private int _score;

    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; // disable vsync so targetFrameRate takes effect
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        instance = this;
        _tilemapVisualizer = new TilemapVisualizer(floorMap, wallMap);
        _generator = new RoomFirstDungeonGenerator(_tilemapVisualizer, walkData, wallParameters, roomParameters);
        _generator.GenerateDungeon();

        Pathfinder = new AStarPathfinder(floorMap, wallMap);

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

    private void SpawnPlayerAndSpawners()
    {
        var roomCenters = _generator.RoomCenters;
        var roomFloors = _generator.RoomFloors;

        if (roomCenters == null || roomCenters.Count == 0)
        {
            Debug.Log("Dungeon has no rooms");
            return;
        }

        // Spawn player in middle room //

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

        // Spawn spawners //

        List<int> roomIdcs = new List<int>();
        for (int i = 0; i < roomCenters.Count; i++)
        {
            if (i == playerRoomIdx)
                continue;
            roomIdcs.Add(i);
        }

        //shuffle spawner rooms
        for (int i = roomIdcs.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = roomIdcs[i];
            roomIdcs[i] = roomIdcs[j];
            roomIdcs[j] = temp;
        }

        int spawnerCount = Mathf.Min(_maxSpawners, roomIdcs.Count);
        _enemySpawners = new GameObject[spawnerCount];
        _spawnerPatrolPoints = new List<Vector3>[spawnerCount];

        // prep spawnerGOs
        for (int s = 0; s < spawnerCount; s++)
        {
            int roomIdx = roomIdcs[s];
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
            _enemySpawners[s] = spawnerGO;
            _spawnerPatrolPoints[s] = patrolPoints;
        }
        SpawnSpooderSpawner();

        var playerController = (PlayerController)_player;
        _hud = new PlayerHUD(_healthSlider, _scoreText, _bulletCooldownOverlay, _fireBallCooldownOverlay, playerController, playerController.FireballAbility, playerController.ShootBulletAbility);
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
        // If the player appears off-center, add +0.5f offsets:
        return new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
    }

    public void SpawnSpooderSpawner()
    {
        for (int i = 0; i < _enemySpawners.Length; i++)
        {
            GameObject spawner = _enemySpawners[i];
            List<Vector3> patrolPoints = _spawnerPatrolPoints[i];

            var spawnerGO = new SpooderSpawner(spawner, _spooderGos, spawner.transform, _player.gameobject.transform, _spooderData, patrolPoints);
        }
    }

    public void PlayerDied()
    {
        Debug.Log("PlayerDied");
        onPlayerDied?.Invoke();
    }

    public void IncreaseScore(int scoreToAdd)
    {
        _score += scoreToAdd;
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
