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

    private bool standInTown = false;
    private bool berserkerMode = false;
    private DamageType priorityDamage = DamageType.Melee;

    public StrategicCommand command;

    private SortedDictionary<float, IFightable> comparator = new SortedDictionary<float, IFightable>();

    private bool isPeace => owner.diplomacy.war.Count == 0;
    public void Logic()
    {
        Tactical();
        //army_.TryMoveTo(owner.regions[Random.Range(0, owner.regions.Count)].Capital);
    }

    private void Tactical()
    {
        HealMonitoring();

        PriorityDamage();
        if (isPeace)
        {
            StrategicAction();
        }
        else
        {
            Tactic();
        }
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
        if (command.attack != null)
        {
            if (army.navAgent.target == null || !army.navAgent.target.Equals(command.attack))
                if (!army.TryMoveToTarget(command.attack, priorityDamage))
                    command.attack = null;
        }
        else
        {
            if (army.navAgent.target == null || !army.navAgent.target.curPosition.Equals(command.stay))
            {
                if (!army.TryMoveTo(command.stay))
                {
                    GoToTown();
                }
            }
        }
    }

    private void GoToTown()
    {
        comparator.Clear();
        foreach (Region r in owner.regions)
        {
            float dist = (r.Capital - army.curPosition).magnitude;
            if (!comparator.ContainsKey(dist))
                comparator.Add(dist, r);
        }
        Vector2Int target = comparator.First().Value.curPosition;

        command.stay = target;
        if (!army.TryMoveTo(target))
        {
            berserkerMode = true;
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
            else
            {
                if (army.navAgent.target == null || !army.navAgent.target.curPosition.Equals(command.stay))
                {
                    GoToTown();
                }
            }
        }
        else
        {
            if (priorityDamage != DamageType.Siege)
            {
                IFightable targetArmy = SearchEnemyInRadius(DamageInfo.AttackRange(DamageType.Range));

                if (army.navAgent.target == null || !army.navAgent.target.Equals(targetArmy))
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
        foreach (Army enemy in owner.stateAI.autoArmyCommander.enemies)
        {
            float dist = (enemy.curPosition - army.curPosition).magnitude;
            if (dist <= radius)
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

    public struct StrategicCommand
    {
        public Vector2Int stay;
        public IFightable attack;

        public StrategicCommand(Vector2Int stay, IFightable attack)
        {
            this.stay = stay;
            this.attack = attack;
        }
    }
}
