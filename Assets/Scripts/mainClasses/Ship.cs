using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour,IMovable
{
    NavAgent navAgent;
    public Army Army;
    public Port Port;
    public State curOwner;
    public GameObject Selection;
    private Port Target;
    private Vector2Int? NearestLand;
    public void Select(bool active)
    {
        Selection.SetActive(active);
    }
    State IMovable.curOwner => curOwner;

    public float Speed { get; set; } = 10;
    public float VisionRadius { get; set; } = 5;
    public Vector2Int curPosition => navAgent.curCell;
    private void ShipInitialization(Region region)
    {
        Vector2 v = region.Port.CornerPosition;
        SetPosition(v);
        curOwner = region.owner;

        navAgent = gameObject.AddComponent<NavAgent>();
        navAgent.pos = v;
        navAgent.curCell = new Vector2Int((int)v.x, (int)v.y);
        FindLand(navAgent.curCell);
        navAgent.SetToMovable(this);
        Select(false);
        if(Player.curPlayer == curOwner)
        {
            MapMetrics.UpdateAgentVision(navAgent.curCell, navAgent.curCell, VisionRadius, 1);
        }
    }
    public static Ship CreateShip(Region region)
    {
        Ship ship = Instantiate(PrefabHandler.GetShip);
        ship.ShipInitialization(region);
        region.owner.ships.Add(ship);
        return ship;
    }
    public void SetPosition(Vector2 v)
    {
        transform.position = MapMetrics.GetPosition(v, true);
    }
    public bool TryMoveTo(Vector3 p)
    {
        return navAgent.MoveTo(new Vector2(p.x, p.z), new Vector2Int((int)p.x, (int)p.z)) || CanDesant(p);
    }
    public bool TryMoveTo(Port port)
    {
        
        bool res =navAgent.MoveTo(port.CornerPosition, NavAgent.ToInt(port.CornerPosition));
        if (res)
            Target = port;
        return res;
    }

    public void CellChange(Vector2Int oldCell, Vector2Int newCell)
    {
        FindLand(newCell);
        if (curOwner == Player.curPlayer)
        {
            MapMetrics.UpdateAgentVision(oldCell, newCell, VisionRadius);
            MapMetrics.UpdateSplatMap();
            Army.FoggedArmy();
        }
    }
    void FindLand(Vector2Int pos)
    {
        
        foreach(var d in MapMetrics.OctoDelta)
        {
            Vector2Int p = pos + d;
            TerrainType land;
            if(MapMetrics.InsideMap(p.y,p.x)&& (land = Main.GetTerrainType(p)) != TerrainType.Water)
            {
                if (NearestLand.HasValue)
                {
                    var a = NearestLand.Value - pos;
                    if (Mathf.Abs(a.x) + Mathf.Abs(a.y) > Mathf.Abs(d.x) + Mathf.Abs(d.y))
                        NearestLand = p;
                }
                else
                {
                    NearestLand = p;
                }
            }
        }
        if (NearestLand.HasValue && (NearestLand.Value - pos).sqrMagnitude > 2)
            NearestLand = null;
    }
    public void SetOnArmy(Army army)
    {
        Army = army;
        army.Active = false;
        MapMetrics.UpdateAgentVision(army.curPosition, army.curPosition, army.VisionRadius, -1);
        army.gameObject.SetActive(false);
    }
    public bool CanSetOn(Army army)
    {
        return curOwner == army.curOwner && Army == null&&(transform.position - army.transform.position).sqrMagnitude < 2;
    }
    public bool CanDesant(Vector3 point)
    {
        if (!NearestLand.HasValue || Army == null)
            return false;
        Army.navAgent.curCell = NearestLand.Value;
        Army.navAgent.pos = NearestLand.Value;
        if(Army.TryMoveTo(point))
        {
            Army.Active = true;
            Army.gameObject.SetActive(true);
            MapMetrics.UpdateAgentVision(Army.curPosition, Army.curPosition, Army.VisionRadius, 1);
            Army = null;
            return true;
        }
        return false;
    }
    public void Stop()
    {
    }
    public void Update()
    {
        if(Target)
        {
           // Debug.Log((Target.transform.position - transform.position).magnitude);
            if((Target.transform.position - transform.position).sqrMagnitude < 1f)
            {
                navAgent.Stop();
                Target.ShipOn(this);
                Target = null;
                if (Player.ship == this)
                    Player.DeselectShip();
            }
        }
    }
}
