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

    private Behavior lastBehavior;

    private IFightable curTarget;
    private DamageType curDamageType;

    private IFightable nearestEnemy;
    private float curHP;

    public ArmyAI()
    {
        InitAI();
    }

    public void Logic()
    {
        Buffering();
        ClearTo();
        ApplyMaxUtility();
        ActionStation();
    }

    private void Buffering()
    {
        curHP = army.MediumCount();
        nearestEnemy = SearchNearestEnemy();
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
                    targets.Remove(curTarget);

                Logic();
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

    private IFightable SearchNearestEnemy()
    {
        comparator.Clear();
        foreach (IFightable enemy in targets.enemyArmies)
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
        var backRegion = new Behavior(AttackRegion, BackMyRegion);
        var rangeAttack = new Behavior(rangeAttackArmy, UsefulRangeAttack);
        var meleeAttack = new Behavior(MeleeAttackArmy, UsefulMeleeAttack);
        var attackRegion = new Behavior(AttackRegion, UsefulAttackRegion);

        behaviors.Add(backForHeal);
        behaviors.Add(standForHeal);
        behaviors.Add(backRegion);
        behaviors.Add(rangeAttack);
        behaviors.Add(meleeAttack);
        behaviors.Add(attackRegion);
    }

    private float UsefulAttackRegion()
    {
        if (targets.enemyRegion != default(IFightable))
        {
            float myMeleeAttack = army.GetDamage(DamageType.Melee).MeleeDamage;
            float enemyMeleeAttack = targets.enemyRegion.GetDamage(DamageType.Melee).MeleeDamage;

            if (myMeleeAttack > enemyMeleeAttack + enemyMeleeAttack * ((Region)targets.enemyRegion).data.wallsLevel * 0.1f)
            {
                return 0.25f;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }

    private void MeleeAttackArmy()
    {
        curTarget = nearestEnemy;
        curDamageType = DamageType.Melee;
    }
    private float UsefulMeleeAttack()
    {
        if (nearestEnemy != default(IFightable))
        {
            float myRangeAttack = army.GetDamage(DamageType.Range).RangeDamage;
            float enemyRangeAttack = nearestEnemy.GetDamage(DamageType.Range).RangeDamage;
            float myMeleeAttack = army.GetDamage(DamageType.Melee).MeleeDamage;
            float enemyMeleeAttack = nearestEnemy.GetDamage(DamageType.Melee).MeleeDamage;

            float dist = (nearestEnemy.position - army.position).magnitude;
            float maxDist = DamageInfo.AttackRange(DamageType.Melee);

            if (dist < DamageInfo.AttackRange(DamageType.Range) &&
                myRangeAttack < enemyRangeAttack &&
                myMeleeAttack * 2 > enemyMeleeAttack
                )
            {
                return 1.3f * enemyRangeAttack / myRangeAttack;
            }
            else if (dist < 0.9f * DamageInfo.AttackRange(DamageType.Range) &&
                myMeleeAttack * 2 > enemyMeleeAttack
                )
            {
                return dist / maxDist;
            }

            return 0;
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
        if (nearestEnemy != default(IFightable))
        {
            //Army nearestEnemy = (Army)this.nearestEnemy;
            DamageInfo myRangeAttack = army.GetDamage(DamageType.Range);
            DamageInfo enemyRangeAttack = nearestEnemy.GetDamage(DamageType.Range);
            float dist = (nearestEnemy.position - army.position).magnitude;
            float maxDist = DamageInfo.AttackRange(DamageType.Range);

            if (dist < DamageInfo.AttackRange(DamageType.Range) &&
                myRangeAttack.RangeDamage > 0 &&
                myRangeAttack.RangeDamage * 2 > enemyRangeAttack.RangeDamage
                )
            {
                return dist / maxDist;
            }

            return 0;
        }
        return 0;
    }

    private void AttackRegion()
    {
        curTarget = targets.enemyRegion;
        curDamageType = DamageType.Melee;
    }
    private float BackMyRegion()
    {
        if (targets.enemyRegion != null)
        {
            var r = (Region)targets.enemyRegion;
            if (r.owner == owner && r.data.garnison.Count * 2 < army.army.Count)
                return 0.9f;
        }
        return 0;
    }

    private void Idle() { }
    private float UsefulHealIdle()
    {
        if (army.inTown)
            return (1 - curHP) * 1.2f;
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
}

public class ArmyTargets
{
    public List<IFightable> enemyArmies;
    public IFightable enemyRegion;
    public IFightable myRegionForDefend;

    public ArmyTargets()
    {
        enemyArmies = new List<IFightable>();
    }

    public void Remove(IFightable target)
    {
        if (enemyRegion == target)
        {
            enemyRegion = null;
        }
        else if (myRegionForDefend == target)
        {
            myRegionForDefend = null;
        }
        else
        {
            enemyArmies.Remove(target);
        }
    }
}
