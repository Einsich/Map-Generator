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
    public bool destoyed;
    GameObject cube;
    public NavAgent navAgent;
    Animation animator;
    public float Speed;
    public float AttackRange { get;  set; }
    public Vector2 position => NavAgent.FromV3(transform.position);
    public DamageType damageType;
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
            case ActionType.Move:if(inRadius && LastAttack + AttackPeriod <= Time.time) {
                    navAgent.Stop();
                    Fight(target);
                }break;
            case ActionType.Attack:if (inRadius)
                {
                    if (LastAttack + AttackPeriod <= Time.time)
                    {
                        target.Hit(GetDamage(damageType));
                        LastAttack = Time.time;
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
       // if (curBattle != null)
        //    MenuManager.ShowBattle(selected?curBattle:null);
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
        genegal.curArmy = this;
        owner = home.owner;
        navAgent = gameObject.AddComponent<NavAgent>();
        navAgent.SetToArmy();
        navAgent.pos = home.Capital + new Vector2(0.5f, 0.5f);
        navAgent.curCell = home.Capital;
        inTown = true;
        AI.army = this;
        UpdateSpeed();

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
        { remove = army; add = list;
            if (army.Count == 1)
                return;
        }
        remove.Remove(regiment);
        add.Add(regiment);
        UpdateSpeed();
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
    public bool TryMoveToTarget(IFightable target, DamageType damageType)
    {
        if (inTown && curReg.siegeby)
            return false;
        if (!owner.diplomacy.canAttack(target.curOwner.diplomacy))
            return false;
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
            this.damageType = damageType;
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
    public float AttackPeriod { get; set; } = 1;

    public void DestroyArmy()
    {
        destoyed = true;
        genegal.Die();
        gameObject.SetActive(false);
        bar.gameObject.SetActive(false);
        navAgent.ResetNavAgent();
    }
    public void Fight(IFightable enemy)
    {
        LastAttack = Time.time;
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
        return count / max;
    }
    public static void ProcessAllArmy()
    {
        AllArmy.RemoveAll(a => a.destoyed);
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
        bool[] HaveDamageTupe = new bool[(int)DamageType.Count];
        for (int i = 0; i < army.Count; i++)
        {
            HaveRegimentType[(int)army[i].baseRegiment.type] = true;
            HaveDamageTupe[(int)army[i].baseRegiment.damageType] = true;
        }
        Speed = HaveRegimentType[(int)RegimentType.Artillery] ? 0.5f : HaveRegimentType[(int)RegimentType.Infantry] ? 1 : 2f;
        AttackRange = HaveDamageTupe[(int)DamageType.Siege] ? 3f : HaveDamageTupe[(int)DamageType.Range] ? 2f : 1f;
    }
    public DamageInfo GetDamage(DamageType damageType)
    {
        DamageInfo info = new DamageInfo();
        foreach (Regiment regiment in army)
        {
            if (damageType <= regiment.baseRegiment.damageType)
                info.damage[(int)regiment.baseRegiment.damageType] += regiment.NormalCount * regiment.baseRegiment.damage;
        }
        return info;
    }

    public void Hit(DamageInfo damage)
    {
        if (army.Count == 0)
            return;
        Regiment[] targets = new Regiment[4] { army[Random.Range(0, army.Count)], army[Random.Range(0, army.Count)], army[Random.Range(0, army.Count)], army[Random.Range(0, army.Count)] };
        float q = 1f / targets.Length;

        foreach(var target in targets)
        {
            float d = 0;
            for (int i = 0; i < (int)DamageType.Count; i++)
                d += damage.damage[i];
            d *= q;
            target.count -= d;
        }
        army.RemoveAll(x => x.count <= 0);
        UpdateSpeed();
        bar.UpdateInformation();

    }
}
public enum ActionType
{
    Idle,
    Move,
    Attack,
    Effect
}

