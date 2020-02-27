using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameTimer 
{
    public static float time, timeScale = 1;
    public static void Start()
    {
        foreach (var state in Main.states)
            state.CalculatePersonImpact();
        foreach (var x in Main.regions)
            x.data?.CalculateIncome();
    }

    public static PriorityQueue<Action> actionQueue = new PriorityQueue<Action>();

    public static void RealTimeUpdate()
    {
        while (actionQueue.Count > 0 && actionQueue.Peek().time <= time)
        {
            Action action = actionQueue.Dequeue();
            if (!action.actually)
                continue;
            action.DoAction();
        }
    }
    public static void DeciSecondUpdate()
    {
       
        Army.ProcessAllArmy();
        OftenUpdate?.Invoke();

    }
    public static void EverySecondUpdate()
    {
        foreach (var army in Army.AllArmy)
            army.UpdateManpower();
    }
    public static void DecaSecondUpdate()
    {
        foreach (var state in Main.states)
            state.CalculatePersonImpact();
        for (int i = 0; i < Main.regions.Count; i++)
            Main.regions[i].MonthUpdate();
        foreach (var army in Army.AllArmy)
            army.UpdateUpkeep();
        foreach (var state in Main.states)
            state.CalculateIncome();
        Army.ProcessAllArmyAI();
        Diplomacy.DiplomacyUpdate();
        SeldomUpdate?.Invoke();
    }
    public static event System.Action SeldomUpdate;
    public static event System.Action OftenUpdate;
}
