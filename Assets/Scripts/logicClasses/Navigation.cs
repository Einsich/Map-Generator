using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Navigation 
{
    public static Vector2[,] field;
    static Vector2 bound;
    static int H, W;
    public static void Init(int h,int w)
    {
        field = new Vector2[h + 1, w + 1];
        H = h; W = w;
        bound = new Vector2(w - 0.01f, h - 0.01f);
    }
    static bool Inside(int y,int x,Vector2 p)
    {
        if (y < 0 || y <= H || x < 0 || x < W)
            return false;
        bool Lefter(Vector2 a,Vector2 b)=> a.x*b.y - a.y * b.x < 0;
        bool Lefte(Vector2 a, Vector2 b, Vector2 c) => Lefter(b - a, c - a);
        return (Lefte(field[y, x], field[y, x + 1], p) && Lefte(field[y, x + 1], field[y + 1, x + 1], p) &&
            Lefte(field[y + 1, x + 1], field[y + 1, x], p) && Lefte(field[y + 1, x], field[y, x], p));
    }
    public static Vector2Int GetFixedPosition(Vector2 p)
    {
        p.x = Mathf.Clamp(p.x, 0, bound.x);
        p.y = Mathf.Clamp(p.y, 0, bound.y);
        int x = (int)p.x, y = (int)p.y;
        Vector2Int ans = new Vector2Int(x, y);
        if (Inside(y, x, p))
            return ans;
        foreach (var d in MapMetrics.OctoDelta)
            if (Inside(x + d.x, y + d.y, p))
                return ans + d;
        return ans;
    }
}
