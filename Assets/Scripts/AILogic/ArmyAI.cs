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

    private bool standInTown = false;
    private bool berserkerMode = false;
    private DamageType priorityDamage = DamageType.Melee;

    private Region targetDef;

    private SortedDictionary<float, IFightable> comparator = new SortedDictionary<float, IFightable>();

    private bool isPeace => owner.diplomacy.war.Count == 0;
    public void Logic()
    {
        Tactical();
    }

    private void Tactical()
    {
        RemoveInvalidTargets();

        targetDef = owner.stateAI.autoArmyCommander.targets[this].defends.FirstOrDefault();

        HealMonitoring();
        RefreshPriorityDamage();

        if (isPeace)
        {
            StrategicAction();
        }
        else
        {
            Tactic();
        }
    }

    private void RemoveInvalidTargets()
    {
        var targets = owner.stateAI.autoArmyCommander.targets[this];

        targets.armies.RemoveAll(a => a.Destoyed);
        targets.regions.RemoveAll(r => r.ocptby != null && !r.ocptby.diplomacy.haveWar(owner.diplomacy));
    }

    private void HealMonitoring()
    {
        float hp = army.MediumCount();
        if (berserkerMode)
        {
            if (hp >= 0.5f)
            {
                berserkerMode = false;
            }
        }
        else
        {
            if (hp < 0.4f)
            {
                standInTown = true;
            }
            else if (hp > 0.95f && army.Person.MaxRegiment <= army.army.Count)
            {
                standInTown = false;
            }
        }
    }

    private void RefreshPriorityDamage()
    {
        if (regimentCount != army.army.Count)
        {
            regimentCount = army.army.Count;
            PriorityDamage();
        }
    }

    private void PriorityDamage()
    {
        DamageInfo info = army.GetDamage();

        float artaCount = 0;
        foreach (var r in army.army)
        {
            if (r.baseRegiment.damageType == DamageType.Siege) artaCount++;
        }

        if (artaCount / army.Person.MaxRegiment > 0.5f)
        {
            priorityDamage = DamageType.Siege;
        }
        else if (info.RangeDamage * 3 >= info.MeleeDamage)
        {
            priorityDamage = DamageType.Range;
        }
        else
        {
            priorityDamage = DamageType.Melee;
        }
    }

    private void StrategicAction()
    {
        var targets = owner.stateAI.autoArmyCommander.targets[this];

        Army targetArmy = targets.armies.FirstOrDefault(a => a.curReg.owner == owner);
        Region targetRegion = targets.regions.FirstOrDefault(r => r.ocptby == null || r.owner.diplomacy.haveWar(owner.diplomacy));

        if (targetArmy != default(Army) && priorityDamage != DamageType.Siege)
        {
            if (isNotMoveToTarget(targetArmy))
                if (!army.TryMoveToTarget(targetArmy, priorityDamage))
                    targets.armies.Remove(targetArmy);
        }
        else if (targetRegion != default(Region))
        {
            if (targetRegion.data.garnison.Count > 0)
            {
                if (isNotMoveToTarget(targetRegion))
                    if (!army.TryMoveToTarget(targetRegion, priorityDamage))
                        targets.regions.Remove(targetRegion);
            }
            else
            {

                if (isNotMoveToTarget(targetRegion))
                    if (!army.TryMoveToTarget(targetRegion, DamageType.Melee))
                        targets.regions.Remove(targetRegion);
            }
        }
        else if (targetDef != default(Region))
        {
            if (isNotMoveToCoord(targetDef.Capital))
                if (!army.TryMoveTo(targetDef.Capital))
                    targets.defends.Remove(targetDef);
        }
        else
        {
            GoToTown();
        }
    }

    private bool isNotMoveToTarget(IFightable target)
    {
        return army.navAgent.target == null || !army.navAgent.target.Equals(target);
    }

    private bool isNotMoveToCoord(Vector2Int coord)
    {
        return army.navAgent.path == null ||
            !army.navAgent.path.Contains(coord);
    }

    private void GoToTown() //переделать если будет накорректное возвращение
    {
        comparator.Clear();
        foreach (Region r in owner.regions)
        {
            float dist = (r.Capital - army.curPosition).magnitude;
            if (!comparator.ContainsKey(dist))
                comparator.Add(dist, r);
        }
        Region target = comparator.First().Value as Region;

        if (!army.TryMoveTo(target.Capital))
        {
            berserkerMode = true;
        }
        else
        {
            owner.stateAI.autoArmyCommander.targets[this].defends.Add(target);
        }
    }

    private void Tactic()
    {
        if (standInTown && !berserkerMode)
        {
            if (army.inTown)
            {
                if (army.ActionState == ActionType.Idle)
                {
                    IFightable targetArmy = SearchEnemyInRadius(army.AttackRange);

                    if (targetArmy != null)
                        army.TryMoveToTarget(targetArmy, army.maxRangeType);
                }
            }
            else if (targetDef != default(Region))
            {
                if (isNotMoveToCoord(targetDef.Capital))
                    if (!army.TryMoveTo(targetDef.Capital))
                        owner.stateAI.autoArmyCommander.targets[this].defends.Remove(targetDef);
            }
            else
            {
                GoToTown();
            }
        }
        else
        {
            if (priorityDamage != DamageType.Siege)
            {
                IFightable targetArmy = SearchEnemyInRadius(DamageInfo.AttackRange(DamageType.Range));

                if (isNotMoveToTarget(targetArmy))
                {
                    if (targetArmy != null && army.ActionState != ActionType.Attack)
                    {
                        army.TryMoveToTarget(targetArmy, priorityDamage);
                    }
                    else
                    {
                        StrategicAction();
                    }
                }
            }
            else
            {
                StrategicAction();
            }
        }
    }

    private IFightable SearchEnemyInRadius(float radius)
    {
        comparator.Clear();
        foreach (Army enemy in owner.stateAI.autoArmyCommander.targets[this].armies)
        {
            float dist = (enemy.curPosition - army.curPosition).magnitude;
            if (dist <= radius && !enemy.Destoyed)
            {
                if (!comparator.ContainsKey(dist))
                    comparator.Add(dist, enemy);
            }
        }

        if (comparator.Count > 0)
        {
            return comparator.First().Value;
        }
        else
        {
            return null;
        }
    }
}
