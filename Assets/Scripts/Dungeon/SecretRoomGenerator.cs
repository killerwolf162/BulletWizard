using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Data describing a successfully spawned secret room.
/// Use Center to place gameplay items
/// </summary>
public readonly struct SecretRoom
{
    public readonly Vector2Int Center;
    public readonly IReadOnlyList<Vector2Int> FloorCells;
    public readonly Vector2Int EntranceWall;
    public readonly Vector2Int EntranceDirection;

    public SecretRoom(Vector2Int center, IReadOnlyList<Vector2Int> floorCells, Vector2Int entranceWall, Vector2Int entranceDirection)
    {
        Center = center;
        FloorCells = floorCells;
        EntranceWall = entranceWall;
        EntranceDirection = entranceDirection;
    }
}

public class SecretRoomGenerator
{
    public IReadOnlyList<SecretRoom> SpawnedRooms => _spawnedRooms;

    private readonly TilemapVisualizer _tilemapVisualizer;
    private readonly WallGenerationParameters _wallParams;
    private readonly SecretRoomParameters _roomParams;
    private readonly List<SecretRoom> _spawnedRooms = new();

    public SecretRoomGenerator(TilemapVisualizer tilemapVisualizer, WallGenerationParameters wallParams, SecretRoomParameters roomParams)
    {
        _tilemapVisualizer = tilemapVisualizer;
        _wallParams = wallParams;
        _roomParams = roomParams;
    }

    public void Run(HashSet<Vector2Int> floor, HashSet<Vector2Int> walls, Vector2 perlinOffset)
    {
        _spawnedRooms.Clear();

        if (!_roomParams.enableSecretRooms || _roomParams.maxCount == 0 || walls.Count == 0)
            return;

        //prefer walls that are already marked as "damaged" by Perlin
        var candidates = BuildCandidates(floor, walls, perlinOffset, damagedOnly: true);
        FisherYatesShuffle.Shuffle(candidates);
        TrySpawnRooms(floor, walls, perlinOffset, candidates);

        //if minimum not reached, try any wall
        if (_spawnedRooms.Count < _roomParams.minCount)
        {
            var fallback = BuildCandidates(floor, walls, perlinOffset, damagedOnly: false);
            FisherYatesShuffle.Shuffle(fallback);
            TrySpawnRooms(floor, walls, perlinOffset, fallback);
        }
    }

    private List<(Vector2Int wall, Vector2Int outward)> BuildCandidates(HashSet<Vector2Int> floor, HashSet<Vector2Int> walls, Vector2 perlinOffset, bool damagedOnly)
    {
        var candidates = new List<(Vector2Int, Vector2Int)>();

        foreach (var wall in walls)
        {
            if (damagedOnly && !PerlinMask(wall, _wallParams.damagedNoiseScale, _wallParams.damagedPercent, perlinOffset))
                continue;

            foreach (var dir in Direction2D.cardinalDirections)
            {
                var inside = wall - dir;
                var outside = wall + dir;
                if (floor.Contains(inside) && !floor.Contains(outside))
                    candidates.Add((wall, dir));
            }
        }
        return candidates;
    }

    private void TrySpawnRooms(HashSet<Vector2Int> floor, HashSet<Vector2Int> walls, Vector2 perlinOffset, List<(Vector2Int wall, Vector2Int outward)> candidates)
    {
        foreach (var (wall, outward) in candidates)
        {
            if (_spawnedRooms.Count >= _roomParams.maxCount) break;

            // Skip if this wall was already converted to floor by an earlier room
            if (!walls.Contains(wall)) continue;

            if (!TryCarveRoom(floor, wall, outward, out var carved)) continue;

            //Open the entrance wall
            floor.Add(wall);
            walls.Remove(wall);
            _tilemapVisualizer.ClearWallAt(wall);
            _tilemapVisualizer.PaintFloorTiles(new[] { wall });

            //Add carved cells to the floor
            floor.UnionWith(carved);
            _tilemapVisualizer.PaintFloorTiles(carved);

            //Remove any newly-interior cells from the wall set
            RemoveStaleWalls(floor, walls, wall, carved);

            //Paint walls around the new room only
            PaintSecretRoomWalls(floor, walls, carved);

            //Record the room
            var center = ComputeCenter(carved);
            _spawnedRooms.Add(new SecretRoom(center, carved.ToList(), wall, outward));
        }
    }

    private bool TryCarveRoom(HashSet<Vector2Int> floor, Vector2Int wall, Vector2Int outward, out HashSet<Vector2Int> roomCells)
    {
        roomCells = null;

        int width = Random.Range(_roomParams.minWidth, _roomParams.maxWidth + 1);
        int height = Random.Range(_roomParams.minHeight, _roomParams.maxHeight + 1);
        int corLen = Random.Range(_roomParams.corridorMinLength, _roomParams.corridorMaxLength + 1);
        int corWidth = Mathf.Max(1, _roomParams.corridorWidth);

        RectInt corridor = BuildCorridorRect(wall, outward, corLen, corWidth);
        RectInt room = BuildRoomRectFromCorridorEnd(corridor, outward, width, height);
        RectInt unionRect = RectUnion(corridor, room);

        if (!IsAreaClear(floor, unionRect, _roomParams.padding))
            return false;

        var cells = new HashSet<Vector2Int>();
        AddRectCells(corridor, cells);
        AddRectCells(room, cells);
        roomCells = cells;
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
            int xMin = corridor.xMin + corridor.width / 2 - w / 2;
            return new RectInt(xMin, corridor.yMax, w, h);
        }
        if (outward == Vector2Int.down)
        {
            int xMin = corridor.xMin + corridor.width / 2 - w / 2;
            return new RectInt(xMin, corridor.yMin - h, w, h);
        }
        if (outward == Vector2Int.right)
        {
            int yMin = corridor.yMin + corridor.height / 2 - h / 2;
            return new RectInt(corridor.xMax, yMin, w, h);
        }
        else // left
        {
            int yMin = corridor.yMin + corridor.height / 2 - h / 2;
            return new RectInt(corridor.xMin - w, yMin, w, h);
        }
    }

    private static void AddRectCells(RectInt r, HashSet<Vector2Int> set)
    {
        for (int x = r.xMin; x < r.xMax; x++)
            for (int y = r.yMin; y < r.yMax; y++)
                set.Add(new Vector2Int(x, y));
    }

    private static RectInt RectUnion(RectInt a, RectInt b)
    {
        int xMin = Mathf.Min(a.xMin, b.xMin);
        int yMin = Mathf.Min(a.yMin, b.yMin);
        int xMax = Mathf.Max(a.xMax, b.xMax);
        int yMax = Mathf.Max(a.yMax, b.yMax);
        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private static bool IsAreaClear(HashSet<Vector2Int> floor, RectInt area, int padding)
    {
        for (int x = area.xMin - padding; x <= area.xMax + padding; x++)
            for (int y = area.yMin - padding; y <= area.yMax + padding; y++)
                if (floor.Contains(new Vector2Int(x, y)))
                    return false;
        return true;
    }

    private static void RemoveStaleWalls(HashSet<Vector2Int> floor, HashSet<Vector2Int> walls, Vector2Int entranceWall, HashSet<Vector2Int> carved)
    {
        // Collect every cell adjacent to the carved set
        var border = new HashSet<Vector2Int>();
        border.Add(entranceWall); // already converted but check neighbours around it

        foreach (var cell in carved)
            foreach (var dir in Direction2D.eightDirectionsList)
                border.Add(cell + dir);

        // Any border cell that is currently in walls but has no non-floor neighbour
        // is now an interior cell — it should not be a wall anymore.
        foreach (var candidate in border)
        {
            if (!walls.Contains(candidate)) continue;

            // A wall cell is still a real wall if at least one 8-neighbour is not floor
            bool stillWall = false;
            foreach (var dir in Direction2D.eightDirectionsList)
            {
                if (!floor.Contains(candidate + dir)) { stillWall = true; break; }
            }

            if (!stillWall)
                walls.Remove(candidate);
        }
    }

    private void PaintSecretRoomWalls(HashSet<Vector2Int> allFloor, HashSet<Vector2Int> mainWalls, HashSet<Vector2Int> newFloor)
    {
        // Derive walls from only the new floor cells
        var secretWalls = new HashSet<Vector2Int>();
        foreach (var cell in newFloor)
            foreach (var dir in Direction2D.eightDirectionsList)
            {
                var neighbour = cell + dir;
                if (!allFloor.Contains(neighbour))
                    secretWalls.Add(neighbour);
            }

        // Don't re-paint cells that are already handled by the main wall pass
        secretWalls.ExceptWith(mainWalls);

        // Build variant / damaged sets using the same parameters as the main walls
        var wallList = secretWalls.ToList();
        var variantSet = SampleByPercent(wallList, _wallParams.variantPercent, _wallParams.variantMinChebyshevDistance);

        var damagedSet = new HashSet<Vector2Int>();

        // Secret room walls intentionally skip the Perlin damage mask — their
        // own walls are fresh stone. Remove this if you want consistent damage.
        if (_wallParams.exclusive) damagedSet.ExceptWith(variantSet);

        foreach (var pos in secretWalls)
        {
            string bits = "";
            foreach (var dir in Direction2D.eightDirectionsList)
                bits += allFloor.Contains(pos + dir) ? "1" : "0";

            _tilemapVisualizer.PaintWall(pos, bits, variantSet.Contains(pos),
                                         damagedSet.Contains(pos));
        }

        // Merge into main walls so later passes know these exist
        mainWalls.UnionWith(secretWalls);
    }

    private static Vector2Int ComputeCenter(HashSet<Vector2Int> cells)
    {
        var avg = Vector2.zero;
        foreach (var c in cells) avg += (Vector2)c;
        avg /= cells.Count;
        return Vector2Int.RoundToInt(avg);
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
}