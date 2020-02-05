using System;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer 
{
    public static void Start()
    {
        foreach (var state in Main.states)
            state.CalculatePersonImpact();
        foreach (var x in Main.regions)
            x.data?.CalculateIncome();
    }

    public static PriorityQueue<Action> actionQueue = new PriorityQueue<Action>();

    public static void DayUpdate(int curdate)
    {
        while (actionQueue.Count > 0 && actionQueue.Peek().time <= curdate)
        {
            Action action = actionQueue.Dequeue();
            if (!action.actually)
                continue;
            action.DoAction();
        }
       // Battle.ProcessAllBattles();
        Army.ProcessAllArmy();
        //BattleInterface.needUpdate = true;
        ProvinceMenu.needUpdate = true;

    }
    public static void MonthUpdate()
    {
        foreach (var state in Main.states)
            state.CalculatePersonImpact();
        for (int i = 0; i < Main.regions.Count; i++)
            Main.regions[i].MonthUpdate();
        foreach (var army in Army.AllArmy)
            army.UpdateUpkeep();
        Army.ProcessAllArmyAI();
        Diplomacy.DiplomacyUpdate();
        FreeMonthUpdate?.Invoke();
    }
    public static event System.Action FreeMonthUpdate;
}
