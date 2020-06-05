using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
public class Army:MonoBehaviour,ITarget,IFightable, IMovable
{
    public State owner;
    public State curOwner => owner;

    Region curReg_;
    public Region curReg => curReg_ != null ? curReg_ : MapMetrics.GetRegion(navAgent.curCell);
    public bool inTown;
    public ActionType ActionState;
    public List<Regiment> army;
    public Person Person;
    public Vector2Int curPosition => navAgent.curCell;
    public Region besiege;
    public ArmyAI AI;
    public GameObject selectia,siegeModel;
    public ArmyBar bar;
    public bool CanExchangeRegimentWith(Region region)=> navAgent.curCell == region?.Capital && owner == region.curOwner;
    public bool Destoyed { get; set; } = false;
    public System.Action HitAction;
    GameObject cube;
    public NavAgent navAgent;
    Animation animator;
    public float Speed { get; set; }
    public float VisionRadius { get; set; } = 5;
    public float MaxRange;
    public DamageType maxRangeType;
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
                        int RegimentWas = 0, RegimentNow = 0;
                        if (target is Army x)
                            RegimentWas = x.army.Count;
                        else if (target is Region y)
                            RegimentWas = y.data.garnison.Count;

                        var back = target.Hit(GetDamage(damageType));
                        LastAttack = GameTimer.time;
                        if (back != null)
                            Hit(back);

                        if (target is Army xx)
                            RegimentNow = xx.army.Count;
                        else if (target is Region yy)
                            RegimentNow = yy.data.garnison.Count;
                        Effect effect;
                        Skill skill;
                        if(RegimentWas > RegimentNow && Person.HaveSkill(SkillType.SkeletonArmy, out skill))
                        {
                            (skill as SkeletonArmySkill).KilledRegiment += RegimentWas - RegimentNow;
                            skill.ButtonUpdate?.Invoke();
                        }
                        if (target.Destoyed)
                        {
                            navAgent.target = null;
                            Stop();
                            if(target is Region region)
                            {

                                Player.instance.Occupated(region, owner);
                            }
                        } else
                        if(target is Army enemy)
                        {
                            if(Person.HaveEffect(EffectType.Fear, out effect))
                            {
                                if(Random.value < (effect as Fear).StunPropability(damageType))
                                {
                                    //todo stun
                                }
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
    public void CellChange(Vector2Int oldCell, Vector2Int newCell)
    {

        UpdateRegion(MapMetrics.GetRegion(newCell));
        if (owner == Player.curPlayer)
        {
            UpdateFog(oldCell, newCell);
        }
            Army.FoggedArmy();
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
            curReg_ = cur;
        }
        MenuManager.CheckExchangeRegiment();
        if (Person is Jaeger)
            UpdateSpeed();
    }
    
    public void UpdateFog(Vector2Int prev,Vector2Int next)
    {
        MapMetrics.UpdateAgentVision(prev, next, VisionRadius);
        MapMetrics.UpdateSplatMap();
        return;
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
        {
            if (Player.curPlayer != st)
            {
                foreach (Army army in st.army)
                    if (!CameraController.showstate)
                    {
                        army.Active = !army.Fogged;
                    }
            } else
            {
                foreach (Army army in st.army)
                    army.Active = true;
            }
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
    public bool Fogged => !MapMetrics.Visionable(curPosition);
    public void InitArmy(List<Regiment>list,Region home,Person person)
    {
        gameObject.SetActive(true);
        army = list;
        Person = person;
        Person.curArmy = this;
        owner = home.owner;
        navAgent = gameObject.AddComponent<NavAgent>();
        navAgent.pos = home.Capital + new Vector2(0.5f, 0.5f);
        transform.position = MapMetrics.GetCellPosition(home.Capital);
        navAgent.curCell = home.Capital;
        navAgent.SetToMovable(this);
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
        UpdateAttackSpeed();

        owner.IncomeChanges?.Invoke();
        if (Player.curPlayer == curOwner)
        {
            MapMetrics.UpdateAgentVision(navAgent.curCell, navAgent.curCell, VisionRadius, 1);
        }
    }
    public void ExchangeRegiment(Regiment regiment)
    {
        var list = curReg.data.garnison;
        List<Regiment> remove, add;
        if (list.Contains(regiment))
        {
            if (army.Count >= Person.MaxRegiment)
                return;
            remove = list; add = army; }
        else
        {
            remove = army; add = list;
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

        GameObject def = Instantiate(PrefabHandler.GetArmyPrefab(person.personType));
        Army army = def.GetComponent<Army>();
        army.AI = def.AddComponent<ArmyAI>();
        army.siegeModel = Instantiate(PrefabHandler.GetSiegePrefab, Main.instance.Towns);
        army.siegeModel.SetActive(false);
        state.army.Add(army);
        ArmyBar bar = Instantiate(PrefabHandler.GethpBarPrefab, Main.instance.mainCanvas);
        army.bar = bar;
        army.InitArmy(list, home, person);
        bar.currentArmy = army;
        AllArmy.Add(army);
        AllArmyAI.Add(army.AI);
        return army;
       
    }
    public bool TryMoveToTarget(IFightable target, DamageType damageType)
    {
        if (inTown && curReg.CurrentSiege!= null)
            return false;
        if (!owner.diplomacy.haveWar(target.curOwner.diplomacy))
            return false;
        this.damageType = damageType;
        bool inRadius = target != null && (target.position - position).magnitude <= AttackRange;
        if (inRadius)
        {
            if (ActionState == ActionType.Move)
            {
                navAgent.Stop();
                Stop();
            }
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
        if (inTown && curReg.CurrentSiege!=null)
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
        Effect effect;
        if(Person.HaveEffect(EffectType.Paling, out effect))
        {
            (effect as Paling).EffectAction.actually = false;
            effect.EffectAction.onAction();
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
        Person.Die();
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
        /*foreach (Army army in AllArmy)
        {
           // if (army.curBattle == null)
           // {
           //     army.UpdateManpower();
           // }
           // army.bar.UpdateInformation();
        }*/
    }
    public static void ProcessAllArmyAI()
    {
        //return;
        AllArmyAI.RemoveAll(a => a.army.Destoyed);
        foreach (var ai in AllArmyAI)
            if (ai.enabled)
                ai.Logic();
    }
    public void Heal(float heal)
    {
        if (army.Count == 0)
            return;
        int Count = 5;
        for (int i = 0, j; i < Count; i++)
        {
            j = Random.Range(0, army.Count);
            army[j].count = Mathf.Clamp(army[j].count + heal / Count, 0, army[j].baseRegiment.maxcount);
        }

        bar.UpdateInformation();
        HitAction?.Invoke();
    }
    public void UpdateManpower()
    {
        Region region = curReg;
        int manpowerBonus = 0;
        Effect effect;
        if((navAgent.TerrainType == TerrainType.ForestLeaf || navAgent.TerrainType == TerrainType.ForestSpire) && Person.HaveEffect(EffectType.ForestBrothers,out effect))
        {
            manpowerBonus += (effect as ForestBrothers).ManpowerIncrease;
        }
        if ( Person.HaveEffect(EffectType.Sabotage, out effect))
        {
            manpowerBonus -= (effect as Sabotage).Deplention;
        }
        if (Person.HaveEffect(EffectType.DeadMarch, out effect))
        {
            manpowerBonus += (effect as DeadMarch).RegenBonus;
        }
        TheBats bats = null;
        float vamrirism = 0;
        if(Person.HaveEffect(EffectType.TheBats, out effect))
        {
            bats = effect as TheBats;
        }
        if (curReg.curOwner == owner && inTown)
            manpowerBonus += GameConst.RecruitInTown;
        if (manpowerBonus != 0)
        {
            float mp = owner.treasury.Manpower,dmp = 0;

            foreach (Regiment regiment in army)
                if (mp >= manpowerBonus || manpowerBonus < 0 || bats !=null)
                {
                    float d = regiment.count + manpowerBonus > regiment.baseRegiment.maxcount ? regiment.baseRegiment.maxcount - regiment.count : manpowerBonus;
                    float v = bats != null ? bats.ArmoredVampirism(regiment.baseRegiment.ArmorLvl(DamageType.Melee)) : 0; 
                    d -= v;
                    vamrirism += v;
                    dmp += d > 0 ? d : 0;
                    mp -= dmp;
                    regiment.count += d;
                }
                else
                    break;

            owner.SpendTreasure(new Treasury(0, dmp, 0, 0, 0), BudgetType.ArmyBudget);
        }
        if(manpowerBonus <0 || bats != null)
        {
            army.RemoveAll(x => x.count <= 0);
            ArmyListChange();
            HitAction?.Invoke();
        }
        if(bats !=null)
        {
            bats.Vampir.curArmy.Heal(vamrirism * bats.VampirismQuality);
        }
        bar.UpdateInformation();

    }

    private float timeFromDecaStart = 0;
    private float upkeepDiscountInTown = 1;
    private VassalDebt debt;

    public void DeciUpdateTime()
    {
        if (inTown && curReg.curOwner == owner)
        {
            timeFromDecaStart += 0.1f;
        }
    }

    public void ResetTimeAndRecalcUpkeepBonuses()
    {
        float normalTime = timeFromDecaStart * 0.1f;
        timeFromDecaStart = 0;

        upkeepDiscountInTown = GameConst.GarnisonUpkeepDiscount * normalTime + (1 - normalTime);

        UpdateDebt();
    }

    private void UpdateDebt()
    {
        Effect effect;
        if (Person.HaveEffect(EffectType.VassalDebt, out effect))
        {
            debt = effect as VassalDebt;
        }
        else
        {
            debt = null;
        }
    }

    public Treasury GetUpkeep()
    {
        Treasury upkeep = new Treasury(0);

        foreach (var g in army)
        {
            upkeep += UpkeepInArmy(g.baseRegiment);
        }
        return upkeep;
    }

    public Treasury UpkeepInArmy(BaseRegiment baseRegiment)
    {
        float bonus = 1f;
        bonus *= debt != null ? debt.ReductionUpkeep(baseRegiment.type) : 1f;
        bonus *= upkeepDiscountInTown;
        return baseRegiment.GetBonusUpkeep() * bonus;
    }

    public void UpdateSpeed()
    {
        bool[] HaveRegimentType = new bool[(int)RegimentType.Count];
        for (int i = 0; i < army.Count; i++)
        {
            HaveRegimentType[(int)army[i].baseRegiment.type] = true;
        }
        RegimentType slowest;
        slowest = HaveRegimentType[(int)RegimentType.Artillery] ? RegimentType.Artillery : HaveRegimentType[(int)RegimentType.Infantry] ? RegimentType.Infantry : RegimentType.Cavalry;

        Effect effect;
        switch(slowest)
        {
            case RegimentType.Artillery: Speed = 2.5f;break;
            case RegimentType.Infantry: Speed = 5f;break;
            case RegimentType.Cavalry: Speed = 10f;break;
        }
        if(Person.HaveEffect(EffectType.HeavyArmor, out effect))
        {
            Speed *= (effect as HeavyArmor).ReductionSpeed(slowest);

        }
        if (Person.HaveEffect(EffectType.Charge, out effect))
        {
            Speed *= (effect as Charge).SpeedBonus;
        }
        if ((navAgent.TerrainType == TerrainType.ForestLeaf || navAgent.TerrainType == TerrainType.ForestSpire) && Person.HaveEffect(EffectType.ForestTrails, out effect))
        {
            Speed *= (effect as ForestTrails).SpeedBonus;
        }
        if (Person.HaveEffect(EffectType.Sabotage, out effect))
        {
            Speed *= (effect as Sabotage).SpeedDebuff;
        }
        if (Person.HaveEffect(EffectType.DeadMarch, out effect))
        {
            Speed *= (effect as DeadMarch).SpeedBonus;
        }
    }
    void UpdateRange()
    {
        maxRangeType = DamageType.Melee;
        for (int i = 0; i < army.Count; i++)
        {
            if (army[i].baseRegiment.damageType > maxRangeType)
                maxRangeType = army[i].baseRegiment.damageType;
        }
        MaxRange = DamageInfo.AttackRange(maxRangeType);
    }
    public void UpdateAttackSpeed()
    {
        float t = Person.AttackSpeed;
        Effect effect;
        if((navAgent.TerrainType == TerrainType.ForestLeaf || navAgent.TerrainType == TerrainType.ForestSpire) && Person.HaveEffect(EffectType.ForestTrails, out effect))
        {
            t /= (effect as ForestTrails).AttackSpeedBonus;
        }
        AttackPeriod = GameConst.AttackPeriod * t;
    }
    public DamageInfo GetDamage(DamageType damageType)
    {
        DamageInfo info = new DamageInfo();
        
        foreach (Regiment regiment in army)
        {
            if (damageType <= regiment.baseRegiment.damageType)
            {
                info.damage[(int)regiment.baseRegiment.damageType] += RegimentDamage(regiment);
            }
        }
        Effect effect;
        if(Person.HaveEffect(EffectType.Charge, out effect))
        {
            (effect as Charge).EffectAction.actually = false;
            Person.StopUseSkillOnHim(effect);
            UpdateSpeed();
        }
        return info;
    }

    private float RegimentDamage(Regiment regiment)
    {
        return (regiment.NormalCount + 1) * 0.5f *
            (regiment.baseRegiment.damage(Person.DamageLvlBuff(regiment.baseRegiment.type, regiment.baseRegiment.damageType) + Person.DamageTypeBuff(regiment.baseRegiment.damageType)) +
            Person.DamageBuff(regiment.baseRegiment.type, regiment.baseRegiment.damageType));
    }

    public DamageInfo GetDamage()
    {
        var info = new DamageInfo();

        foreach (Regiment regiment in army)
        {
            info.damage[(int)regiment.baseRegiment.damageType] += RegimentDamage(regiment);
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
        Effect effect;
        if(Person.HaveEffect(EffectType.Immortaly, out effect))
        {
            return null;
        }
        Regiment[] targets = new Regiment[4] { army[Random.Range(0, army.Count)], army[Random.Range(0, army.Count)], army[Random.Range(0, army.Count)], army[Random.Range(0, army.Count)] };
        float q = 1f / targets.Length;

        foreach(var target in targets)
        {
            float d = 0;
            for (int i = 0; i < (int)DamageType.Count; i++)
                d += damage.damage[i] * DamageInfo.Armor(target.baseRegiment.ArmorLvl((DamageType)i) + Person.ArmorLvlBuff(target.baseRegiment.type, (DamageType)i));
            d *= q;
            target.count -= d;
        }
        int regimentCount = army.Count;
        army.RemoveAll(x => x.count <= 0);

        if(army.Count != regimentCount)
            owner.IncomeChanges?.Invoke();

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

