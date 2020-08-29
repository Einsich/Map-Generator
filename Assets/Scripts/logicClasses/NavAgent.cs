using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour
{
    const float WayPointStep = 0.4f;
    public IFightable target;
    //public Vector2Int curCell => Navigation.GetFixedPosition(pos_);
    public Vector2Int curCell;
    Vector2 pos_;
    public Vector2 pos
    {
        get => pos_;
        set
        {
            var l1 = Navigation.GetNavList(pos_);
            var l2 = Navigation.GetNavList(value);
            if (l1 != l2)
            {
                Navigation.GetNavList(pos_).Remove(this);
                Navigation.GetNavList(value).Add(this);
            }
            pos_ = value;
        }
    }
    public State owner;
    public IMovable Movable;
    public bool InTown { get => Movable is Army a ? a.inTown : false; set { if (Movable is Army a) a.inTown = value; } }
    float needAngle, dAngle = 2.5f;
    public Region lastCollidedTown;
    public NavAgent lastCollidedAgent;
    public void SetToMovable(IMovable movable)
    {
        Movable = movable;
        owner = movable.curOwner ;
        Navigation.AddNavAgent(this);
        UpdateCellInformation(curCell);
    }
    public void ResetNavAgent() => Navigation.GetNavList(pos_).Remove(this);
    public TerrainType TerrainType { get; private set; }
    Vector2 next, nextpl, end;
    public Vector2 CollideForce;
    public List<Vector2Int> path;
    Vector2Int lastPathCell => path != null ?path.Count>0? path[path.Count - 1]:ToInt(end) : Vector2Int.zero;
    int pathIndex;
    private float SpeedLandCoef = 1;
    public Vector2 Speed { get; private set; }
    void FixedUpdate()
    {
        if (!Date.play)
            return;
        UpdateRotation();
        if (path == null)
            return;

        Vector2 direction = DirectionIndex();
        float delta = Time.fixedDeltaTime* GameTimer.timeScale * Movable.Speed * SpeedLandCoef;
        Speed = delta * direction;
        pos += Speed;

        needAngle = Vector3.SignedAngle(transform.forward, new Vector3(direction.x, 0, direction.y), Vector3.up);
        

        if ((pos - end).sqrMagnitude < 0.001f)
        {
            pos = end;
            if(target==null)
            Stop();
        }
        UpdateWayPoints();
        CollideForce = Vector2.zero;
        if (Movable is Army)
            Navigation.CalculateTownCollide(this);
        Navigation.CalculateArmyCollide(this);
        pos += CollideForce * Time.fixedDeltaTime * GameTimer.timeScale;
        if(CollideForce.magnitude > WayPointStep)
        {
            int j = wayIndex;
            float l = 100000000;
            for(int i = j;i<waypoints.Count;i++)
            {
                float d = (FromV3(waypoints[i].transform.position) - pos).sqrMagnitude;
                if(d < l)
                {
                    l = d;
                    j = i;
                }
            }
            wayIndex = j;
        }
        //direction = (pos - possave).normalized;
        //pos = possave +  direction * delta;
        Vector3 position = MapMetrics.RayCastedGround(pos);
        if(delta > 0 && (position - transform.position).sqrMagnitude < 0.000001f)
        {
            if (target == null)

                Stop();
            return;
        }
        transform.position = position;
        Vector2Int cell = Navigation.GetFixedPosition(pos);
        if (cell != curCell)
        {
            Movable.CellChange(curCell, cell);
            curCell = cell;
        }
        if (target != null)
        {
            if (target.curPosition != lastPathCell)
                MoveTo(target.position, target.curPosition);
        }
    }
    public void ShowPath(bool selected)
    {

        int i = 0;
        foreach (var x in waypoints)
            x.SetActive(selected && i++ >= wayIndex);
    }
    public static Vector2Int ToInt(Vector2 p) => new Vector2Int((int)p.x, (int)p.y);
    public static Vector2 FromV3(Vector3 p) => new Vector2(p.x, p.z);
    public void RotateTo(Vector2 enemy)
    {
        needAngle = Vector3.SignedAngle(transform.forward, new Vector3(enemy.x - pos_.x, 0, enemy.y - pos_.y), Vector3.up);
    }
    float lastDist;
    void UpdateWayPoints()
    {
        if (wayIndex >= waypoints.Count)
            return;
        Vector2 v = FromV3(waypoints[wayIndex].transform.position);
        float dist = (v - pos).sqrMagnitude;
        if (dist < 0.01f)
        {
            waypoints[wayIndex++].SetActive(false);
            UpdateCellInformation(Navigation.GetFixedPosition(v));
        }
    }
    void UpdateRotation()
    {
        //transform.LookAt(transform.position + new Vector3(v.x, 0, v.y), Vector3.up);
        if (Mathf.Abs(needAngle) > dAngle)
        {
            float d = Mathf.Abs(needAngle) * 0.1f * Mathf.Sign(needAngle);
            transform.Rotate(Vector3.up, d);
            needAngle -= d;
        }
    }
    private void UpdateCellInformation(Vector2Int cell)
    {

        SpeedLandCoef = 1f / Main.CellMoveCost(cell);
        TerrainType = Main.GetTerrainType(cell);
    }
    public void RecalculatePath()
    {
        if (path != null && MoveTo(end, lastPathCell))
        {

        }
        else
            Stop();
    }
    public bool MoveToTarget(IFightable target)
    {
        bool res = MoveTo(target.position, target.curPosition);
        if(res)
        {
            this.target = target;
        }
        return res;
    }
    public bool MoveTo(Vector2Int to) => MoveTo(to + Vector2.one * 0.5f, to);
    public bool MoveTo(Vector2 to, Vector2Int toInt)
    {
        List<Vector2Int> list = Main.FindPath(curCell, toInt, owner);
        if (list != null)
        {
            ClearWayPoints();
            BuildWayPoints(pos, list, to);
            path = list;
            pathIndex = 0;
            end = to;
            if (path.Count > 1)
            {
                next = path[0];
                nextpl = path[1];
                pathIndex = 2;
            }
            else
                if (path.Count > 0)
            {
                next = path[0];
                nextpl = to;
                pathIndex = 1;
            }
            else
                next = nextpl = to;
        }
        return path != null;
    }
    public void Stop()
    {
        ClearWayPoints();
        path = null;
        lastCollidedTown = null;
        Movable.Stop();
    }
    int wayIndex;
    List<GameObject> waypoints = new List<GameObject>();
    void ClearWayPoints()
    {
        foreach (GameObject wp in waypoints)
            Navigation.SetWayPoint(wp);
        waypoints.Clear();
    }
    void BuildWayPoints(Vector2 begin, List<Vector2Int> way, Vector2 end)
    {
        Vector2 pos = begin;
        wayIndex = 0;
        if (way.Count < 2)
            goto shortway;

        int i = 2;
        Vector2 next = way[0] + Vector2.one * 0.5f;
        Vector2 nextpl = way[1] + Vector2.one * 0.5f;
        GameObject point;
        int iter = 0;
        for (; iter < 1000;)
        {
            point = Navigation.GetWayPoint();
            pos += DirectionInPoint(pos, next, nextpl) * WayPointStep;
            point.transform.position = MapMetrics.RayCastedGround(pos);
            //point.SetActive(selectia.activeSelf);
            waypoints.Add(point);
            float dist = (next - pos).sqrMagnitude;
            if (dist < 1f)
            {
                if (i == way.Count + 1)
                    break;
                next = nextpl;
                nextpl = i < way.Count ? way[i] + Vector2.one * 0.5f : end;
                i++;
            }
            iter++;
        }
        shortway:
        Vector2 dir = end - pos;
        float l = dir.magnitude;
        dir /= l;
        for (float d = 0.4f; d < l; d += 0.4f)
        {
            point = Navigation.GetWayPoint();
            point.transform.position = MapMetrics.GetPosition(pos + dir * d, true);
            waypoints.Add(point);
        }
    }
    static Vector2 DirectionInPoint(Vector2 pos, Vector2 next, Vector2 nextpl)
    {
        Vector2 a = (next - pos).normalized, b = (nextpl - pos).normalized;
        
        return (0.8f * a + 0.2f * b).normalized;
    }
    private Vector2 DirectionIndex()
    {
        Vector2 to;
        if (wayIndex < waypoints.Count)
            to = FromV3(waypoints[wayIndex].transform.position);
        else
            to = end;
        return (to - pos_).normalized;

    }
}
