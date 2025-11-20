using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer
{
    private Tilemap _floorTilemap;
    private Tilemap _wallTilemap;
    private TileBase _floorTile;
    private TileBase _wallTop, _wallSideRight, _wallSideLeft, _wallBottom, _wallFull, _wallTopVariant, _wallSideRightVariant, _wallSideLeftVariant, _wallBottomVariant,
        _wallTopDamaged, _wallSideRightDamaged, _wallSideLeftDamaged, _wallBottomDamaged,
        _wallInnerCornerUpRight, _wallInnerCornerUpLeft, _wallInnerCornerDownRight, _wallInnerCornerDownLeft,
        _wallDiagonalCornerDownRight, _wallDiagonalCornerDownLeft, _wallDiagonalCornerUpRight, _wallDiagonalCornerUpLeft;

    public TilemapVisualizer(Tilemap floorMap, Tilemap wallMap)
    {
        _floorTilemap = floorMap;
        _wallTilemap = wallMap;

        _floorTile = LoadTile(TilePaths.Floor);
        _wallTop = LoadTile(TilePaths.WallTop);
        _wallSideRight = LoadTile(TilePaths.WallSideRight);
        _wallSideLeft = LoadTile(TilePaths.WallSideLeft);
        _wallBottom = LoadTile(TilePaths.WallBottom);
        _wallFull = LoadTile(TilePaths.WallFull);
        _wallTopVariant = LoadTile(TilePaths.WallTopVariant);
        _wallSideRightVariant = LoadTile(TilePaths.WallSideRightVariant);
        _wallSideLeftVariant = LoadTile(TilePaths.WallSideLeftVariant);
        _wallBottomVariant = LoadTile(TilePaths.WallBottomVariant);
        _wallTopDamaged = LoadTile(TilePaths.WallTopDamaged);
        _wallSideRightDamaged = LoadTile(TilePaths.WallSideRightDamaged);
        _wallSideLeftDamaged = LoadTile(TilePaths.WallSideLeftDamaged);
        _wallBottomDamaged = LoadTile(TilePaths.WallBottomDamaged);
        _wallInnerCornerUpRight = LoadTile(TilePaths.InnerCornerUpRight);
        _wallInnerCornerUpLeft = LoadTile(TilePaths.InnerCornerUpLeft);
        _wallInnerCornerDownRight = LoadTile(TilePaths.InnerCornerDownRight);
        _wallInnerCornerDownLeft = LoadTile(TilePaths.InnerCornerDownLeft);
        _wallDiagonalCornerDownRight = LoadTile(TilePaths.DiagonalCornerDownRight);
        _wallDiagonalCornerDownLeft = LoadTile(TilePaths.DiagonalCornerDownLeft);
        _wallDiagonalCornerUpRight = LoadTile(TilePaths.DiagonalCornerUpRight);
        _wallDiagonalCornerUpLeft = LoadTile(TilePaths.DiagonalCornerUpLeft);
    }

    private TileBase LoadTile(string path)
    {
        var tile = Resources.Load(path, typeof(TileBase)) as TileBase;
        if (tile == null)
            Debug.LogError($"[TilemapVisualizer] Failed to load tile at '{path}'.");
        return tile;
    }

    public void PaintFloorTiles(IEnumerable<Vector2Int> _floorPositions)
    {
        PaintTiles(_floorPositions, _floorTilemap, _floorTile);
    }

    public void PaintWall(Vector2Int _pos, string _binaryType, bool _variant, bool _damaged)
    {
        int _type = Convert.ToInt32(_binaryType, 2);
        TileBase _tile = _type switch
        {
            var t when WallTypesHelper.wallTop.Contains(t)
                => ChooseTile(_variant, _damaged, _wallTop, _wallTopVariant, _wallTopDamaged),

            var t when WallTypesHelper.wallSideLeft.Contains(t)
                => ChooseTile(_variant, _damaged, _wallSideLeft, _wallSideLeftVariant, _wallSideLeftDamaged), // fixed damaged case

            var t when WallTypesHelper.wallSideRight.Contains(t)
                => ChooseTile(_variant, _damaged, _wallSideRight, _wallSideRightVariant, _wallSideRightDamaged),

            var t when WallTypesHelper.wallBottom.Contains(t)
                => ChooseTile(_variant, _damaged, _wallBottom, _wallBottomVariant, _wallBottomDamaged),

            var t when WallTypesHelper.wallInnerCornerDownLeft.Contains(t) => _wallInnerCornerDownLeft,
            var t when WallTypesHelper.wallInnerCornerDownRight.Contains(t) => _wallInnerCornerDownRight,
            var t when WallTypesHelper.wallInnerCornerUpRight.Contains(t) => _wallInnerCornerUpRight,
            var t when WallTypesHelper.wallInnerCornerUpLeft.Contains(t) => _wallInnerCornerUpLeft,

            var t when WallTypesHelper.wallDiagonalCornerDownRight.Contains(t) => _wallDiagonalCornerDownRight,
            var t when WallTypesHelper.wallDiagonalCornerDownLeft.Contains(t) => _wallDiagonalCornerDownLeft,
            var t when WallTypesHelper.wallDiagonalCornerUpRight.Contains(t) => _wallDiagonalCornerUpRight,
            var t when WallTypesHelper.wallDiagonalCornerUpLeft.Contains(t) => _wallDiagonalCornerUpLeft,

            var t when WallTypesHelper.wallFull.Contains(t) => _wallFull,
            _ => null
        };

        if (_tile != null)
            PaintTile(_wallTilemap, _tile, _pos);
    }

    public void PaintSecretEntrance(Vector2Int _pos, Vector2Int _outward, bool _damaged = true, bool _variant = false)
    {
        TileBase tile = null;

        if (_outward == Vector2Int.up) tile = _damaged ? _wallTopDamaged : (_variant ? _wallTopVariant : _wallTop);
        else if (_outward == Vector2Int.down) tile = _damaged ? _wallBottomDamaged : (_variant ? _wallBottomVariant : _wallBottom);
        else if (_outward == Vector2Int.right) tile = _damaged ? _wallSideRightDamaged : (_variant ? _wallSideRightVariant : _wallSideRight);
        else if (_outward == Vector2Int.left) tile = _damaged ? _wallSideLeftDamaged : (_variant ? _wallSideLeftVariant : _wallSideLeft);

        if (tile != null)
            PaintTile(_wallTilemap, tile, _pos);
    }
    public void ClearWallAt(Vector2Int pos)
    {
        var cell = _wallTilemap.WorldToCell((Vector3Int)pos);
        _wallTilemap.SetTile(cell, null);
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
        _floorTilemap.ClearAllTiles();
        _wallTilemap.ClearAllTiles();
    }
}
