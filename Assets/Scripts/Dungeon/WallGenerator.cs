using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class WallGenerator
{
    public static void CreateWalls(HashSet<Vector2Int> _floor, TilemapVisualizer _tilemapVisualizer, WallGenerationParameters _wallParams, SecretRoomParameters _secertRoomParams)
    {
        while (true)
        {
            var _walls = FindWallsInDirections(_floor, Direction2D.eightDirectionsList);

            bool _hasWallFull = false;
            var _toFloor = new HashSet<Vector2Int>();

            foreach (var _pos in _walls)
            {
                int _code = BuildCodeInt(_pos, _floor);
                if (!WallTypesHelper.wallFull.Contains(_code))
                    continue;

                _hasWallFull = true;

                int _wallNbrs = CountCardinalWalls(_pos, _walls);
                if (_wallNbrs >= 2)
                    continue;

                int _cardFloors = CountCardinalFloors(_code);
                if (_cardFloors == 0 || _cardFloors >= 2 || _wallNbrs == 0)
                    _toFloor.Add(_pos);
            }

            if (!_hasWallFull)
                break;

            if (_toFloor.Count == 0)
            {
                foreach (var _pos in _walls)
                {
                    int _code = BuildCodeInt(_pos, _floor);
                    if (WallTypesHelper.wallFull.Contains(_code))
                        _toFloor.Add(_pos);
                }
            }
            _floor.UnionWith(_toFloor);
            _tilemapVisualizer.PaintFloorTiles(_toFloor);
        }

        Vector2 _perlinOffset = _wallParams.randomizePerlinOffsetEachRun ? new Vector2(Random.value * 1000f, Random.value * 1000f) : _wallParams.perlinOffset;
        var _finalWalls = FindWallsInDirections(_floor, Direction2D.eightDirectionsList);
        var _secretEntrances = new List<(Vector2Int wall, Vector2Int outward)>();

        PaintWalls(_tilemapVisualizer, _finalWalls, _floor, _wallParams, _perlinOffset);

        if (_secertRoomParams.enableSecretRooms && _secertRoomParams.maxCount > 0)
        {
            SpawnSecretRoomsIncremental(_tilemapVisualizer, _floor, _finalWalls, _wallParams, _secertRoomParams, _perlinOffset);
        }
        _tilemapVisualizer.PaintFloorTiles(_floor);


        if (_secertRoomParams.enableSecretRooms && _secertRoomParams.keepEntryWallSealed)
        {
            foreach (var (_wall, _dir) in _secretEntrances)
                _tilemapVisualizer.PaintSecretEntrance(_wall, _dir);
        }
    }

    private static void PaintWalls(TilemapVisualizer _tilemapVisualizer, HashSet<Vector2Int> _walls, HashSet<Vector2Int> _floor, WallGenerationParameters _parameters, Vector2 _offset)
    {
        var _wallList = _walls.ToList();
        var _variationSet = SampleByPercent(_wallList, _parameters.variantPercent, _parameters.variantMinChebyshevDistance);

        var _damagedSet = new HashSet<Vector2Int>();
        foreach (var _pos in _wallList)
        {
            if (PerlinMask(_pos, _parameters.damagedNoiseScale, _parameters.damagedPercent, _offset))
                _damagedSet.Add(_pos);
        }

        if (_parameters.exclusive)
            _damagedSet.ExceptWith(_variationSet);

        foreach (var _pos in _walls)
        {
            string _bits = "";
            foreach (var _direction in Direction2D.eightDirectionsList)
                _bits += _floor.Contains(_pos + _direction) ? "1" : "0";

            bool _variant = _variationSet.Contains(_pos);
            bool _damaged = _damagedSet.Contains(_pos);

            _tilemapVisualizer.PaintWall(_pos, _bits, _variant, _damaged);
        }
    }

    private static void PaintSecretWalls(TilemapVisualizer _tilemapVisualizer, HashSet<Vector2Int> _newFloors, HashSet<Vector2Int> _allFloors, HashSet<Vector2Int> _mainWalls, WallGenerationParameters _param, Vector2 _offset)
    {
        var _secretWalls = FindWallsInDirections(_newFloors, Direction2D.eightDirectionsList);
        _secretWalls.ExceptWith(_mainWalls);

        var secretWallList = _secretWalls.ToList();
        var list = _secretWalls.ToList();

        var variantSet = SampleByPercent(list, _param.variantPercent, _param.variantMinChebyshevDistance);
        var damagedSet = new HashSet<Vector2Int>();
        foreach (var p in list)
            if (PerlinMask(p, _param.damagedNoiseScale, _param.damagedPercent, _offset))
                damagedSet.Add(p);
        if (_param.exclusive) damagedSet.ExceptWith(variantSet);

        foreach (var pos in _secretWalls)
        {
            string bits = "";
            foreach (var d in Direction2D.eightDirectionsList)
                bits += _allFloors.Contains(pos + d) ? "1" : "0";

            _tilemapVisualizer.PaintWall(pos, bits, variantSet.Contains(pos), damagedSet.Contains(pos));
        }

        _mainWalls.UnionWith(_secretWalls);
    }

    private static void SpawnSecretRoomsIncremental(TilemapVisualizer _tilemapVisualizer, HashSet<Vector2Int> _floor, HashSet<Vector2Int> _mainWalls, WallGenerationParameters _wallParam, SecretRoomParameters _secretRoomParam, Vector2 _offset)
    {
        if (_secretRoomParam.maxCount == 0 || _mainWalls.Count == 0) return;

        // candidates are built from the CURRENT main walls (pre-carve)
        var _candidates = new List<(Vector2Int wall, Vector2Int outward)>();
        foreach (var _wall in _mainWalls)
        {
            if (!PerlinMask(_wall, _wallParam.damagedNoiseScale, _wallParam.damagedPercent, _offset))
                continue;

            foreach (var _dir in Direction2D.eightDirectionsList)
            {
                var _inside = _wall - _dir;
                var _outside = _wall + _dir;
                if (_floor.Contains(_inside) && !_floor.Contains(_outside))
                    _candidates.Add((_wall, _dir));
            }
        }

        FisherYatesShuffle(_candidates);

        int _spawned = 0;
        foreach (var (_wall, _outward) in _candidates)
        {
            if (_spawned >= _secretRoomParam.maxCount) break;
            if (Random.value > _secretRoomParam.chancePerDmgedWall) continue;

            if (TryCarveRoom(_floor, _wall, _outward, _secretRoomParam, out var _carved))
            {
                _floor.UnionWith(_carved);

                if (!_secretRoomParam.keepEntryWallSealed)
                {
                    _floor.Add(_wall);
                    _tilemapVisualizer.ClearWallAt(_wall);
                    _tilemapVisualizer.PaintFloorTiles(new[] { _wall });
                    _mainWalls.Remove(_wall);
                }
                PaintSecretWalls(_tilemapVisualizer, _carved, _floor, _mainWalls, _wallParam, _offset);

                _spawned++;
            }
        }
    }

    private static bool TryCarveRoom(HashSet<Vector2Int> _floor, Vector2Int _wall, Vector2Int _outward, SecretRoomParameters _parameters, out HashSet<Vector2Int> _roomCells)
    {
        _roomCells = null;

        // Pick dimensions
        int _width = Random.Range(_parameters.minWidth, _parameters.maxWidth + 1);
        int _height = Random.Range(_parameters.minHeight, _parameters.maxHeight + 1);
        int _corLength = Random.Range(_parameters.corridorMinLength, _parameters.corridorMaxLength + 1);
        int _corWidth = Mathf.Max(1, _parameters.corridorWidth);

        RectInt _corridor = BuildCorridorRect(_wall, _outward, _corLength, _corWidth);
        RectInt _room = BuildRoomRectFromCorridorEnd(_corridor, _outward, _width, _height);

        RectInt union = Union(_corridor, _room);
        if (!IsAreaClear(_floor, union, _parameters.padding))
            return false;

        var set = new HashSet<Vector2Int>();
        AddRectCells(_corridor, set);
        AddRectCells(_room, set);

        _roomCells = set;

        return true;
    }

    private static RectInt BuildCorridorRect(Vector2Int wall, Vector2Int outward, int length, int width)
    {
        if (outward == Vector2Int.up || outward == Vector2Int.down)
        {
            int xMin = wall.x - (width - 1) / 2;
            int yStart = outward == Vector2Int.up ? wall.y + 1 : wall.y - length;
            return new RectInt(xMin, yStart, width, length);
        }
        else
        {
            int yMin = wall.y - (width - 1) / 2;
            int xStart = outward == Vector2Int.right ? wall.x + 1 : wall.x - length;
            return new RectInt(xStart, yMin, length, width);
        }
    }

    private static RectInt BuildRoomRectFromCorridorEnd(RectInt corridor, Vector2Int outward, int w, int h)
    {
        if (outward == Vector2Int.up)
        {
            int centerX = corridor.xMin + corridor.width / 2;
            int xMin = centerX - w / 2;
            return new RectInt(xMin, corridor.yMax, w, h);
        }
        if (outward == Vector2Int.down)
        {
            int centerX = corridor.xMin + corridor.width / 2;
            int xMin = centerX - w / 2;
            return new RectInt(xMin, corridor.yMin - h, w, h);
        }
        if (outward == Vector2Int.right)
        {
            int centerY = corridor.yMin + corridor.height / 2;
            int yMin = centerY - h / 2;
            return new RectInt(corridor.xMax, yMin, w, h);
        }
        else // left
        {
            int centerY = corridor.yMin + corridor.height / 2;
            int yMin = centerY - h / 2;
            return new RectInt(corridor.xMin - w, yMin, w, h);
        }
    }

    private static void AddRectCells(RectInt r, HashSet<Vector2Int> set)
    {
        for (int x = r.xMin; x < r.xMax; x++)
            for (int y = r.yMin; y < r.yMax; y++)
                set.Add(new Vector2Int(x, y));
    }

    private static RectInt Union(RectInt a, RectInt b)
    {
        int xMin = Mathf.Min(a.xMin, b.xMin);
        int yMin = Mathf.Min(a.yMin, b.yMin);
        int xMax = Mathf.Max(a.xMax, b.xMax);
        int yMax = Mathf.Max(a.yMax, b.yMax);
        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private static bool IsAreaClear(HashSet<Vector2Int> _floor, RectInt _area, int _padding)
    {
        int xMin = _area.xMin - _padding;
        int xMax = _area.xMax + _padding;
        int yMin = _area.yMin - _padding;
        int yMax = _area.yMax + _padding;

        for (int x = xMin; x <= xMax; x++)
            for (int y = yMin; y <= yMax; y++)
                if (_floor.Contains(new Vector2Int(x, y)))
                    return false;

        return true;
    }

    static void FisherYatesShuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static HashSet<Vector2Int> SampleByPercent(List<Vector2Int> _walls, float _percent, int _minDistance)
    {
        _percent = Mathf.Clamp01(_percent);
        int _target = Mathf.Clamp(Mathf.RoundToInt(_walls.Count * _percent), 0, _walls.Count);
        int _radius = Mathf.Max(0, _minDistance - 1);

        var _ordererdPos = Enumerable.Range(0, _walls.Count).ToList();
        FisherYatesShuffle(_ordererdPos);

        var _selected = new HashSet<Vector2Int>();
        var _blocked = new HashSet<Vector2Int>();

        foreach (var idx in _ordererdPos)
        {
            if (_selected.Count >= _target) break;

            var _pos = _walls[idx];
            if (_blocked.Contains(_pos)) continue;

            _selected.Add(_pos);

            for (int dx = -_radius; dx <= _radius; dx++)
                for (int dy = -_radius; dy <= _radius; dy++)
                    _blocked.Add(new Vector2Int(_pos.x + dx, _pos.y + dy));
        }
        return _selected;
    }

    static bool PerlinMask(Vector2Int p, float _scale, float _threshold, Vector2 _offset)
    {
        float v = Mathf.PerlinNoise(p.x * _scale + _offset.x, p.y * _scale + _offset.y);
        return v < Mathf.Clamp01(_threshold);
    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> _floorPositions, List<Vector2Int> _directionsList)
    {
        HashSet<Vector2Int> _wallPositions = new();
        foreach (var pos in _floorPositions)
        {
            foreach (var direction in _directionsList)
            {
                var _neighbourPos = pos + direction;
                if (!_floorPositions.Contains(_neighbourPos))
                    _wallPositions.Add(_neighbourPos);
            }
        }
        return _wallPositions;
    }

    private static int BuildCodeInt(Vector2Int _pos, HashSet<Vector2Int> _floor)
    {
        int _code = 0;
        foreach (var _dir in Direction2D.eightDirectionsList)
        {
            var n = _pos + _dir;
            _code = (_code << 1) | (_floor.Contains(n) ? 1 : 0);
        }
        return _code;
    }

    private static bool Has(int _code, int _idxLeftToRight)
    {
        int _bit = 7 - _idxLeftToRight;
        return (_code & (1 << _bit)) != 0;
    }

    private static int CountCardinalWalls(Vector2Int _pos, HashSet<Vector2Int> _walls)
    {
        int _c = 0;
        if (_walls.Contains(_pos + Vector2Int.up)) _c++;
        if (_walls.Contains(_pos + Vector2Int.right)) _c++;
        if (_walls.Contains(_pos + Vector2Int.down)) _c++;
        if (_walls.Contains(_pos + Vector2Int.left)) _c++;
        return _c;
    }

    private static int CountCardinalFloors(int _code)
    {
        int _c = 0;
        if (Has(_code, 0)) _c++; // U
        if (Has(_code, 2)) _c++; // R
        if (Has(_code, 4)) _c++; // D
        if (Has(_code, 6)) _c++; // L
        return _c;
    }
}
