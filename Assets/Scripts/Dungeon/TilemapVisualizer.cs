using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase floorTile;
    [SerializeField]
    private TileBase wallTop, wallSideRight, wallSideLeft, wallBottom, wallFull, wallTopVariant, wallSideRightVariant, wallSideLeftVariant, wallBottomVariant,
        wallTopDamaged, wallSideRightDamaged, wallSideLeftDamaged, wallBottomDamaged,
        wallInnerCornerUpRight, wallInnerCornerUpLeft, wallInnerCornerDownRight, wallInnerCornerDownLeft,
        wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;

    public void PaintFloorTiles(IEnumerable<Vector2Int> _floorPositions)
    {
        PaintTiles(_floorPositions, floorTilemap, floorTile);
    }

    public void PaintWall(Vector2Int _pos, string _binaryType, bool _variant, bool _damaged)
    {
        int _type = Convert.ToInt32(_binaryType, 2);
        TileBase _tile = _type switch
        {
            var t when WallTypesHelper.wallTop.Contains(t)
                => ChooseTile(_variant, _damaged, wallTop, wallTopVariant, wallTopDamaged),

            var t when WallTypesHelper.wallSideLeft.Contains(t)
                => ChooseTile(_variant, _damaged, wallSideLeft, wallSideLeftVariant, wallSideLeftDamaged), // fixed damaged case

            var t when WallTypesHelper.wallSideRight.Contains(t)
                => ChooseTile(_variant, _damaged, wallSideRight, wallSideRightVariant, wallSideRightDamaged),

            var t when WallTypesHelper.wallBottom.Contains(t)
                => ChooseTile(_variant, _damaged, wallBottom, wallBottomVariant, wallBottomDamaged),

            var t when WallTypesHelper.wallInnerCornerDownLeft.Contains(t) => wallInnerCornerDownLeft,
            var t when WallTypesHelper.wallInnerCornerDownRight.Contains(t) => wallInnerCornerDownRight,
            var t when WallTypesHelper.wallInnerCornerUpRight.Contains(t) => wallInnerCornerUpRight,
            var t when WallTypesHelper.wallInnerCornerUpLeft.Contains(t) => wallInnerCornerUpLeft,

            var t when WallTypesHelper.wallDiagonalCornerDownRight.Contains(t) => wallDiagonalCornerDownRight,
            var t when WallTypesHelper.wallDiagonalCornerDownLeft.Contains(t) => wallDiagonalCornerDownLeft,
            var t when WallTypesHelper.wallDiagonalCornerUpRight.Contains(t) => wallDiagonalCornerUpRight,
            var t when WallTypesHelper.wallDiagonalCornerUpLeft.Contains(t) => wallDiagonalCornerUpLeft,

            var t when WallTypesHelper.wallFull.Contains(t) => wallFull,
            _ => null
        };

        if (_tile != null)
            PaintTile(wallTilemap, _tile, _pos);
    }

    public void PaintSecretEntrance(Vector2Int _pos, Vector2Int _outward, bool _damaged = true, bool _variant = false)
    {
        TileBase tile = null;

        if (_outward == Vector2Int.up) tile = _damaged ? wallTopDamaged : (_variant ? wallTopVariant : wallTop);
        else if (_outward == Vector2Int.down) tile = _damaged ? wallBottomDamaged : (_variant ? wallBottomVariant : wallBottom);
        else if (_outward == Vector2Int.right) tile = _damaged ? wallSideRightDamaged : (_variant ? wallSideRightVariant : wallSideRight);
        else if (_outward == Vector2Int.left) tile = _damaged ? wallSideLeftDamaged : (_variant ? wallSideLeftVariant : wallSideLeft);

        if (tile != null)
            PaintTile(wallTilemap, tile, _pos);
    }
    public void ClearWallAt(Vector2Int pos)
    {
        var cell = wallTilemap.WorldToCell((Vector3Int)pos);
        wallTilemap.SetTile(cell, null);
    }

    private static TileBase ChooseTile(bool _variant, bool _damaged, TileBase _normal, TileBase _variantTile, TileBase _damagedTile)
    {
        if (_variant && _variantTile != null) return _variantTile;
        if (_damaged && _damagedTile != null) return _damagedTile;
        return _normal;
    }

    private void PaintTiles(IEnumerable<Vector2Int> _positions, Tilemap _tilemap, TileBase _tile)
    {
        foreach (var _position in _positions)
        {
            PaintTile(_tilemap, _tile, _position);
        }
    }

    private void PaintTile(Tilemap _tilemap, TileBase _tile, Vector2Int _pos)
    {
        var _tilePos = _tilemap.WorldToCell((Vector3Int)_pos);
        _tilemap.SetTile(_tilePos, _tile);
    }

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }
}
