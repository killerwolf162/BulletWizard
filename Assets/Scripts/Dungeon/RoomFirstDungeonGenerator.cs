using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    public IReadOnlyList<BoundsInt> Rooms => _rooms;
    public IReadOnlyList<Vector2Int> RoomCenters => _roomCenters;
    public IReadOnlyList<List<Vector2Int>> RoomFloors => _roomFloors;
    public IReadOnlyList<SecretRoom> SecretRooms => _secretRoomGenerator.SpawnedRooms;
    public HashSet<Vector2Int> AllFloorPositions => _allFloorPositions;

    private List<BoundsInt> _rooms;
    private List<Vector2Int> _roomCenters;
    private List<List<Vector2Int>> _roomFloors;
    private HashSet<Vector2Int> _allFloorPositions;
    private readonly SecretRoomGenerator _secretRoomGenerator;
    private DungeonData _dungeonData;

    public RoomFirstDungeonGenerator(TilemapVisualizer visualizer, SimpleRandomWalkData walkData, DungeonData dungeonData, WallGenerationParameters wallParameters, SecretRoomParameters roomParameters)
    {
        tilemapVisualizer = visualizer;
        randomWalkParameters = walkData;
        wallGeneratorParameters = wallParameters;
        secertRoomParameters = roomParameters;
        _dungeonData = dungeonData;
        _secretRoomGenerator = new SecretRoomGenerator(visualizer, wallParameters, roomParameters);
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
            new Vector3Int(_dungeonData.dungeonWidth, _dungeonData.dungeonHeight, 0)),
            _dungeonData.minRoomWidth, _dungeonData.minRoomHeight);
        }
        while (_roomList.Count < _dungeonData.minRoomCount);

        if (_roomList.Count > _dungeonData.maxRoomCount)
        {
            // Trim to max room count with a shuffle so we don't always drop the same quadrants
            FisherYatesShuffle.Shuffle(_roomList);
            _roomList = _roomList.GetRange(0, _dungeonData.maxRoomCount);
        }

        // Clamp individual room dimensions and re-centre within their BSP cell
        for (int i = 0; i < _roomList.Count; i++)
        {
            BoundsInt b = _roomList[i];

            int currentW = b.size.x;
            int currentH = b.size.y;

            int clampedW = Mathf.Clamp(currentW, _dungeonData.minRoomWidth, _dungeonData.maxRoomWidth);
            int clampedH = Mathf.Clamp(currentH, _dungeonData.minRoomHeight, _dungeonData.maxRoomHeight);

            // Only bother re-centering if we actually shrank something
            if (clampedW != currentW || clampedH != currentH)
            {
                int xOffset = (currentW - clampedW) / 2;
                int yOffset = (currentH - clampedH) / 2;

                b.SetMinMax(
                    new Vector3Int(b.xMin + xOffset, b.yMin + yOffset, 0),
                    new Vector3Int(b.xMin + xOffset + clampedW, b.yMin + yOffset + clampedH, 0)
                );
                _roomList[i] = b;
            }
        }

        CreateRooms(_roomList);
    }

    private void CreateRooms(List<BoundsInt> roomList)
    {
        // Carve floor
        HashSet<Vector2Int> floor = _dungeonData.randomWalkRooms
            ? CreateWalkRooms(roomList)
            : CreateSimpleRooms(roomList);

        // Build per-room floor lists and merge overlapping rooms
        _rooms = new List<BoundsInt>(roomList);
        _roomFloors = new List<List<Vector2Int>>(roomList.Count);

        foreach (var room in roomList)
        {
            var cells = new List<Vector2Int>();
            for (int x = room.xMin + _dungeonData.offset; x < room.xMax - _dungeonData.offset; x++)
                for (int y = room.yMin + _dungeonData.offset; y < room.yMax - _dungeonData.offset; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (floor.Contains(pos))cells.Add(pos);
                }

            _roomFloors.Add(cells);
        }

        _roomFloors = MergeConnectedRooms(_roomFloors, floor);
        _roomCenters = _roomFloors.Select(cells =>
        {
            var avg = Vector2.zero;
            foreach (var c in cells) avg += (Vector2)c;
            avg /= cells.Count;
            return Vector2Int.RoundToInt(avg);
        }).ToList();

        // Connect rooms with corridors
        HashSet<Vector2Int> corridors = ConnectRooms(_roomCenters.ToList(), out var corridorPaths);
        floor.UnionWith(corridors);
        IncreaseCorridors(corridorPaths, floor, _dungeonData.mediumCorridorPercent, _dungeonData.largeCorridorPercent);

        // Paint floor tiles
        tilemapVisualizer.PaintFloorTiles(floor);

        // Derive initial wall set for secret rooms
        var walls = WallGenerator.FindWallsInDirections(floor, Direction2D.eightDirectionsList);

        // Determine shared perlin offset
        Vector2 perlinOffset = WallGenerator.BuildPerlinOffset(wallGeneratorParameters);

        // Spawn Secretrooms
        _secretRoomGenerator.Run(floor, walls, perlinOffset);

        // Final wall pass
        _allFloorPositions = floor;
        WallGenerator.CreateWalls(floor, tilemapVisualizer, wallGeneratorParameters);
    }

    private List<List<Vector2Int>> MergeConnectedRooms(List<List<Vector2Int>> roomFloors, HashSet<Vector2Int> allFloor)
    {
        int count = roomFloors.Count;
        int[] parent = Enumerable.Range(0, count).ToArray();

        int Find(int x)
        {
            while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; }
            return x;
        }
        void Union(int a, int b) { parent[Find(a)] = Find(b); }

        // Build a lookup: floor cell -> which rooms contain it
        var cellToRooms = new Dictionary<Vector2Int, List<int>>();
        for (int i = 0; i < count; i++)
        {
            foreach (var cell in roomFloors[i])
            {
                if (!cellToRooms.TryGetValue(cell, out var list))
                    cellToRooms[cell] = list = new List<int>();
                list.Add(i);
            }
        }

        // Also check adjacency (rooms separated by a wall that got removed)
        for (int i = 0; i < count; i++)
        {
            foreach (var cell in roomFloors[i])
            {
                foreach (var dir in Direction2D.cardinalDirections)
                {
                    var neighbour = cell + dir;
                    if (cellToRooms.TryGetValue(neighbour, out var others))
                        foreach (int j in others)
                            if (i != j) Union(i, j);
                }
            }
        }

        // Group rooms by root
        var groups = new Dictionary<int, List<Vector2Int>>();
        for (int i = 0; i < count; i++)
        {
            int root = Find(i);
            if (!groups.TryGetValue(root, out var merged))
                groups[root] = merged = new List<Vector2Int>();
            merged.AddRange(roomFloors[i]);
        }

        // Deduplicate cells within each merged room
        var result = new List<List<Vector2Int>>();
        foreach (var g in groups.Values)
            result.Add(g.Distinct().ToList());

        return result;
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
                if (_position.x >= (_roomBounds.xMin + _dungeonData.offset) && _position.x <= (_roomBounds.xMax - _dungeonData.offset) &&
                    _position.y >= (_roomBounds.yMin + _dungeonData.offset) && _position.y <= (_roomBounds.yMax - _dungeonData.offset))
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
            for (int col = _dungeonData.offset; col < _room.size.x - _dungeonData.offset; col++)
            {
                for (int row = _dungeonData.offset; row < _room.size.y; row++)
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

        while (_roomCenters.Count > 0)
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
