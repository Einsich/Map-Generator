using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmyAI : MonoBehaviour
{
    Army army_;
    public Army army
    {
        set { army_ = value; owner = value.owner; }
        get => army_;
    }
    State owner;
    public int regimentCount;

    private List<Behavior> behaviors;

    private SortedDictionary<float, IFightable> comparator = new SortedDictionary<float, IFightable>();
    private SortedDictionary<float, (IFightable, float)> comparator2 = new SortedDictionary<float, (IFightable, float)>();
    private SortedDictionary<float, Behavior> utilities = new SortedDictionary<float, Behavior>();

    public ArmyTargets targets = new ArmyTargets();
    private DeltaDamageCalculator deltaDamage;

    private Behavior lastBehavior;

    private IFightable curTarget;
    private DamageType curDamageType;

    private IFightable nearestEnemy;
    private IFightable nearestEnemyRegion;

    private float curHP;
    private DamageStatistic damageStat;

    public ArmyAI()
    {
        InitAI();
        deltaDamage = new DeltaDamageCalculator();
    }

    public void Logic()
    {
        Buffering();
        RemoveStrongEnemy();
        ClearTo();
        ApplyMaxUtility();
        ActionStation();
    }

    private void Buffering()
    {
        curHP = army.curHP;
        deltaDamage.ForceTick(curHP, army);
        nearestEnemy = SearchNearestEnemy(targets.enemyArmies);
        nearestEnemyRegion = SearchNearestEnemy(targets.enemyRegion);
        damageStat = army.GetDamageStatistic();
    }

    private void RemoveStrongEnemy()
    {
        if (deltaDamage.DeltaMeHP < 1.3f * deltaDamage.DeltaTargetHP && !army.inTown)
        {
            //Debug.Log(deltaDamage.DeltaMeHP + " W " + deltaDamage.DeltaTargetHP);
            targets.Remove(army.navAgent.target);
        }
    }

    private void ClearTo()
    {
        curTarget = null;
    }

    private void ActionStation()
    {
        if (curTarget != null)
        {
            if (!army.TryMoveToTarget(curTarget, curDamageType))
            {
                if (!Main.isPossibleMove(army.curPosition, curTarget.curPosition, owner))
                {
                    targets.Remove(curTarget);
                    Logic();
                }   
            }
        }
    }

    private void ApplyMaxUtility()
    {
        if (army.ActionState == ActionType.Idle)
        {
            lastBehavior = null;
        }

        utilities.Clear();

        foreach (Behavior b in behaviors)
        {
            float useful = b.useful();
            if (!utilities.ContainsKey(useful))
                utilities.Add(useful, b);
            if (useful == 1)
                break;
        }

        Behavior maxUtility = utilities.Last().Value;

        if (lastBehavior != maxUtility)
        {
            maxUtility.action();
            lastBehavior = maxUtility;
        }
    }

    private IFightable SearchNearestEnemy(List<IFightable> enemies)
    {
        comparator.Clear();
        foreach (IFightable enemy in enemies)
        {
            if (!enemy.Destoyed)
            {
                float dist = (enemy.position - army.position).magnitude;
                if (!comparator.ContainsKey(dist))
                    comparator.Add(dist, enemy);
            }
        }

        return comparator.FirstOrDefault().Value;
    }

    private void InitAI()
    {
        behaviors = new List<Behavior>();

        var backForHeal = new Behavior(SetMyRegion, UsefulMoveToHeal);
        var standForHeal = new Behavior(Idle, UsefulHealIdle);
        var rangeAttack = new Behavior(rangeAttackArmy, UsefulRangeAttack);
        var decreaseDistance = new Behavior(MeleeAttackArmy, UsefulDecreaseDistance);
        var meleeAttack = new Behavior(MeleeAttackArmy, UsefulMeleeAttack);
        var attackRegion = new Behavior(AttackRegion, UsefulAttackRegion);
        var rangeAttackFromTown = new Behavior(rangeAttackArmy, UsefulRangeAttackFromTown);
        var meleeAttackFromTown = new Behavior(MeleeAttackArmy, UsefulMeleeAttackFromTown);
        var backRegion = new Behavior(AttackRegion, UsefulBackRegion);

        behaviors.Add(rangeAttackFromTown);
        behaviors.Add(meleeAttackFromTown);
        behaviors.Add(standForHeal);
        behaviors.Add(backForHeal);
        behaviors.Add(rangeAttack);
        behaviors.Add(decreaseDistance);
        behaviors.Add(meleeAttack);
        behaviors.Add(attackRegion);
        behaviors.Add(backRegion);
    }

    private float UsefulBackRegion()
    {
        if (army.army.Count > 0)
        {
            comparator.Clear();

            foreach (IFightable region in targets.enemyRegion)
            {
                Region r = (Region)region;

                if (r.data.garnison.Count == 0 && r.owner == owner)
                {
                    float dist = (region.position - army.position).magnitude;
                    if (!comparator.ContainsKey(dist))
                        comparator.Add(dist, region);
                }
            }

            IFightable target = comparator.FirstOrDefault().Value;

            if (target != default(IFightable))
            {
                nearestEnemyRegion = target;
                return 0.8f;
            }
        }

        return 0;
    }

    private float UsefulDecreaseDistance()
    {
        int damager = damageStat.meleeDamager;
        if (nearestEnemy != null && damager != 0 && !army.inTown)
        {
            float dist = (nearestEnemy.position - army.position).magnitude;
            if (dist <= DamageInfo.AttackRange(DamageType.Range) &&
                dist > DamageInfo.AttackRange(DamageType.Melee))
            {
                float curDmg = damageStat.meleeDamage;
                if (deltaDamage.DeltaMeHP < 0.95f * deltaDamage.DeltaTargetHP ||
                    (damageStat.rangeDamager == 0 && damager > 0) ||
                    curDmg > 2 * damageStat.rangeDamage)
                {
                    float expectMaxDmg = army.Person.MaxRegiment * (curDmg / damager);
                    
                    return curDmg / expectMaxDmg;
                }
            }
        }
        return 0;
    }

    private float UsefulRangeAttackFromTown()
    {
        if (nearestEnemy != null && damageStat.rangeDamager > 0)
        {
            float dist = (nearestEnemy.position - army.position).magnitude;
            if (army.inTown &&
                dist <= DamageInfo.AttackRange(DamageType.Range) && dist > DamageInfo.AttackRange(DamageType.Melee))
                return 1;
        }
        return 0;
    }

    private float UsefulMeleeAttackFromTown()
    {
        if (nearestEnemy != null && damageStat.meleeDamager > 0)
        {
            float dist = (nearestEnemy.position - army.position).magnitude;
            if (army.inTown && dist <= DamageInfo.AttackRange(DamageType.Melee))
                return 1;
        }
        return 0;
    }

    private float UsefulAttackRegion()
    {
        if (army.army.Count > 0)
        {
            int meClearCount = damageStat.meleeDamager + damageStat.rangeDamager;
            float meClearDmg = damageStat.meleeDamage + damageStat.rangeDamage;
            float meAverClearDmg = meClearDmg / meClearCount;

            var info = army.GetDamage(DamageType.Melee);
            float meRealDamage = info.MeleeDamage + info.ChargeDamage + info.RangeDamage;

            comparator2.Clear();

            foreach (IFightable region in targets.enemyRegion)
            {
                Region r = (Region)region;

                int walls = r.data.wallsLevel;

                var enemyDmg = r.GetDamage(DamageType.Melee);

                float expDmgRegion = enemyDmg.MeleeDamage + enemyDmg.ChargeDamage + enemyDmg.RangeDamage;

                walls = (walls * walls + 3 * walls) / 2;
                walls = Mathf.Clamp(walls - info.SiegeDamage, 0, 1000);

                float armorReduction = DamageInfo.Armor(walls);

                armorReduction = 1;

                if (expDmgRegion < meRealDamage * armorReduction)
                {
                    float dist = (region.position - army.position).magnitude;
                    if (!comparator2.ContainsKey(dist))
                    {
                        if (nearestEnemy != null)
                        {
                            float distEn = (nearestEnemy.position - army.position).magnitude;
                            if (!comparator2.ContainsKey(dist))
                            {
                                if (distEn > dist)
                                    comparator2.Add(dist, (region, expDmgRegion / meRealDamage * armorReduction));
                            }
                        }
                        else
                        {
                            comparator2.Add(dist, (region, expDmgRegion / meRealDamage * armorReduction));
                        }
                    }
                }
            }

            var item = comparator2.FirstOrDefault().Value;
            IFightable target = item.Item1;

            if (target != default(IFightable))
            {
                nearestEnemyRegion = target;
                return 1 - item.Item2;
            }
        }

        return 0;
    }

    private void MeleeAttackArmy()
    {
        curTarget = nearestEnemy;
        curDamageType = DamageType.Melee;
    }
    private float UsefulMeleeAttack()
    {
        int damager = damageStat.meleeDamager;
        if (nearestEnemy != null && damager != 0)
        {
            float dist = (nearestEnemy.position - army.position).magnitude;
            if (dist < DamageInfo.AttackRange(DamageType.Melee))
            {
                float curDmg = damageStat.meleeDamage;
                float expectMaxDmg = army.Person.MaxRegiment * (curDmg / damager);

                return curDmg / expectMaxDmg;
            }
        }
        return 0;
    }

    private void rangeAttackArmy()
    {
        curTarget = nearestEnemy;
        curDamageType = DamageType.Range;
    }
    private float UsefulRangeAttack()
    {
        int damager = damageStat.rangeDamager;
        if (nearestEnemy != null && damager != 0)
        {
            float dist = (nearestEnemy.position - army.position).magnitude;
            if (dist < DamageInfo.AttackRange(DamageType.Range) &&
                dist >= DamageInfo.AttackRange(DamageType.Melee))
            {
                float curDmg = damageStat.rangeDamage;
                float expectMaxDmg = army.Person.MaxRegiment * (curDmg / damager);

                return curDmg / expectMaxDmg;
            }
        }
        return 0;
    }

    private void AttackRegion()
    {
        curTarget = nearestEnemyRegion;
        curDamageType = DamageType.Melee;
    }

    private void Idle()
    {
        Region reg = army.curReg;
        if(reg?.curOwner == owner)
        {
            while(reg.data.garnison.Count > 1 && army.army.Count < army.Person.MaxRegiment )
            {
                int i = UnityEngine.Random.Range(0, reg.data.garnison.Count);
                army.ExchangeRegiment(reg.data.garnison[i]);
            }
        }

        //army.Stop();
    }
    private float UsefulHealIdle()
    {
        if (army.inTown)
            return 1 - curHP;
        return 0;
    }

    private void SetMyRegion()
    {
        curTarget = targets.myRegionForDefend;
    }
    private float UsefulMoveToHeal()
    {
        if (army.inTown || targets.myRegionForDefend == null)
            return 0;
        return 1 - curHP;
    }



    private class Behavior
    {
        public readonly System.Action action;
        public readonly System.Func<float> useful;

        public Behavior(System.Action action, System.Func<float> useful)
        {
            this.action = action;
            this.useful = useful;
        }
    }

    private class DeltaDamageCalculator
    {
        private int calculateTick;
        private readonly Dictionary<int, HistoryHP> savingValues;

        public float DeltaMeHP { get; private set; }
        public float DeltaTargetHP { get; private set; }

        public DeltaDamageCalculator()
        {
            savingValues = new Dictionary<int, HistoryHP>();
        }

        public void ForceTick(float meHP, Army army)
        {
            DeltaMeHP = 0;
            DeltaTargetHP = 0;

            if (army.ActionState == ActionType.Attack)
            {
                float targetHP = army.navAgent.target.curHP;
                savingValues.Add(calculateTick + 15, new HistoryHP(meHP, targetHP));

                if (savingValues.ContainsKey(calculateTick))
                {
                    HistoryHP history = savingValues[calculateTick];

                    DeltaMeHP = meHP - history.meHP;
                    DeltaTargetHP = targetHP - history.targetHP;
                }

                calculateTick++;
            }
            else if (savingValues.Count > 0)
            {
                savingValues.Clear();
                calculateTick = 0;
            }
        }

        private class HistoryHP
        {
            public float meHP;
            public float targetHP;

            public HistoryHP(float meHP, float targetHP)
            {
                this.meHP = meHP;
                this.targetHP = targetHP;
            }
        }
    }
}

public class ArmyTargets
{
    public List<IFightable> enemyArmies;
    public List<IFightable> enemyRegion;
    public IFightable myRegionForDefend;

    public ArmyTargets()
    {
        enemyArmies = new List<IFightable>();
        enemyRegion = new List<IFightable>();
    }

    public void Remove(IFightable target)
    {
        if (myRegionForDefend == target)
        {
            myRegionForDefend = null;
        }
        else if (enemyArmies.Contains(target))
        {
            enemyArmies.Remove(target);
        }
        else
        {
            enemyRegion.Remove(target);
        }
    }
}
