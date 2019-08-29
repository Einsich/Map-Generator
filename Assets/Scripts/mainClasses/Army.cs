using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army:MonoBehaviour,ITarget
{
    public State owner;
    public State curOwner => owner;
    public Region curReg => MapMetrics.GetRegion(curCell);
    public Battle curBattle;
    public ITarget target;
    public List<Regiment> army;
    public Person genegal;
    public Vector2Int curPosition => curCell;
    public bool retreat;
    public Region besiege;
    public ArmyAI AI;
    public GameObject selectia,siegeModel;
    public ArmyBar bar;
    public bool CanExchangeRegimentWith(Region region)=> curCell == region?.Capital && owner == region.owner;    
    public List<Vector2Int> path = null;
    List<GameObject> waypoints;
    public Vector2Int curCell;// => Navigation.GetFixedPosition(pos);
    public Vector2Int nextCell;
    public int next = -2,pointind;
    float  speed = 0.2f,needAngle;
    static float dAngle = 2.5f;
    public Vector2 endPath, direction,pos;
    public bool destoyed;
    GameObject cube;
    private void Start()
    {
        selectia.SetActive(false);
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale *= 0.3f;
    }
    private void Update()
    {
        UpdateRotation();
        cube.transform.position = MapMetrics.GetCellPosition(curCell);
        if (path == null)
            return;
        if (Reached(pos, nextCell))
        {
            Vector2Int p1 = curCell, p2 = nextCell;
            curCell = nextCell;

            UpdateFog(p1, p2);
            UpdateUnitFog();
            if(besiege!=null)
            {
                Vector2Int d = besiege.Capital - curCell;
                if (Mathf.Max(Mathf.Abs(d.x), Mathf.Abs(d.y)) > 1)
                    besiege.SiegeEnd();
            }
            if (next < 0)
            {
                if ((pos - endPath).sqrMagnitude < 0.01f)
                {
                    transform.position = MapMetrics.GetPosition(endPath);
                    needAngle = Vector3.SignedAngle(transform.forward, Vector3.back, Vector3.up);
                    Stop();
                    return;
                }
            }
            else
            {
                speed = 1f / Main.CellMoveCost(curCell);
                nextCell = path[next--];
                if (target != null && target.curPosition == nextCell)
                {

                    target.WasCatch(this);
                    target = null;
                    return;
                }
                Army arm = ArmyInPoint(nextCell);
                if (arm!=null)
                {
                    if (owner.GetDiplomacyWith(arm.owner).canAttack)
                        TryStartBattle(this, arm);
                    else
                        TryMoveTo(MapMetrics.GetPosition(endPath));
                    return;
                }
            }
        }

        if (pointind < waypoints.Count)
        {
            Vector2 distance = new Vector2(waypoints[pointind].transform.position.x, waypoints[pointind].transform.position.z) - pos;
            if (distance.sqrMagnitude < 1.0f)
                waypoints[pointind++].SetActive(false);
        }

        if (next >= 0)
        {
            direction = DirectionInPoint(pos, nextCell, path[next]);
        }
        else
            direction = endPath - pos;
        direction.Normalize();
        needAngle = Vector3.SignedAngle(transform.forward, new Vector3(direction.x, 0, direction.y), Vector3.up);

        pos += Date.dayPerSecond * Time.deltaTime * direction * speed;
        transform.position = MapMetrics.GetPosition(pos);


    }
    static bool Reached(Vector2 pos,Vector2Int next)
    {
        return Mathf.Max(Mathf.Abs(pos.x - next.x - 0.5f) , Mathf.Abs(pos.y - next.y - 0.5f)) <= 0.5f;
    }
    static Vector2 DirectionInPoint(Vector2 pos,Vector2Int next,Vector2Int nextpl)
    {
        return 0.66f * (next + Vector2.one * 0.5f - pos).normalized + 0.33f * (nextpl + Vector2.one * 0.5f - pos).normalized;
    }
    
    void UpdateRotation()
    {
        //transform.LookAt(transform.position + new Vector3(v.x, 0, v.y), Vector3.up);
        if (Mathf.Abs(needAngle) > dAngle)
        {
            transform.Rotate(Vector3.up, dAngle * Mathf.Sign(needAngle));
            needAngle -= dAngle * Mathf.Sign(needAngle);
        }
        if (selectia.activeSelf)
            selectia.transform.Rotate(Vector3.up, .4f);
    }

    
    void UpdateFog(Vector2Int prev,Vector2Int next)
    {
        if(Player.curPlayer==owner)
        {
            Region r1 = MapMetrics.GetRegion(prev)
            , r2 = MapMetrics.GetRegion(next);
            if (r1 != r2)
            {
                foreach (Region neib in r1.neib)
                    neib.UpdateSplateState(owner);
                foreach (Region neib in r2.neib)
                    neib.UpdateSplateState(owner);
                r1.UpdateSplateState(owner);
                r2.UpdateSplateState(owner);
                MapMetrics.UpdateSplatMap();
            }
        }
    }    
    public static void UpdateUnitFog()
    {
        if (Player.curPlayer == null)
            return;
        foreach (State st in Main.states)
            foreach (Army army in st.army)
                if (Player.curPlayer != army.owner && !CameraController.showstate)
                {
                    if (army.Fogged )
                        army.Active = false;
                    else
                        army.Active = true;
                }
    }

    public void Selected(bool selected)
    {
        selectia.SetActive(selected);
        //for (int i = 0; i <= next + 1; i++)
        int i = 0;
        foreach(var x in waypoints)
            x.SetActive(selected && i++ >= pointind);
        if (curBattle != null)
            MenuManager.ShowBattle(selected?curBattle:null);
    }
    bool active;
    public bool Active
    {
        get { return active; }
        set { active = value;
            bool sel = selectia.activeSelf;
            foreach (Transform t in transform)
                t.gameObject.SetActive(active);
            bar.gameObject.SetActive(active);
            Selected(sel);
        }
    }
    public bool Fogged
    {
        get { return curReg.HiddenFrom(Player.curPlayer) || curReg.InFogFrom(Player.curPlayer); }
    }
    public void InitArmy(List<Regiment>list,Region home,Person person)
    {
        army = list;

        waypoints = new List<GameObject>();
         nextCell = curCell = home.Capital;
        pos = curCell + new Vector2(0.5f, 0.5f);
        //nextCell = home.Capital;
        //pos = new Vector2(transform.position.x, transform.position.z);
        path = null;
        genegal = person;
        owner = home.owner;
        AI.army = this;
        AllArmy.Add(this);
        AllArmyAI.Add(AI);
        Stop();
    }
    public void ExchangeRegiment(Regiment regiment)
    {
        var list = curReg.data.garnison;
        List<Regiment> remove, add;
        if (list.Contains(regiment))
        { remove = list; add = army; }
        else
        { remove = army; add = list; }
        remove.Remove(regiment);
        add.Add(regiment);
    }
    static public List<Army> AllArmy = new List<Army>();
    static public List<ArmyAI> AllArmyAI = new List<ArmyAI>();
    public static Army CreateArmy(Region home,List<Regiment> list,Person person)
    {
        State state = home.owner;

        GameObject def = Instantiate(Main.instance.ArmyPrefab[(int)state.fraction]);
        Army army = def.GetComponent<Army>();
        def.transform.position = MapMetrics.GetCellPosition(home.Capital);
        army.AI = def.AddComponent<ArmyAI>();
        army.siegeModel = Instantiate(Main.instance.SiegePrefab, Main.instance.Towns);
        army.siegeModel.SetActive(false);
        state.army.Add(army);
        army.InitArmy(list, home, person);
        ArmyBar bar = Instantiate(Main.instance.ArmyBarPrefab, Main.instance.mainCanvas);
        bar.currentArmy = army;
        return army;
       
    }

    public bool TryAttack(ITarget target)
    {
        Diplomacy dip = owner.GetDiplomacyWith(target.curOwner);
        if (dip == null || dip.canAttack)
        {
            this.target = target;
            bool ans = TryMoveTo(target.curPosition);
            if (!ans)
                target = null;
            return ans;
        }
        return false;
    }

    public bool TryMoveTo(Vector3 p, bool retreat = false)=> TryMoveTo(new Vector2Int((int)p.x, (int)p.z), p, retreat);
    public bool TryMoveTo(Vector2Int to, bool retreat = false)=> TryMoveTo(to, MapMetrics.GetCellPosition(to), retreat);
    public bool TryMoveTo(Vector2Int to,Vector3 p, bool retreat = false)
    {
        Vector3 rp = MapMetrics.GetCellPosition(to);

        if (to==curCell)
        {
            ClearWayPoints();
            path = ListPool<Vector2Int>.Get();
            next = -1;
            GetComponent<Animation>().Play("walk");
            endPath.x = Mathf.Clamp(p.x, rp.x - 0.2f, rp.x + 0.2f);
            endPath.y = Mathf.Clamp(p.z, rp.z - 0.2f, rp.z + 0.2f);
            nextCell = curCell;
            return true;
        }
        List<Vector2Int> pa = Main.FindPath(curCell,to, owner);
        if (pa != null)
        {
            endPath.x = Mathf.Clamp(p.x, rp.x - 0.2f, rp.x + 0.2f);
            endPath.y = Mathf.Clamp(p.z, rp.z - 0.2f, rp.z + 0.2f);
            path = pa;
            next = path.Count - 2;
            nextCell = path[next + 1];

            if (target != null && target.curPosition == nextCell)
            {
                target.WasCatch(this);
                target = null;
            }
            else
                if (!TryStartBattle(this, ArmyInPoint(nextCell)))
            {

                ClearWayPoints();
                BuildWayPoints(pos, path, endPath);
                GetComponent<Animation>().Play("walk");
            }
        }
        return pa != null;
    }
    public void Stop()
    {
        if (path != null)
            ListPool<Vector2Int>.Add(path);
        path = null;
        next = -2;
        ClearWayPoints();
        GetComponent<Animation>().Play("idle");
        retreat = false;
    }
    
    public void WasCatch(Army catcher)
    {
        if (owner == catcher.owner)
        {
            catcher.Stop();
        }
        else
        {
            if (curReg.Capital == curPosition)
                curReg.WasCatch(catcher);
            else
                TryStartBattle(this, catcher);
        }
    }
    public static bool TryStartBattle(Army attacker,Army defender)
    {
        if (attacker.retreat || !defender || defender.retreat || attacker.curBattle != null ||
            defender.curBattle != null ||attacker == defender) 
            return false;
        attacker.Stop();
        defender.Stop();
        attacker.Fight(defender.pos);
        defender.Fight(attacker.pos);
        Battle battle = attacker.curBattle = defender.curBattle = new Battle(attacker, defender);
        if (Player.army == attacker || Player.army == defender)
            MenuManager.ShowBattle(battle);
        return true;
    }
    public void EndBattle()
    {
        if (curBattle != null)
        {
            curBattle = null;
            Stop();
        }
    }
    public void Retreat(Vector3 p)
    {
        if (TryMoveTo(p,true))
        {
            retreat = true;
        }
    }
    public static Queue<Region> queueRetreat = new Queue<Region>();
    public static Stack<Region> stackRetreat = new Stack<Region>();
    public Region FindRetreatRegion()
    {
        queueRetreat.Clear();
        Region cur = curReg;
        queueRetreat.Enqueue(cur);
        stackRetreat.Push(cur);
        cur.retreatused = true;
        while (queueRetreat.Count > 0)
        {
            cur = queueRetreat.Dequeue();
            foreach (Region neib in cur.neib)
                if (!neib.retreatused && neib.iswater == cur.iswater)
                {
                    if (neib.owner == owner && neib.ocptby == null)
                    {
                        while (stackRetreat.Count > 0)
                            stackRetreat.Pop().retreatused = false;
                        
                        return neib;
                    }
                    queueRetreat.Enqueue(neib);
                    stackRetreat.Push(neib);
                    neib.retreatused = true;
                }
        }
        while (stackRetreat.Count > 0)
            stackRetreat.Pop().retreatused = false;
        
        return null;
    }
    public void DestroyArmy()
    {
        destoyed = true;
        owner.army.Remove(this);
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }
    public void Fight(Vector2 enemy)
    {
        needAngle = Vector3.SignedAngle(transform.forward, new Vector3(enemy.x - pos.x, 0, enemy.y - pos.y), Vector3.up);
        GetComponent<Animation>().Play("attack");
    }
    public static List<GameObject> WayPoints = new List<GameObject>();
    public static int WayPointsIndex = -1;
    void ClearWayPoints()
    {
        foreach (GameObject wp in waypoints)
        {
            WayPointsIndex++;
            if (WayPoints.Count > WayPointsIndex)
                WayPoints[WayPointsIndex] = wp;
            else
                WayPoints.Add(wp);
            wp.SetActive(false);
        }
        waypoints.Clear();
    }
    void BuildWayPoints(Vector2 begin, List<Vector2Int>way,Vector2 end)
    {
        Vector2 pos = begin;
        int n = way.Count;
        pointind = 0;
        if (way.Count < 3)
            goto shortway;
        Vector2Int next = way[n-1];
        Vector2Int nextpl = way[n-2];
        GameObject point;
        for (int i=n-3;i>=0;)
        {
            point = 0 <= WayPointsIndex? WayPoints[WayPointsIndex--]: Instantiate(Main.instance.WayPoint);
            pos += DirectionInPoint(pos, next, nextpl).normalized;
            point.transform.position = MapMetrics.GetPosition(pos);
            point.SetActive(selectia.activeSelf);
            waypoints.Add(point);
            if(Reached(pos,next))
            {
                next = nextpl;
                nextpl = way[i--];
            }
        }
        shortway:
        Vector2 d = end - pos;
        for (int i = way.Count>1?1:2; i <= 2; i++)
        {
            point = 0 <= WayPointsIndex ? WayPoints[WayPointsIndex--] : Instantiate(Main.instance.WayPoint);
            point.transform.position = MapMetrics.GetPosition(pos + d * 0.5f*i);
            point.SetActive(selectia.activeSelf);
            waypoints.Add(point);
        }
    }

    public static Army ArmyInPoint(Vector2Int point)
    {
        foreach (Army a in AllArmy)
            if (a.curCell == point)
                return a;
        return null;
    }
    public float MediumMoral()
    {
        float m = 0;
        foreach (Regiment r in army)
            m += r.moralF;
        return m / army.Count;
    }
    public float MediumCount()
    {
        float c = 0;
        foreach (Regiment r in army)
            c += r.countF;
        return c / army.Count ;
    }
    public static void ProcessAllArmy()
    {
        AllArmy.RemoveAll(a => a.destoyed);
        foreach (Army army in AllArmy)
        {
            if (army.curBattle == null)
            {
                army.UpdateManpower();
                army.UpdateMoral();
            }
            army.bar.UpdateInformation();
        }
    }
    public static void ProcessAllArmyAI()
    {
        AllArmyAI.RemoveAll(a => a.army.destoyed);
        foreach (var ai in AllArmyAI)
            if (ai.enabled)
                ai.DoRandomMove();
    }
    public void UpdateManpower()
    {
        Region region = curReg;
        if(curReg.owner == owner&& curReg.ocptby==null)
        {
            int mp = curReg.Capital == curCell ? 10 : 4;
            foreach (Regiment regiment in army)
                if (owner.treasury.Manpower >= mp)
                {
                    int d = regiment.count + mp > 1000 ? 1000 - regiment.count : mp;
                    owner.treasury -= new Treasury(0, d,0,0,0);
                    regiment.count += d;
                }
                else
                    break;
        }
    }
    public void UpdateMoral()
    {
        float d = curReg.Capital == curCell ? 0.02f : 0.01f;
        foreach (Regiment regiment in army)
            regiment.moral = Mathf.Clamp(regiment.moral + d, 0, regiment.maxmoral);
    }
}
public enum ArmyAction
{
    Idle,
    Move,
    Siege,
    Retreat,
    Fight
}

