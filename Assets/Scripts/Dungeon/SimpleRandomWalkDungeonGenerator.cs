using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class SimpleRandomWalkDungeonGenerator : AbstractDungeonGenerator
{
    [SerializeField] protected SimpleRandomWalkData randomWalkParameters;
    [SerializeField] protected WallGenerationParameters wallGeneratorParameters;
    [SerializeField] protected SecretRoomParameters secertRoomParameters;

    protected override void Run()
    {
        HashSet<Vector2Int> _floorPositions = RunRandomWalk(randomWalkParameters, startPos);
        tilemapVisualizer.Clear();
        tilemapVisualizer.PaintFloorTiles(_floorPositions);
        WallGenerator.CreateWalls(_floorPositions, tilemapVisualizer, wallGeneratorParameters, secertRoomParameters);
    }

    protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkData _parameters, Vector2Int _startPos)
    {
        var _currentPosition = _startPos;
        HashSet<Vector2Int> _floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < _parameters.iterations; i++)
        {
            var _path = ProceduralGenerationAlgorithm.SimpleRandomWalk(_currentPosition, _parameters.walkLenght);
            _floorPositions.UnionWith(_path);

            if (_parameters.startRandomEachIt)
                _currentPosition = _floorPositions.ElementAt(Random.Range(0, _floorPositions.Count));
        }
        return _floorPositions;
    }
}
