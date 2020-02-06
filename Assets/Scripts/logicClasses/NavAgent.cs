using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour
{
    public IFightable target;
    //public Vector2Int curCell => Navigation.GetFixedPosition(pos_);
    public Vector2Int curCell;
    Vector2 pos_;
    public Vector2 pos
    {
        get => pos_;
        set
        {
            Navigation.GetNavList(pos_).Remove(this);
            pos_ = value;
            Navigation.GetNavList(pos_).Add(this);
        }
    }
    public State owner;
    public Army army;
    float needAngle, dAngle = 2.5f;
    public Region lastCollidedTown;
    public void SetToArmy()
    {
        army = GetComponent<Army>();
        owner = army.owner;
        Navigation.AddNavAgent(this);
    }
    public void ResetNavAgent() => Navigation.GetNavList(pos_).Remove(this);
    Vector2 next, nextpl, end;
    List<Vector2Int> path;
    Vector2Int lastPathCell => path != null ?path.Count>0? path[path.Count - 1]:ToInt(end) : Vector2Int.zero;
    int pathIndex;
    private float SpeedLandCoef = 1;
    void FixedUpdate()
    {
        if (!Date.play)
            return;
        UpdateRotation();
        if (path == null)
            return;

        Vector2 direction = DirectionInPoint(pos, next, nextpl);
        Vector2 possave = pos;
        float delta = Time.fixedDeltaTime* GameTimer.timeScale * army.Speed * SpeedLandCoef;
        pos += delta * direction;

        needAngle = Vector3.SignedAngle(transform.forward, new Vector3(direction.x, 0, direction.y), Vector3.up);
        float dist = (next - pos).sqrMagnitude;
        if (dist < .01f)
        {

            next = nextpl;
            if (pathIndex < path.Count)
            {
                nextpl = path[pathIndex] + Vector2.one * 0.5f;
                SpeedLandCoef = 1f / Main.CellMoveCost(path[pathIndex]);
                pathIndex++;
            }
            else
            {
                nextpl = end;
                SpeedLandCoef = 1f / Main.CellMoveCost(Navigation.GetFixedPosition(end));
            }

        }

        if ((pos - end).sqrMagnitude < 0.001f)
        {
            pos = end;
            if(target==null)
            Stop();
        }
        UpdateWayPoints();
        Navigation.CalculateTownCollide(this);
        Navigation.CalculateArmyCollide(this);
        direction = (pos - possave).normalized;
        pos = possave +  direction * delta;
        Vector3 position = MapMetrics.GetPosition(pos);
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

            Vector2Int buf = curCell;
            curCell = cell;
            army.UpdateRegion(MapMetrics.GetRegion(cell));
            army.UpdateFog(buf, cell);
            Army.FoggedArmy();
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
        float dist = (FromV3(waypoints[wayIndex].transform.position) - pos).sqrMagnitude;
        if (lastDist < dist)//dist < 0.16f)
        {
            waypoints[wayIndex++].SetActive(false);
            lastDist = 10000f;
        }
        else
        lastDist = dist;
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
        army.Stop();
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
            pos += DirectionInPoint(pos, next, nextpl) * 0.4f;
            point.transform.position = MapMetrics.GetPosition(pos);
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
            point.transform.position = MapMetrics.GetPosition(pos + dir * d);
            waypoints.Add(point);
        }
    }
    static Vector2 DirectionInPoint(Vector2 pos, Vector2 next, Vector2 nextpl)
    {
        Vector2 a = (next - pos).normalized, b = (nextpl - pos).normalized;
        
        return (0.8f * a + 0.2f * b).normalized;
    }
}
