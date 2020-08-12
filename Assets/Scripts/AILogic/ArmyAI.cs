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
        if (deltaDamage.DeltaMeHP < 1.2f * deltaDamage.DeltaTargetHP && !army.inTown)
        {
            Debug.Log(deltaDamage.DeltaMeHP + " W " + deltaDamage.DeltaTargetHP);
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

        behaviors.Add(backForHeal);
        behaviors.Add(standForHeal);
        behaviors.Add(rangeAttackFromTown);
        behaviors.Add(meleeAttackFromTown);
        behaviors.Add(rangeAttack);
        behaviors.Add(decreaseDistance);
        behaviors.Add(meleeAttack);
        behaviors.Add(attackRegion);
    }

    private float UsefulDecreaseDistance()
    {
        int damager = damageStat.meleeDamager;
        if (nearestEnemy != null && damager != 0)
        {
            float dist = (nearestEnemy.position - army.position).magnitude;
            if (dist <= DamageInfo.AttackRange(DamageType.Range) &&
                dist > DamageInfo.AttackRange(DamageType.Melee))
            {
                float curDmg = damageStat.meleeDamage;
                if (deltaDamage.DeltaMeHP < deltaDamage.DeltaTargetHP ||
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
        if (nearestEnemy != null)
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
        if (nearestEnemy != null)
        {
            float dist = (nearestEnemy.position - army.position).magnitude;
            if (army.inTown && dist <= DamageInfo.AttackRange(DamageType.Melee))
                return 1;
        }
        return 0;
    }

    private float UsefulAttackRegion()
    {
        var region = (Region)nearestEnemyRegion;

        int meCount = army.army.Count;
        if (nearestEnemyRegion != null && meCount > 0)
        {
            if (region.data.garnison.Count <= meCount * 2.5f)
            {
                int walls = region.data.wallsLevel - army.GetDamage(DamageType.Siege).SiegeDamage;
                walls = walls >= 1 ? walls : 1;

                //return 1 - (region.data.garnison.Count * walls / (float)meCount * 2.5f);
                return 0.5f;
            }
            else
            {
                targets.Remove(region);
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
            if (dist <= DamageInfo.AttackRange(DamageType.Melee))
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
            if (dist <= DamageInfo.AttackRange(DamageType.Range) &&
                dist > DamageInfo.AttackRange(DamageType.Melee))
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

    private void Idle() { }
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
