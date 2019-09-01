using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army:MonoBehaviour,ITarget
{
    public State owner;
    public State curOwner => owner;
    public Region curReg => MapMetrics.GetRegion(navAgent.curCell);
    public bool inTown => curPosition == curReg.curPosition;
    public Battle curBattle;
    public List<Regiment> army;
    public Person genegal;
    public Vector2Int curPosition => navAgent.curCell;
    public bool retreat;
    public Region besiege;
    public ArmyAI AI;
    public GameObject selectia,siegeModel;
    public ArmyBar bar;
    public bool CanExchangeRegimentWith(Region region)=> navAgent.curCell == region?.Capital && owner == region.owner; 
    public bool destoyed;
    GameObject cube;
    public NavAgent navAgent;
    Animation animator;
    private void Start()
    {
        animator = GetComponent<Animation>();
        Stop();
        selectia.SetActive(false);
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale *= 0.3f;
    }
    private void FixedUpdate()
    {
        cube.transform.position = MapMetrics.GetCellPosition(navAgent.curCell);

    }
    void UpdateRotation()
    {
        if (selectia.activeSelf)
            selectia.transform.Rotate(Vector3.up, .4f);
    }

    
    public void UpdateFog(Vector2Int prev,Vector2Int next)
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
    public static void FoggedArmy()
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
        navAgent.ShowPath(selected);
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
        genegal = person;
        owner = home.owner;
        navAgent = gameObject.AddComponent<NavAgent>();
        navAgent.SetToArmy();
        navAgent.pos = home.Capital + new Vector2(0.5f, 0.5f);
        navAgent.curCell = home.Capital;
        AI.army = this;
        AllArmy.Add(this);
        AllArmyAI.Add(AI);
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
    public bool TryMoveToTarget(ITarget target)
    {
        Diplomacy dip = owner.GetDiplomacyWith(target.curOwner);
        if (dip != null && !dip.canAttack)
            return false;
        bool res = navAgent.MoveToTarget(target);
        if (res)
        {
            MoveAction();
        }
        return res;
    }
    public bool TryMoveTo(Vector3 p)=> TryMoveTo(new Vector2Int((int)p.x, (int)p.z), p);
    public bool TryMoveTo(Vector2Int to)=> TryMoveTo(to, MapMetrics.GetCellPosition(to));
    public bool TryMoveTo(Vector2Int to,Vector3 p)
    {
        bool res =  navAgent.MoveTo(new Vector2(p.x, p.z), to);
        if (res)
        {
            MoveAction();
            return true;
        }
        return res;
    }
    void MoveAction()
    {
        if (besiege != null)
        {
            besiege.SiegeEnd();
        }
        animator.Play("walk");
    }
    public void Stop()
    {
        animator.Play("idle");
        retreat = false;
    }
    public void WasCatch(Army catcher)
    {
        if (owner == catcher.owner)
        {
            catcher.Stop();
        }
        Diplomacy dip = owner.GetDiplomacyWith(catcher.owner);
        if(dip.canAttack)
        {
            if (curReg.Capital == navAgent.curCell)
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
        attacker.Fight(defender);
        defender.Fight(attacker);
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
    public void Retreat(Vector2Int p)
    {
        if (TryMoveTo(p))
        {
            retreat = true;
        }
        else
            DestroyArmy();
    }
    public static Queue<Region> queueRetreat = new Queue<Region>();
    public static Stack<Region> stackRetreat = new Stack<Region>();
    public Region FindRetreatRegion()
    {
        queueRetreat.Clear();
        stackRetreat.Clear();
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
    public void Fight(Army enemy)
    {
        navAgent.RotateTo(enemy.navAgent.pos);
        animator.Play("attack");
    }
    public static Army ArmyInPoint(Vector2Int point)
    {
        foreach (Army a in AllArmy)
            if (a.navAgent.curCell == point)
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
        return;
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
            int mp = curReg.Capital == navAgent.curCell ? GameConst.RecruitInTown : GameConst.RecruitInCountry;
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
        float d = curReg.Capital == navAgent.curCell ? GameConst.MoralInHome : GameConst.MoralInForeign;
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

