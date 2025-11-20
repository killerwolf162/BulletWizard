using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorFirstGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField] private int corridorLength = 14;
    [SerializeField] private int corridorCount = 5;


    [Range(0.1f, 1)] [SerializeField] private float roomPercent = 0.8f;
    [Range(0.1f, 1)] [SerializeField] private float mediumCorridorPercent = 0.25f;
    [Range(0.1f, 1)] [SerializeField] private float largeCorridorPercent = 0.25f;

    protected override void Run()
    {
        CorridorFirstGeneration();
    }

    private void CorridorFirstGeneration()
    {
        HashSet<Vector2Int> _floorPositions = new();
        HashSet<Vector2Int> _potRoomPositions = new();

        List<List<Vector2Int>> _corridors = CreateCorridors(_floorPositions, _potRoomPositions);

        HashSet<Vector2Int> _roomPositions = CreateRooms(_potRoomPositions);

        List<Vector2Int> _deadEnds = FindDeadends(_floorPositions);
        CreateRoomsAtDeadEnds(_deadEnds, _roomPositions);

        _floorPositions.UnionWith(_roomPositions);

        IncreaseCorridors(_corridors, _floorPositions, mediumCorridorPercent, largeCorridorPercent);

        tilemapVisualizer.PaintFloorTiles(_floorPositions);
        WallGenerator.CreateWalls(_floorPositions, tilemapVisualizer, wallGeneratorParameters, secertRoomParameters);
    }

    private List<List<Vector2Int>> CreateCorridors(HashSet<Vector2Int> _floorPositions, HashSet<Vector2Int> _potRoomPositions)
    {
        var _currentPos = startPos;
        _potRoomPositions.Add(_currentPos);
        List<List<Vector2Int>> _corridors = new();

        for (int i = 0; i < corridorCount; i++)
        {
            var _path = ProceduralGenerationAlgorithm.RandomWalkCorridor(_currentPos, corridorLength);
            _corridors.Add(_path);
            _currentPos = _path[_path.Count - 1];
            _potRoomPositions.Add(_currentPos);
            _floorPositions.UnionWith(_path);
        }
        return _corridors;
    }

    private void IncreaseCorridors(List<List<Vector2Int>> _corridors, HashSet<Vector2Int> _floorPositions, float _mediumCorridorPercent, float _largeCorridorPercent)
    {
        int _totalCount = _corridors.Count;
        int _mediumCorridorCount = Mathf.Clamp(Mathf.RoundToInt(_totalCount * _mediumCorridorPercent), 0, _totalCount);
        int _largeCorridorCount = Mathf.Clamp(Mathf.RoundToInt(_totalCount * _largeCorridorPercent), 0, _totalCount - _mediumCorridorCount);

        var _corridorsCopy = new List<List<Vector2Int>>(_totalCount);
        for (int i = 0; i < _totalCount; i++)
            _corridorsCopy.Add(new List<Vector2Int>(_corridors[i]));

        var _shuffeledCorridors = Enumerable.Range(0, _totalCount).OrderBy(_ => UnityEngine.Random.value).ToList();
        var _workingSet = new HashSet<Vector2Int>();

        for (int k = 0; k < _mediumCorridorCount; k++)
        {
            int i = _shuffeledCorridors[k];
            var _newCorridor = IncreaseCorridorSizeByOne(_corridorsCopy[i]);
            _corridors[i] = _newCorridor;
            _workingSet.UnionWith(_newCorridor);
        }
        for (int k = _mediumCorridorCount; k < _mediumCorridorCount + _largeCorridorCount; k++)
        {
            int i = _shuffeledCorridors[k];
            var _newCorridor = IncreaseCorridorSizeBrush3by3(_corridorsCopy[i]);
            _corridors[i] = _newCorridor;
            _workingSet.UnionWith(_newCorridor);
        }
        _floorPositions.UnionWith(_workingSet);
    }

    private List<Vector2Int> IncreaseCorridorSizeByOne(List<Vector2Int> _corridor)
    {
        List<Vector2Int> _newCorridor = new();
        Vector2Int _previousDirection = Vector2Int.zero;
        for (int i = 1 ; i < _corridor.Count; i++)
        {
            Vector2Int _directionFromCell = _corridor[i] - _corridor[i - 1];
            if(_previousDirection != Vector2Int.zero && _directionFromCell != _previousDirection)
            {
                //handle corner
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        _newCorridor.Add(_corridor[i - 1] + new Vector2Int(x, y));
                    }
                }
                _previousDirection = _directionFromCell;
            }
            else
            {
                Vector2Int _newCorridorTileOffset = GetDirection90From(_directionFromCell);
                _newCorridor.Add(_corridor[i - 1]);
                _newCorridor.Add(_corridor[i - 1] + _newCorridorTileOffset);
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
            {
                for (int y = -1; y < 2; y++)
                {
                    _newCorridor.Add(_corridor[i - 1] + new Vector2Int(x, y));
                }
            }
        }
        return _newCorridor;
    }

    private Vector2Int GetDirection90From(Vector2Int _direction)
    {
        if (_direction == Vector2Int.up)
            return Vector2Int.right;
        if (_direction == Vector2Int.right)
            return Vector2Int.down;
        if (_direction == Vector2Int.down)
            return Vector2Int.left;
        if (_direction == Vector2Int.left)
            return Vector2Int.up;
        else
            return Vector2Int.zero;
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> _potRoomPositions)
    {
        HashSet<Vector2Int> _roomPositions = new();
        int _roomCount = Mathf.RoundToInt(_potRoomPositions.Count * roomPercent);

        List<Vector2Int> _roomsToCreate = _potRoomPositions.OrderBy(x => Guid.NewGuid()).Take(_roomCount).ToList();

        foreach (var _roomPos in _roomsToCreate)
        {
            var _roomFloor = RunRandomWalk(randomWalkParameters, _roomPos);
            _roomPositions.UnionWith(_roomFloor);
        }
        return _roomPositions;
    }

    private List<Vector2Int> FindDeadends(HashSet<Vector2Int> _floorPositions)
    {
        List<Vector2Int> _deadEnds = new();
        foreach (var _position in _floorPositions)
        {
            int _neighbourCount = 0;
            foreach (var _direction in Direction2D.cardinalDirections)
            {
                if (_floorPositions.Contains(_position + _direction))
                    _neighbourCount++;
            }
            if (_neighbourCount == 1)
                _deadEnds.Add(_position);
        }
        return _deadEnds;
    }

    private void CreateRoomsAtDeadEnds(List<Vector2Int> _deadEnds, HashSet<Vector2Int> _roomPositions)
    {
        foreach (var _position in _deadEnds)
        {
            if (_roomPositions.Contains(_position) == false)
            {
                var _room = RunRandomWalk(randomWalkParameters, _position);
                _roomPositions.UnionWith(_room);
            }
        }
    }

    
}
