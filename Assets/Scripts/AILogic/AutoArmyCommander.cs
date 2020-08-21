using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static AutoRegimentBuilder;
using static ArmyAI;

public class AutoArmyCommander : AutoManager
{
    private bool isOn = false;
    public bool IsOn
    {
        get => isOn; set
        {
            if (isOn == value)
                return;
            if (value)
            {
                GameTimer.AddListener(Strategy, stateAI.Data);
            }
            else
            {
                GameTimer.RemoveListener(Strategy, stateAI.Data);
            }
            isOn = value;
        }
    }


    private StateAI stateAI;
    public AutoArmyCommander(StateAI stateAI)
    {
        this.stateAI = stateAI;
    }

    private List<IFightable> enemyArmies = new List<IFightable>();
    private List<IFightable> enemyRegions = new List<IFightable>();
    private IFightable myRegionForDefend;
    public void DeclaredWar()
    {
        if (IsOn)
            Strategy();
    }
    public void Strategy()
    {
        CollectEnemies();
        CollectTargetRegion();
        CollectDefend();

        foreach (Army a in stateAI.Data.army)
        {
            var ai = a.AI;

            if (!a.Destoyed)
            {
                var command = ai.targets;
                command.enemyArmies.Clear();
                command.enemyArmies.AddRange(enemyArmies);
                command.enemyRegion.Clear();
                command.enemyRegion.AddRange(enemyRegions);
                command.myRegionForDefend = myRegionForDefend;
            }
        }
    }
    public void RecalculateRegions()
    {
        CollectTargetRegion();
        CollectDefend();

        foreach (Army a in stateAI.Data.army)
        {
            var ai = a.AI;

            if (!a.Destoyed)
            {
                var command = ai.targets;
                command.enemyRegion.Clear();
                command.enemyRegion.AddRange(enemyRegions);
                command.myRegionForDefend = myRegionForDefend;
            }
        }
    }
    private void CollectEnemies()
    {
        enemyArmies.Clear();
        foreach (var warDate in stateAI.Data.diplomacy.war)
        {
            State enemy = warDate.Enemy(stateAI.Data);
            enemyArmies.AddRange(enemy.army);
        }
    }

    private void CollectTargetRegion()
    {
        enemyRegions.Clear();
        foreach (Region r in stateAI.Data.regions)
        {
            if (r.curOwner.diplomacy.haveWar(stateAI.Data.diplomacy))
            {
                enemyRegions.Add(r);
            }
        }

        if (enemyRegions.Count > 0)
            return;

        foreach (var warDate in stateAI.Data.diplomacy.war)
        {
            State enemy = warDate.Enemy(stateAI.Data);
            foreach (Region r in enemy.regions)
            {
                if (neibOwnOwner(r) && r.curOwner.diplomacy.haveWar(stateAI.Data.diplomacy))
                {
                    enemyRegions.Add(r);
                }
            }
        }
    }

    private bool neibOwnOwner(Region region)
    {
        foreach (Region neib in region.neib)
        {
            if (neib.owner == stateAI.Data || neib.ocptby == stateAI.Data)
            {
                return true;
            }
        }
        return false;
    }

    private void CollectDefend()
    {
        foreach (RegionProxi r in stateAI.autoRegimentBuilder.RiskI)
        {
            Region region = r.data.region;
            if (neibOwnerEnemy(region) &&
                region.curOwner == stateAI.Data)
            {
                myRegionForDefend = region;
                return;
            }
        }

        if (myRegionForDefend == null)
            myRegionForDefend = stateAI.Data.regions.First();
    }

    private bool neibOwnerEnemy(Region r)
    {

        foreach (Region neib in r.neib)
        {

            if (!neib.iswater && neib.owner.diplomacy.haveWar(stateAI.Data.diplomacy))
                return true;
        }
        return false;
    }
}