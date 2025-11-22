using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathfinder
{
    private readonly Tilemap _floorMap;
    private readonly Tilemap _wallMap;

    private struct Node
    {
        public Vector3Int Cell;
        public float G;
        public float H;
        public float F => G + H;
        public Vector3Int Parent;
        public bool HasParent;
    }

    public AStarPathfinder(Tilemap floormap, Tilemap wallMap)
    {
        _floorMap = floormap;
        _wallMap = wallMap;
    }

    public List<Vector3> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        Debug.Log("Astar ran");

        if (_floorMap == null)
            return null;

        Vector3Int startCell = _floorMap.WorldToCell(startWorld);
        Vector3Int targetCell = _floorMap.WorldToCell(targetWorld);

        startCell = EnsureWalkable(startCell);
        targetCell = EnsureWalkable(targetCell);

        if (!IsWalkable(startCell) || !IsWalkable(targetCell))
            return null;

        var openList = new List<Node>();
        var closed = new HashSet<Vector3Int>();
        var nodes = new Dictionary<Vector3Int, Node>();

        Node startNode = new Node
        {
            Cell = startCell,
            G = 0,
            H = Heuristic(startCell, targetCell),
            Parent = Vector3Int.zero,
            HasParent = false
        };
        nodes[startCell] = startNode;
        openList.Add(startNode);

        while(openList.Count > 0)
        {
            int bestIdx = 0;
            float bestCost = openList[0].F;
            for(int i = 1; i < openList.Count; i++)
            {
                float f = openList[i].F;
                if(f < bestCost)
                {
                    bestCost = f;
                    bestIdx = i;
                }
            }
            Node current = openList[bestIdx];
            openList.RemoveAt(bestIdx);
            closed.Add(current.Cell);

            if(current.Cell == targetCell)
            {
                return ReconstructWorldPath(current, nodes);
            }

            foreach(Vector3Int neighbourCell in GetNeighbors(current.Cell))
            {
                if (closed.Contains(neighbourCell))
                    continue;
                if (!IsWalkable(neighbourCell))
                    continue;

                float tentativeG = current.G + 1f;
                if(!nodes.TryGetValue(neighbourCell, out Node neighbour))
                {
                    neighbour = new Node
                    {
                        Cell = neighbourCell,
                        G = float.PositiveInfinity,
                        H = Heuristic(neighbourCell, targetCell),
                        Parent = Vector3Int.zero,
                        HasParent = false
                    };
                }

                if (tentativeG >= neighbour.G)
                    continue;

                neighbour.G = tentativeG;
                neighbour.Parent = current.Cell;
                neighbour.HasParent = true;
                nodes[neighbourCell] = neighbour;

                bool inOpen = false;
                for(int i = 0; i < openList.Count; i++)
                {
                    if(openList[i].Cell == neighbourCell)
                    {
                        openList[i] = neighbour;
                        inOpen = true;
                        break;
                    }
                }
                if (!inOpen)
                    openList.Add(neighbour);
            }
        }

        return null;
    }

    private Vector3Int EnsureWalkable(Vector3Int cell)
    {
        if (IsWalkable(cell))
            return cell;

        const int radius = 2;
        float bestDist = float.PositiveInfinity;
        Vector3Int bestCell = cell;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                Vector3Int c = new Vector3Int(cell.x + dx, cell.y + dy, cell.z);
                if (!IsWalkable(c))
                    continue;
                float d = (c - cell).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    bestCell = c;
                }
            }
        }

        return bestCell;
    }

    private bool IsWalkable(Vector3Int cell)
    {
        if (_floorMap == null)
            return false;

        if (!_floorMap.cellBounds.Contains(cell))
            return false;

        bool hasFloor = _floorMap.HasTile(cell);
        bool hasWall = _wallMap != null && _wallMap.HasTile(cell);

        return hasFloor && !hasWall;
    }

    private static float Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private IEnumerable<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        yield return new Vector3Int(cell.x + 1, cell.y, cell.z);
        yield return new Vector3Int(cell.x - 1, cell.y, cell.z);
        yield return new Vector3Int(cell.x, cell.y + 1, cell.z);
        yield return new Vector3Int(cell.x, cell.y - 1, cell.z);
    }

    private List<Vector3> ReconstructWorldPath(Node endNode, Dictionary<Vector3Int, Node> nodes)
    {
        var pathCells = new List<Vector3Int>();
        Node current = endNode;
        pathCells.Add(current.Cell);

        while (current.HasParent && nodes.TryGetValue(current.Parent, out Node parent))
        {
            current = parent;
            pathCells.Add(current.Cell);
        }

        pathCells.Reverse();

        var pathWorld = new List<Vector3>(pathCells.Count);
        for (int i = 0; i < pathCells.Count; i++)
        {
            pathWorld.Add(_floorMap.GetCellCenterWorld(pathCells[i]));
        }

        return pathWorld;
    }
}

