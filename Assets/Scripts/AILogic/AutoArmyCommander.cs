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

    Treasury AutoManager.NeedTreasure => Treasury.zero;

    private StateAI stateAI;
    public AutoArmyCommander(StateAI stateAI)
    {
        this.stateAI = stateAI;
    }

    private List<IFightable> enemyArmies = new List<IFightable>();
    private IFightable enemyRegion;
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
                command.enemyRegion = enemyRegion;
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
                command.enemyRegion = enemyRegion;
                command.myRegionForDefend = myRegionForDefend;
            }
        }
    }
    private void CollectEnemies()
    {
        enemyArmies.Clear();
        foreach (Diplomacy enemyDiplomacy in stateAI.Data.diplomacy.war)
        {
            enemyArmies.AddRange(enemyDiplomacy.state.army);
        }
    }

    private void CollectTargetRegion()
    {
        foreach (Region r in stateAI.Data.regions)
        {
            if (r.ocptby != null && r.ocptby.diplomacy.haveWar(stateAI.Data.diplomacy))
            {
                enemyRegion = r;
                return;
            }
        }

        foreach (Diplomacy enemyDiplomacy in stateAI.Data.diplomacy.war)
        {
            foreach (Region r in enemyDiplomacy.state.regions)
            {
                if (neibOwnOwner(r) &&
                    (r.ocptby == null || r.ocptby.diplomacy.haveWar(stateAI.Data.diplomacy)))
                {
                    enemyRegion = r;
                    return;
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