using System.Collections.Generic;

public static class WallTypesHelper
{
    // Bit order used throughout (matches WallGenerator's string build order):
    // Index:  0   1   2   3   4   5   6   7
    // Dir:    U  UR   R  DR   D  DL   L  UL
    //
    // So the binary literal 0b00011100 corresponds to DR=1, D=1, DL=1.

    public static readonly HashSet<int> wallTop = new();               // D = 1, only this cardinal
    public static readonly HashSet<int> wallSideLeft = new();           // R = 1, only this cardinal
    public static readonly HashSet<int> wallSideRight = new();          // L = 1, only this cardinal
    public static readonly HashSet<int> wallBottom = new();             // U = 1, only this cardinal

    public static readonly HashSet<int> wallInnerCornerDownLeft = new();  // D & L
    public static readonly HashSet<int> wallInnerCornerDownRight = new(); // D & R
    public static readonly HashSet<int> wallInnerCornerUpRight = new();   // U & R
    public static readonly HashSet<int> wallInnerCornerUpLeft = new();    // U & L

    public static readonly HashSet<int> wallDiagonalCornerDownLeft = new();  // only DL
    public static readonly HashSet<int> wallDiagonalCornerDownRight = new(); // only DR
    public static readonly HashSet<int> wallDiagonalCornerUpLeft = new();    // only UL
    public static readonly HashSet<int> wallDiagonalCornerUpRight = new();   // only UR

    public static readonly HashSet<int> wallFull = new(); // everything else (catch-all)

    static WallTypesHelper()
    {
        for (int _code = 0; _code <= 0b11111111; _code++)
        {
            // Read bits in the same left->right order as the built string:
            // leftmost bit (bit 7) == U (index 0), rightmost bit (bit 0) == UL (index 7).
            bool U = IsSet(_code, 0);
            bool UR = IsSet(_code, 1);
            bool R = IsSet(_code, 2);
            bool DR = IsSet(_code, 3);
            bool D = IsSet(_code, 4);
            bool DL = IsSet(_code, 5);
            bool L = IsSet(_code, 6);
            bool UL = IsSet(_code, 7);

            int _cardinalCount = CountTrue(U, R, D, L);
            int _diagCount = CountTrue(UR, DR, DL, UL);

            // 1) Straight walls: exactly one cardinal neighbor
            if (_cardinalCount == 1)
            {
                if (D) { wallTop.Add(_code); continue; }      // "tile is above a floor"
                if (U) { wallBottom.Add(_code); continue; }   // "tile is below a floor"
                if (R) { wallSideLeft.Add(_code); continue; } // "tile is left of a floor"
                if (L) { wallSideRight.Add(_code); continue; } // "tile is right of a floor"
            }

            // 2) Inner corners: two adjacent cardinals
            if (_cardinalCount == 2)
            {
                if (D && L) { wallInnerCornerDownLeft.Add(_code); continue; }
                if (D && R) { wallInnerCornerDownRight.Add(_code); continue; }
                if (U && R) { wallInnerCornerUpRight.Add(_code); continue; }
                if (U && L) { wallInnerCornerUpLeft.Add(_code); continue; }

                // Opposite cardinals (U&D) or (L&R) fall through to wallFull.
            }

            // 3) Diagonal-only corners: zero cardinals, exactly one diagonal
            if (_cardinalCount == 0 && _diagCount == 1)
            {
                if (DL) { wallDiagonalCornerDownLeft.Add(_code); continue; }
                if (DR) { wallDiagonalCornerDownRight.Add(_code); continue; }
                if (UL) { wallDiagonalCornerUpLeft.Add(_code); continue; }
                if (UR) { wallDiagonalCornerUpRight.Add(_code); continue; }
            }

            // 4) Everything else → wallFull (T-junctions, crossings, opposites, multiple diags, or empty)
            wallFull.Add(_code);
        }

        // (Optional sanity checks in editor)
        // Debug.Log($"Top:{wallTop.Count} Bottom:{wallBottom.Count} L:{wallSideLeft.Count} R:{wallSideRight.Count} " +
        //           $"IC DL:{wallInnerCornerDownLeft.Count} DR:{wallInnerCornerDownRight.Count} UR:{wallInnerCornerUpRight.Count} UL:{wallInnerCornerUpLeft.Count} " +
        //           $"Diag DL:{wallDiagonalCornerDownLeft.Count} DR:{wallDiagonalCornerDownRight.Count} UL:{wallDiagonalCornerUpLeft.Count} UR:{wallDiagonalCornerUpRight.Count} " +
        //           $"Full:{wallFull.Count} Total:{TotalUnique()}");
    }

    private static bool IsSet(int _code, int _idxLeftToRight)
    {
        // Map left-to-right index (U..UL) to actual bit index (7..0).
        int _bit = 7 - _idxLeftToRight;
        return (_code & (1 << _bit)) != 0;
    }

    private static int CountTrue(params bool[] _vals)
    {
        int _count = 0;
        for (int i = 0; i < _vals.Length; i++) if (_vals[i]) _count++;
        return _count;
    }

    // If you turn the debug logs on, this can help confirm coverage equals 256.
    private static int TotalUnique()
    {
        var _seen = new HashSet<int>();
        _seen.UnionWith(wallTop);
        _seen.UnionWith(wallBottom);
        _seen.UnionWith(wallSideLeft);
        _seen.UnionWith(wallSideRight);
        _seen.UnionWith(wallInnerCornerDownLeft);
        _seen.UnionWith(wallInnerCornerDownRight);
        _seen.UnionWith(wallInnerCornerUpLeft);
        _seen.UnionWith(wallInnerCornerUpRight);
        _seen.UnionWith(wallDiagonalCornerDownLeft);
        _seen.UnionWith(wallDiagonalCornerDownRight);
        _seen.UnionWith(wallDiagonalCornerUpLeft);
        _seen.UnionWith(wallDiagonalCornerUpRight);
        _seen.UnionWith(wallFull);
        return _seen.Count;
    }
}
