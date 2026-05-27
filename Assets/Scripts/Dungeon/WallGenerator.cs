using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class WallGenerator
{
    public static void CreateWalls( HashSet<Vector2Int> floor, TilemapVisualizer tilemapVisualizer, WallGenerationParameters wallParams)
    {
        //collapse wallFull cells
        while (true)
        {
            var walls = FindWallsInDirections(floor, Direction2D.eightDirectionsList);

            bool hasWallFull = false;
            var toFloor = new HashSet<Vector2Int>();

            foreach (var pos in walls)
            {
                int code = BuildCodeInt(pos, floor);
                if (!WallTypesHelper.wallFull.Contains(code)) continue;

                hasWallFull = true;

                int wallNbrs = CountCardinalWalls(pos, walls);
                if (wallNbrs >= 2) continue;

                int cardFloors = CountCardinalFloors(code);
                if (cardFloors == 0 || cardFloors >= 2 || wallNbrs == 0)
                    toFloor.Add(pos);
            }

            if (!hasWallFull) break;

            if (toFloor.Count == 0)
            {
                // safety: convert every remaining wallFull to floor to avoid an infinite loop on unusual room layouts
                foreach (var pos in walls)
                {
                    int code = BuildCodeInt(pos, floor);
                    if (WallTypesHelper.wallFull.Contains(code))
                        toFloor.Add(pos);
                }
            }

            floor.UnionWith(toFloor);
            tilemapVisualizer.PaintFloorTiles(toFloor);
        }

        //paint all walls
        Vector2 perlinOffset = wallParams.randomizePerlinOffsetEachRun
            ? new Vector2(Random.value * 1000f, Random.value * 1000f)
            : wallParams.perlinOffset;

        var finalWalls = FindWallsInDirections(floor, Direction2D.eightDirectionsList);
        PaintWalls(tilemapVisualizer, finalWalls, floor, wallParams, perlinOffset);
    }

    public static Vector2 BuildPerlinOffset(WallGenerationParameters wallParams)
    {
        return wallParams.randomizePerlinOffsetEachRun
            ? new Vector2(Random.value * 1000f, Random.value * 1000f)
            : wallParams.perlinOffset;
    }

    private static void PaintWalls( TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> walls, HashSet<Vector2Int> floor, WallGenerationParameters parameters, Vector2 offset)
    {
        var wallList = walls.ToList();
        var variantSet = SampleByPercent(wallList, parameters.variantPercent, parameters.variantMinChebyshevDistance);

        var damagedSet = new HashSet<Vector2Int>();
        foreach (var pos in wallList)
            if (PerlinMask(pos, parameters.damagedNoiseScale, parameters.damagedPercent, offset))
                damagedSet.Add(pos);

        if (parameters.exclusive)
            damagedSet.ExceptWith(variantSet);

        foreach (var pos in walls)
        {
            string bits = "";
            foreach (var dir in Direction2D.eightDirectionsList)
                bits += floor.Contains(pos + dir) ? "1" : "0";

            tilemapVisualizer.PaintWall(pos, bits,
                variantSet.Contains(pos), damagedSet.Contains(pos));
        }
    }

    public static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionsList)
    {
        var wallPositions = new HashSet<Vector2Int>();
        foreach (var pos in floorPositions)
            foreach (var dir in directionsList)
            {
                var neighbour = pos + dir;
                if (!floorPositions.Contains(neighbour))
                    wallPositions.Add(neighbour);
            }
        return wallPositions;
    }

    private static HashSet<Vector2Int> SampleByPercent(List<Vector2Int> walls, float percent, int minDistance)
    {
        percent = Mathf.Clamp01(percent);
        int target = Mathf.Clamp(Mathf.RoundToInt(walls.Count * percent), 0, walls.Count);
        int radius = Mathf.Max(0, minDistance - 1);

        var indices = Enumerable.Range(0, walls.Count).ToList();
        FisherYatesShuffle.Shuffle(indices);

        var selected = new HashSet<Vector2Int>();
        var blocked = new HashSet<Vector2Int>();

        foreach (int idx in indices)
        {
            if (selected.Count >= target) break;
            var pos = walls[idx];
            if (blocked.Contains(pos)) continue;
            selected.Add(pos);
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                    blocked.Add(new Vector2Int(pos.x + dx, pos.y + dy));
        }
        return selected;
    }

    private static bool PerlinMask(Vector2Int p, float scale, float threshold, Vector2 offset)
    {
        float v = Mathf.PerlinNoise(p.x * scale + offset.x, p.y * scale + offset.y);
        return v < Mathf.Clamp01(threshold);
    }

    private static int BuildCodeInt(Vector2Int pos, HashSet<Vector2Int> floor)
    {
        int code = 0;
        foreach (var dir in Direction2D.eightDirectionsList)
            code = (code << 1) | (floor.Contains(pos + dir) ? 1 : 0);
        return code;
    }

    private static bool Has(int code, int idxLeftToRight)
    {
        int bit = 7 - idxLeftToRight;
        return (code & (1 << bit)) != 0;
    }

    private static int CountCardinalWalls(Vector2Int pos, HashSet<Vector2Int> walls)
    {
        int c = 0;
        if (walls.Contains(pos + Vector2Int.up)) c++;
        if (walls.Contains(pos + Vector2Int.right)) c++;
        if (walls.Contains(pos + Vector2Int.down)) c++;
        if (walls.Contains(pos + Vector2Int.left)) c++;
        return c;
    }

    private static int CountCardinalFloors(int code)
    {
        int c = 0;
        if (Has(code, 0)) c++; // U
        if (Has(code, 2)) c++; // R
        if (Has(code, 4)) c++; // D
        if (Has(code, 6)) c++; // L
        return c;
    }
}