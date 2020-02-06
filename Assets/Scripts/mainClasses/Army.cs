using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army:MonoBehaviour,ITarget,IFightable
{
    public State owner;
    public State curOwner => owner;

    Region curReg_;
    public Region curReg => curReg_ != null ? curReg_ : MapMetrics.GetRegion(navAgent.curCell);
    public bool inTown;
    public ActionType ActionState;
    public List<Regiment> army;
    public Person genegal;
    public Vector2Int curPosition => navAgent.curCell;
    //public bool retreat;
    public Region besiege;
    public ArmyAI AI;
    public GameObject selectia,siegeModel;
    public ArmyBar bar;
    public bool CanExchangeRegimentWith(Region region)=> navAgent.curCell == region?.Capital && owner == region.owner;
    public bool Destoyed { get; set; } = false;
    public System.Action HitAction;
    GameObject cube;
    public NavAgent navAgent;
    Animation animator;
    public float Speed, MaxRange;
    public float AttackRange { get => Mathf.Min(MaxRange, DamageInfo.AttackRange(damageType)); set => MaxRange = value; }
    public Vector2 position => NavAgent.FromV3(transform.position);
    public DamageType damageType;
    public System.Action ArmyListChange;
    private void Start()
    {
        UpdateRegion(curReg);
        animator = GetComponent<Animation>();
        Stop();
        selectia.SetActive(false);
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale *= 0.3f;
    }
    private void FixedUpdate()
    {
        UpdateRotation();
        cube.transform.position = MapMetrics.GetCellPosition(navAgent.curCell);
        if (inTown)
            cube.transform.position += Vector3.up * 2;
        StateLogic();

    }
    private void StateLogic()
    {
        IFightable target= navAgent.target;
        bool inRadius = target != null && (target.position - position).magnitude <= AttackRange;
        switch (ActionState)
        {
            case ActionType.Idle:
            case ActionType.Move:if(inRadius && LastAttack + AttackPeriod <= GameTimer.time) {
                    navAgent.Stop();
                    Fight(target);
                }break;
            case ActionType.Attack:if (inRadius)
                {
                    if (LastAttack + AttackPeriod <= GameTimer.time)
                    {
                        var back = target.Hit(GetDamage(damageType));
                        LastAttack = GameTimer.time;
                        if (back != null)
                            Hit(back);
                        if(target.Destoyed)
                        {
                            navAgent.target = null;
                            Stop();
                            if(target is Region region)
                            {

                                Player.instance.Occupated(region, owner);
                            }
                        }
                    }
                    navAgent.RotateTo(target.position);
                }
                else
                {
                    if (target != null)
                    {
                        if (!TryMoveToTarget(target, damageType))
                            Stop();
                    }
                    else
                    {
                        Stop();
                    }
                }
                break;
            case ActionType.Effect:
                break;
        }
    }
    void UpdateRotation()
    {
        if (selectia.activeSelf)
            selectia.transform.Rotate(Vector3.up, .4f);
    }
    public void UpdateRegion(Region cur)
    {
        if (cur != curReg_)
        {
            if (genegal is Trader trader)
            {
                if (curReg_ != null)
                    trader.UpdateInfluence(curReg_, false);
                trader.UpdateInfluence(cur, true);
            }
            curReg_ = cur;
        }
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

    }
    bool active;
    public bool Active
    {
        get { return active; }
        set { active = value;
            if (Destoyed)
                return;
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
        gameObject.SetActive(true);
        army = list;        
        genegal = person;
        genegal.curArmy = this;
        owner = home.owner;
        navAgent = gameObject.AddComponent<NavAgent>();
        navAgent.SetToArmy();
        navAgent.pos = home.Capital + new Vector2(0.5f, 0.5f);
        transform.position = MapMetrics.GetCellPosition(home.Capital);
        navAgent.curCell = home.Capital;
        inTown = true;
        AI.army = this;
        Destoyed = false;
        Active = !Fogged;
        ArmyListChange += UpdateRange;
        ArmyListChange += UpdateSpeed;
        ArmyListChange += () => bar.UpdateInformation();
        // ArmyListChange();
        UpdateRange();
        UpdateSpeed();
    }
    public void ExchangeRegiment(Regiment regiment)
    {
        var list = curReg.data.garnison;
        List<Regiment> remove, add;
        if (list.Contains(regiment))
        { remove = list; add = army; }
        else
        { remove = army; add = list;
           // if (army.Count == 1)
             //   return;
        }
        remove.Remove(regiment);
        add.Add(regiment);
        ArmyListChange();
    }
    static public List<Army> AllArmy = new List<Army>();
    static public List<ArmyAI> AllArmyAI = new List<ArmyAI>();
    public static Army CreateArmy(Region home,List<Regiment> list,Person person)
    {
        State state = home.owner;

        GameObject def = Instantiate(Main.instance.ArmyPrefab[(int)state.fraction]);
        Army army = def.GetComponent<Army>();
        army.AI = def.AddComponent<ArmyAI>();
        army.siegeModel = Instantiate(Main.instance.SiegePrefab, Main.instance.Towns);
        army.siegeModel.SetActive(false);
        state.army.Add(army);
        ArmyBar bar = Instantiate(Main.instance.ArmyBarPrefab, Main.instance.mainCanvas);
        army.bar = bar;
        army.InitArmy(list, home, person);
        bar.currentArmy = army;
        AllArmy.Add(army);
        AllArmyAI.Add(army.AI);
        return army;
       
    }
    public bool TryMoveToTarget(IFightable target, DamageType damageType)
    {
        if (inTown && curReg.siegeby)
            return false;
        if (!owner.diplomacy.canAttack(target.curOwner.diplomacy))
            return false;
        this.damageType = damageType;
        bool inRadius = target != null && (target.position - position).magnitude <= AttackRange;
        if (inRadius)
        {
            Fight(target);
            navAgent.target = target;
            return false;
        }
        else
        {
            bool res = navAgent.MoveToTarget(target);
            if (res)
            {
                Move();
            }

            return res;
        }
    }
    public bool TryMoveTo(Vector3 p)=> TryMoveTo(new Vector2Int((int)p.x, (int)p.z), p);
    public bool TryMoveTo(Vector2Int to)=> TryMoveTo(to, MapMetrics.GetCellPosition(to));
    public bool TryMoveTo(Vector2Int to,Vector3 p)
    {
        if (inTown && curReg.siegeby)
            return false;
        bool res =  navAgent.MoveTo(new Vector2(p.x, p.z), to);
        if (res)
        {
            navAgent.target = null;
            Move();
        }
        return res;
    }
    void Move()
    {
        if (besiege != null)
        {
            besiege.SiegeEnd();
        }
        animator.Play("walk");
        ActionState = ActionType.Move;
    }
    public void Stop()
    {
        animator.Play("idle");
        ActionState = ActionType.Idle;
    }

    public float LastAttack { get; set; } = 0;
    public float AttackPeriod { get; set; } = 0.5f;

    public void DestroyArmy()
    {
        if (Destoyed)
            return;
        Destoyed = true;
        genegal.Die();
        gameObject.SetActive(false);
        bar.gameObject.SetActive(false);
        navAgent.ResetNavAgent();
    }
    public void Fight(IFightable enemy)
    {
        LastAttack = GameTimer.time;
        ActionState = ActionType.Attack;
        animator.Play("attack");
    }

    public static Army ArmyInPoint(Vector2Int point)
    {
        foreach (Army a in AllArmy)
            if (a.navAgent.curCell == point)
                return a;
        return null;
    }
   
    public float MediumCount()
    {
        float count = 0, max=0;
        foreach (Regiment r in army)
        {
            count += r.count;
            max += r.baseRegiment.maxcount;
        }
        
        return max ==0? 0:count / max;
    }
    public static void ProcessAllArmy()
    {
        AllArmy.RemoveAll(a => a.Destoyed);
        foreach (Army army in AllArmy)
        {
           // if (army.curBattle == null)
           // {
           //     army.UpdateManpower();
           // }
           // army.bar.UpdateInformation();
        }
    }
    public static void ProcessAllArmyAI()
    {
        return;
        AllArmyAI.RemoveAll(a => a.army.Destoyed);
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
                    float d = regiment.count + mp > regiment.baseRegiment.maxcount ? regiment.baseRegiment.maxcount - regiment.count : mp;
                    owner.treasury -= new Treasury(0, d,0,0,0);
                    regiment.count += d;
                }
                else
                    break;
        }
        bar.UpdateInformation();

    }

    public void UpdateUpkeep()
    {
        Treasury upkeep = new Treasury(0);
        foreach (var g in army)
            upkeep += g.baseRegiment.upkeep * owner.technology.UpkeepBonus;
        owner.treasury -= upkeep;
    }
    void UpdateSpeed()
    {
        bool[] HaveRegimentType = new bool[(int)RegimentType.Count];
        for (int i = 0; i < army.Count; i++)
        {
            HaveRegimentType[(int)army[i].baseRegiment.type] = true;
        }
        Speed = HaveRegimentType[(int)RegimentType.Artillery] ? 2.5f : HaveRegimentType[(int)RegimentType.Infantry] ? 5f : 10f;
    }
    void UpdateRange()
    {
        DamageType maxType = DamageType.Melee;
        for (int i = 0; i < army.Count; i++)
        {
            if (army[i].baseRegiment.damageType > maxType)
                maxType = army[i].baseRegiment.damageType;
        }
        MaxRange = DamageInfo.AttackRange(maxType);
    }
    public DamageInfo GetDamage(DamageType damageType)
    {
        DamageInfo info = new DamageInfo();
        foreach (Regiment regiment in army)
        {
            if (damageType <= regiment.baseRegiment.damageType)
                info.damage[(int)regiment.baseRegiment.damageType] += (regiment.NormalCount+1)*0.5f * regiment.baseRegiment.damage;
        }
        return info;
    }

    public DamageInfo Hit(DamageInfo damage)
    {

        if (army.Count == 0)
        {
            DestroyArmy();
            return null;
        }
        if (inTown)
        {
            return curReg.Hit(damage);
        }
        Regiment[] targets = new Regiment[4] { army[Random.Range(0, army.Count)], army[Random.Range(0, army.Count)], army[Random.Range(0, army.Count)], army[Random.Range(0, army.Count)] };
        float q = 1f / targets.Length;

        foreach(var target in targets)
        {
            float d = 0;
            for (int i = 0; i < (int)DamageType.Count; i++)
                d += damage.damage[i] * DamageInfo.Armor(target.baseRegiment.armor[i] ); 
            d *= q;
            target.count -= d;
        }
        army.RemoveAll(x => x.count <= 0);

        ArmyListChange();
        HitAction?.Invoke();
        return null;
    }
}
public enum ActionType
{
    Idle,
    Move,
    Attack,
    Effect
}

