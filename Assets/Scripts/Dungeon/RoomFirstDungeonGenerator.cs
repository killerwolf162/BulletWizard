using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{

    [SerializeField] private int minRoomWidth = 5;
    [SerializeField] private int minRoomHeight = 5;
    [SerializeField] private int minRoomCount = 7;

    [SerializeField] private int dungeonWidth = 50;
    [SerializeField] private int dungeonHeight = 50;

    [SerializeField] private bool randomWalkRooms = false;
    [Range(0, 10)] [SerializeField] private int offset = 1;

    [Range(0.0f, 1f)] [SerializeField] private float mediumCorridorPercent = 0.25f; // ~width 2
    [Range(0.0f, 1f)] [SerializeField] private float largeCorridorPercent = 0.25f;  // ~width 3

    public RoomFirstDungeonGenerator(TilemapVisualizer visualizer, SimpleRandomWalkData walkData, WallGenerationParameters wallParameters, SecretRoomParameters roomParameters)
    {
        tilemapVisualizer = visualizer;
        randomWalkParameters = walkData;
        wallGeneratorParameters = wallParameters;
        secertRoomParameters = roomParameters;
    }

    private void Awake()
    {
        //tilemapVisualizer.Clear();
        //Run();
    }

    protected override void Run()
    {
        List<BoundsInt> _roomList;
        do
        {
            _roomList = ProceduralGenerationAlgorithm.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPos,
            new Vector3Int(dungeonWidth, dungeonHeight, 0)),
            minRoomWidth, minRoomHeight);
        }
        while (_roomList.Count < minRoomCount);

        CreateRooms(_roomList);
    }

    private void CreateRooms(List<BoundsInt> _roomList)
    {
        HashSet<Vector2Int> _floor = new();

        if(randomWalkRooms)
            _floor = CreateWalkRooms(_roomList);
        else
            _floor = CreateSimpleRooms(_roomList);

        List<Vector2Int> _roomCenters = new();
        foreach (var _room in _roomList)
        {
            _roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(_room.center));
        }

        List<List<Vector2Int>> _corridorPaths;
        HashSet<Vector2Int> _corridors = ConnectRooms(_roomCenters, out _corridorPaths);

        _floor.UnionWith(_corridors);
        IncreaseCorridors(_corridorPaths, _floor, mediumCorridorPercent, largeCorridorPercent);

        tilemapVisualizer.PaintFloorTiles(_floor);
        WallGenerator.CreateWalls(_floor, tilemapVisualizer, wallGeneratorParameters, secertRoomParameters);
    }

    private HashSet<Vector2Int> CreateWalkRooms(List<BoundsInt> _roomList)
    {
        HashSet<Vector2Int> _floor = new();
        for (int i = 0; i < _roomList.Count; i++)
        {
            var _roomBounds = _roomList[i];
            var _roomCenter = new Vector2Int(Mathf.RoundToInt(_roomBounds.center.x), Mathf.RoundToInt(_roomBounds.center.y));
            var _roomFloor = RunRandomWalk(randomWalkParameters, _roomCenter);
            foreach (var _position in _roomFloor)
            {
                if (_position.x >= (_roomBounds.xMin + offset) && _position.x <= (_roomBounds.xMax - offset) && 
                    _position.y >= (_roomBounds.yMin + offset) && _position.y <= (_roomBounds.yMax - offset))
                {
                    _floor.Add(_position);
                }
            }
        }
        return _floor;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> _roomList)
    {
        HashSet<Vector2Int> _floor = new();
        foreach (var _room in _roomList)
        {
            for (int col = offset; col < _room.size.x - offset; col++)
            {
                for (int row = offset; row < _room.size.y; row++)
                {
                    Vector2Int _pos = (Vector2Int)_room.min + new Vector2Int(col, row);
                    _floor.Add(_pos);
                }
            }
        }
        return _floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> _roomCenters, out List<List<Vector2Int>> _corridorPaths)
    {
        HashSet<Vector2Int> _corridors = new();
        _corridorPaths = new List<List<Vector2Int>>();

        var _currentRoomCenter = _roomCenters[UnityEngine.Random.Range(0, _roomCenters.Count)];
        _roomCenters.Remove(_currentRoomCenter);

        while(_roomCenters.Count > 0)
        {
            Vector2Int _closestCenter = FindClosestPoint(_currentRoomCenter, _roomCenters);
            _roomCenters.Remove(_closestCenter);

            var _path = CreateCorridorPath(_currentRoomCenter, _closestCenter);
            _corridorPaths.Add(_path);

            foreach (var p in _path) _corridors.Add(p);

            _currentRoomCenter = _closestCenter;
        }
        return _corridors;
    }

    private Vector2Int FindClosestPoint(Vector2Int _currentRoomCenter, List<Vector2Int> _roomCenters)
    {
        Vector2Int _closest = Vector2Int.zero;
        float _distance = float.MaxValue;
        foreach (var _pos in _roomCenters)
        {
            float _currentDistance = Vector2.Distance(_pos, _currentRoomCenter);
            if (_currentDistance < _distance)
            {
                _distance = _currentDistance;
                _closest = _pos;
            }
        }
        return _closest;
    }

    private List<Vector2Int> CreateCorridorPath(Vector2Int _start, Vector2Int _destination)
    {
        List<Vector2Int> _path = new();
        var _pos = _start;
        _path.Add(_pos);

        while (_pos.y != _destination.y)
        {
            _pos += (_destination.y > _pos.y) ? Vector2Int.up : Vector2Int.down;
            _path.Add(_pos);
        }
        while (_pos.x != _destination.x)
        {
            _pos += (_destination.x > _pos.x) ? Vector2Int.right : Vector2Int.left;
            _path.Add(_pos);
        }
        return _path;
    }

    private void IncreaseCorridors(List<List<Vector2Int>> _corridors, HashSet<Vector2Int> _floorPositions, float _mediumCorridorPercent, float _largeCorridorPercent)
    {
        int _totalCount = _corridors.Count;
        if (_totalCount == 0) return;

        int _mediumCorridorCount = Mathf.Clamp(Mathf.RoundToInt(_totalCount * _mediumCorridorPercent), 0, _totalCount);
        int _largeCorridorCount = Mathf.Clamp(Mathf.RoundToInt(_totalCount * _largeCorridorPercent), 0, _totalCount - _mediumCorridorCount);

        var _corridorsCopy = new List<List<Vector2Int>>(_totalCount);
        for (int i = 0; i < _totalCount; i++)
            _corridorsCopy.Add(new List<Vector2Int>(_corridors[i]));

        var _shuffled = Enumerable.Range(0, _totalCount).OrderBy(_ => UnityEngine.Random.value).ToList();
        var _workingSet = new HashSet<Vector2Int>();

        for (int k = 0; k < _mediumCorridorCount; k++)
        {
            int i = _shuffled[k];
            var _newCorridor = IncreaseCorridorSizeByOne(_corridorsCopy[i]);
            _corridors[i] = _newCorridor;
            foreach (var p in _newCorridor) _workingSet.Add(p);
        }
        for (int k = _mediumCorridorCount; k < _mediumCorridorCount + _largeCorridorCount; k++)
        {
            int i = _shuffled[k];
            var _newCorridor = IncreaseCorridorSizeBrush3by3(_corridorsCopy[i]);
            _corridors[i] = _newCorridor;
            foreach (var p in _newCorridor) _workingSet.Add(p);
        }

        _floorPositions.UnionWith(_workingSet);
    }

    private List<Vector2Int> IncreaseCorridorSizeByOne(List<Vector2Int> _corridor)
    {
        List<Vector2Int> _newCorridor = new();
        Vector2Int _previousDirection = Vector2Int.zero;

        for (int i = 1; i < _corridor.Count; i++)
        {
            Vector2Int _directionFromCell = _corridor[i] - _corridor[i - 1];
            if (_previousDirection != Vector2Int.zero && _directionFromCell != _previousDirection)
            {
                for (int x = -1; x < 2; x++)
                    for (int y = -1; y < 2; y++)
                        _newCorridor.Add(_corridor[i - 1] + new Vector2Int(x, y));

                _previousDirection = _directionFromCell;
            }
            else
            {
                Vector2Int _offset = GetDirection90From(_directionFromCell);
                _newCorridor.Add(_corridor[i - 1]);
                _newCorridor.Add(_corridor[i - 1] + _offset);
            }
        }
        return _newCorridor;
    }
    private List<Vector2Int> IncreaseCorridorSizeBrush3by3(List<Vector2Int> _corridor)
    {
        List<Vector2Int> _newCorridor = new();
        for (int i = 1; i < _corridor.Count; i++)
        {
            for (int x = -1; x < 2; x++)
                for (int y = -1; y < 2; y++)
                    _newCorridor.Add(_corridor[i - 1] + new Vector2Int(x, y));
        }
        return _newCorridor;
    }

    private Vector2Int GetDirection90From(Vector2Int _direction)
    {
        if (_direction == Vector2Int.up) return Vector2Int.right;
        if (_direction == Vector2Int.right) return Vector2Int.down;
        if (_direction == Vector2Int.down) return Vector2Int.left;
        if (_direction == Vector2Int.left) return Vector2Int.up;
        return Vector2Int.zero;
    }

}
