using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGenerationAlgorithm
{
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int _startPos, int _walkLength)
    {
        HashSet<Vector2Int> _path = new();

        _path.Add(_startPos);
        var _previousPosition = _startPos;

        for (int i = 0; i < _walkLength; i++)
        {
            var _newPosition = _previousPosition + Direction2D.GetRandomCardinalDirection();
            _path.Add(_newPosition);
            _previousPosition = _newPosition;
        }
        return _path;
    }

    public static List<Vector2Int> RandomWalkCorridor(Vector2Int _startPos, int _corridorLength)
    {
        List<Vector2Int> _path = new();
        var _direction = Direction2D.GetRandomCardinalDirection();
        var _currentPos = _startPos;
        _path.Add(_startPos);

        for (int i = 0; i < _corridorLength; i++)
        {
            _currentPos += _direction;
            _path.Add(_currentPos);
        }
        return _path;
    }

    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<BoundsInt> _roomsQueue = new();
        List<BoundsInt> _roomList = new();

        _roomsQueue.Enqueue(spaceToSplit);
        while (_roomsQueue.Count > 0)
        {
            var _room = _roomsQueue.Dequeue();
            if (_room.size.y >= minHeight && _room.size.x >= minWidth)
            {
                if (Random.value < 0.5f)
                {
                    if (_room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, _roomsQueue, _room);
                    }
                    else if (_room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, _roomsQueue, _room);
                    }
                    else if (_room.size.x >= minWidth && _room.size.y >= minHeight)
                    {
                        _roomList.Add(_room);
                    }
                }
                else
                {
                    if (_room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, _roomsQueue, _room);
                    }
                    else if (_room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, _roomsQueue, _room);
                    }
                    else if (_room.size.x >= minWidth && _room.size.y >= minHeight)
                    {
                        _roomList.Add(_room);
                    }
                }
            }
        }
        return _roomList;
    }

    private static void SplitVertically(int _minWidth, Queue<BoundsInt> _roomsQueue, BoundsInt _room)
    {
        var _xSplit = Random.Range(1, _room.size.x);
        BoundsInt _room1 = new BoundsInt(_room.min, new Vector3Int(_xSplit, _room.size.y, _room.size.z));
        BoundsInt _room2 = new BoundsInt(new Vector3Int(_room.min.x + _xSplit, _room.min.y, _room.min.z), new Vector3Int(_room.size.x - _xSplit, _room.size.y, _room.size.z));

        _roomsQueue.Enqueue(_room1);
        _roomsQueue.Enqueue(_room2);
    }

    private static void SplitHorizontally(int _minHeight, Queue<BoundsInt> _roomsQueue, BoundsInt _room)
    {
        var _ySplit = Random.Range(1, _room.size.y); //(minHeight, room.size.y - minHeight) for gridlike structure
        BoundsInt _room1 = new BoundsInt(_room.min, new Vector3Int(_room.size.x, _ySplit, _room.size.z));
        BoundsInt _room2 = new BoundsInt(new Vector3Int(_room.min.x, _room.min.y + _ySplit, _room.min.z), new Vector3Int(_room.size.x, _room.size.y - _ySplit, _room.size.z));

        _roomsQueue.Enqueue(_room1);
        _roomsQueue.Enqueue(_room2);
    }
}
