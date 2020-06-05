using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
                //GameTimer.AddListener(TryTrade, state.Data);

            }
            else
            {
                //GameTimer.RemoveListener(TryTrade, state.Data);
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

        foreach(Army a in stateAI.Data.army)
        {
            a.AI.strategicStay = stateAI.Data.regions.Last().Capital;
        }
    }

    private void CollectEnemies()
    {
        foreach (Diplomacy enemyDiplomacy in stateAI.Data.diplomacy.war)
        {
            enemies.AddRange(enemyDiplomacy.state.army);
        }
    }

    private Army DetectEnemyOnTerritory()
    {
        foreach ()
    }
}
