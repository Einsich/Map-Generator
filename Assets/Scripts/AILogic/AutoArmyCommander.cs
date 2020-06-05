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

    public List<Army> enemies = new List<Army>();

    private void Strategy()
    {
        CollectEnemies();

        var stay = stateAI.Data.regions.Last().Capital;
        var attack = DetectEnemyOnTerritory();

        foreach (Army a in stateAI.Data.army)
        {
            if (attack != null)
            {
                a.AI.command = new StrategicCommand(stay, attack);
            }
            else
            {
                a.AI.command = new StrategicCommand(stay, DetectTargetTown());
            }
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

    private Army DetectEnemyOnTerritory()
    {
        foreach (Army a in enemies)
        {
            if (a.curReg.owner.Equals(stateAI.Data))
            {
                return a;
            }
        }
        return null;
    }

    private Region DetectTargetTown()
    {
        foreach (RegionProxi proxi in stateAI.autoRegimentBuilder.RiskI)
        {
            Region region = proxi.data.region;
            foreach (Region neib in region.neib)
            {
                if (stateAI.Data.diplomacy.haveWar(neib.owner.diplomacy))
                {
                    return neib;
                }
            }
        }
        return null;
    }
}
