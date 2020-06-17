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

    public StrategicCommand command = new StrategicCommand();

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
        RefreshPriorityDamage();
        TestStrategicTask();

        if (isPeace)
        {
            StrategicAction();
        }
        else
        {
            Tactic();
        }
    }

    private void TestStrategicTask()
    {
        if (command.attack != null)
        {
            if (command.attack is Region)
            {
                var tmp = (Region)command.attack;
                if (tmp.ocptby != null)
                    owner.stateAI.autoArmyCommander.Strategy();
            }
            else if (command.attack is Army)
            {
                var tmp = (Army)command.attack;
                if (tmp.Destoyed)
                    owner.stateAI.autoArmyCommander.Strategy();
            }
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
        if (command.attack != null)
        {
            if (isNotMoveToTarget(command.attack))
                if (!army.TryMoveToTarget(command.attack, priorityDamage))
                    command.attack = null;
        }
        else
        {
            if (isNotMoveToCoord(command.stay))
            {
                if (!army.TryMoveTo(command.stay))
                {
                    GoToTown();
                }
            }
        }
    }

    private bool isNotMoveToTarget(IFightable target)
    {
        return army.navAgent.target == null || !army.navAgent.target.Equals(target);
    }

    private bool isNotMoveToCoord(Vector2Int coord)
    {
        return army.navAgent.path == null ||
            army.navAgent.path.Count == 0 ||
            army.navAgent.path[0] != coord;
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
                if (isNotMoveToCoord(command.stay))
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

    public class StrategicCommand
    {
        public Vector2Int stay;
        public IFightable attack;

        public StrategicCommand() { attack = null; }
        public StrategicCommand(Vector2Int stay, IFightable attack)
        {
            this.stay = stay;
            this.attack = attack;
        }
    }
}
