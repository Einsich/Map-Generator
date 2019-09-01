using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Navigation
{
    public const float townRadius = 0.71f;
    public const float townRadiusSqr = townRadius*townRadius;
    public const float armyRadius = 0.5f;
    public const float armyRadiusSqr = armyRadius * armyRadius;

    const int navCellSize = 5;
    public static Vector2[,] field;
     static List<NavAgent>[,] navList;
    static Vector2 bound;
    static int H, W;
    static int navListN, navListM;
    public static void Init(int h, int w)
    {
        field = new Vector2[h + 1, w + 1];
        H = h; W = w;
        bound = new Vector2(w - 0.01f, h - 0.01f);
        navList = new List<NavAgent>[navListN = (h + navCellSize - 1) / navCellSize, navListM = (w + navCellSize - 1) / navCellSize];
        for (int i = 0; i < navListN; i++)
            for (int j = 0; j < navListM; j++)
                navList[i, j] = new List<NavAgent>(4);
        WayPoints = new Stack<GameObject>();
    }
    static Vector2Int navIndex(Vector2 pos) => new Vector2Int(Mathf.Clamp((int)pos.y, 0, H - 1) / navCellSize, Mathf.Clamp((int)pos.x, 0, W - 1) / navCellSize);
    public static List<NavAgent>GetNavList(Vector2 pos)
    {
        var v = navIndex(pos);
        return navList[v.y, v.x];
    }
    public static void AddNavAgent(NavAgent agent)
    {
        GetNavList(agent.pos).Add(agent);
    }
    static bool Inside(int y, int x, Vector2 p)
    {
        if (y < 0 || y <= H || x < 0 || x < W)
            return false;
        bool Lefter(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x < 0;
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


    static Stack<GameObject> WayPoints;
    public static GameObject GetWayPoint()
    {
        var p = WayPoints.Count > 0 ? WayPoints.Pop() : GameObject.Instantiate(Main.instance.WayPoint);
        p.SetActive(true);
        return p;
    }
    public static void SetWayPoint(GameObject point)
    {
        point.SetActive(false);
        WayPoints.Push(point);
    }
    public static void CalculateTownCollide(NavAgent agent)
    {
        
        Vector2 p = agent.pos;
        Vector2Int cell = NavAgent.ToInt(p);
        Region region = agent.lastCollidedTown;
        bool Collide()
        {
            if (region.curOwner == agent.owner)
                return false;
            if ((region.pos - p).sqrMagnitude < townRadiusSqr)
            {
                agent.pos = region.pos + (p - region.pos).normalized * townRadius;
                if (agent.lastCollidedTown != region)
                {
                    agent.lastCollidedTown = region;
                    if (agent.target == region)
                    {
                        agent.CatchTarget();
                        region.WasCatch(agent.army);
                    }
                }

                return true;
            }
            return false;
        }
        if (region != null)
            if (Collide())
                return;
        region = MapMetrics.GetRegion(cell);
        if (region.Capital == cell)
            if (Collide())
                return;
        foreach (var d in MapMetrics.OctoDelta)
            if ((region = MapMetrics.GetRegion(cell + d)).Capital == cell + d)
                if (Collide())
                    return;
        agent.lastCollidedTown = null;
    }
    public static void CalculateArmyCollide(NavAgent agent)
    {
        Vector2 p = agent.pos;
        Vector2Int cell = navIndex(p);
        void Collide(NavAgent other)
        {
            if (other == agent)
                return;
            if ((other.pos - p).sqrMagnitude <= armyRadiusSqr)
            {
                p = other.pos + (p - other.pos).normalized * armyRadius;
                if (agent.target == (object)other.army)
                {
                    Debug.Log("catch");
                    agent.CatchTarget();
                }
                other.army.WasCatch(agent.army);
            }
        }
        bool InsideNavList(int x, int y) => 0 <= x && x < navListM && 0 <= y && y < navListN;
        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
                if (InsideNavList(cell.x + x, cell.y + y))
                    foreach (var other in navList[cell.y + y, cell.x + x])
                        Collide(other);
        agent.pos = p;
    }
    
}
