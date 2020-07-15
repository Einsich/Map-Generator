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
                Strategy();
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

    public Dictionary<ArmyAI, ArmyTargets> targets = new Dictionary<ArmyAI, ArmyTargets>();

    private List<Army> enemies = new List<Army>();
    private List<Region> regions = new List<Region>();
    private List<Region> defends = new List<Region>();

    public void Strategy()
    {
        ActualizeCommandsList();

        CollectEnemies();
        CollectTargetRegion();
        CollectDefend();

        foreach (var t in targets)
        {
            if (!t.Key.army.Destoyed)
            {
                var command = t.Value;
                command.armies.Clear();
                command.armies.AddRange(enemies);
                command.regions.Clear();
                command.regions.AddRange(regions);
                command.defends.Clear();
                command.defends.AddRange(defends);
            }
        }
    }

    private void ActualizeCommandsList()
    {
        foreach (Army army in stateAI.Data.army)
        {
            var ai = army.AI;

            if (!targets.ContainsKey(ai))
                targets.Add(ai, new ArmyTargets());
        }
    }

    private void CollectEnemies()
    {
        enemies.Clear();
        foreach (Diplomacy enemyDiplomacy in stateAI.Data.diplomacy.war)
        {
            enemies.AddRange(enemyDiplomacy.state.army);
        }
    }

    private void CollectTargetRegion()
    {
        regions.Clear();
        foreach (Region r in stateAI.Data.regions)
        {
            if (r.ocptby != null && r.ocptby.diplomacy.haveWar(stateAI.Data.diplomacy))
                regions.Add(r);
        }

        if (regions.Count > 0)
            return;

        foreach (Diplomacy enemyDiplomacy in stateAI.Data.diplomacy.war)
        {
            foreach (Region r in enemyDiplomacy.state.regions)
            {
                if (neibOwnOwner(r) &&
                    (r.ocptby == null || r.ocptby.diplomacy.haveWar(stateAI.Data.diplomacy)))
                {
                    regions.Add(r);
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
        defends.Clear();

        foreach (RegionProxi r in stateAI.autoRegimentBuilder.RiskI)
        {
            if (neibOwnerEnemy(r.data.region))
                defends.Add(r.data.region);
        }

        if (defends.Count == 0)
            defends.Add(stateAI.Data.regions.First());
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
public class ArmyTargets
{
    public readonly List<Army> armies;
    public readonly List<Region> regions;
    public readonly List<Region> defends;

    public ArmyTargets()
    {
        armies = new List<Army>();
        regions = new List<Region>();
        defends = new List<Region>();
    }
}
