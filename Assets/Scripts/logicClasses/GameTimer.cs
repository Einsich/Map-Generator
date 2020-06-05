using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;

public static class GameTimer 
{
    public static float time, timeScale = 1;
    public static void Start()
    {
        foreach (var x in Main.states)
        {
            foreach (var s in x.regions)
            {
                x.Income += s.data.CalculateIncome();
            }
            GlobalTrade.AddIncome(x.Income);
        }
        GlobalTrade.StartGlobalTrade();
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
        Army.ProcessAllArmyAI();

        foreach (var army in Army.AllArmy)
            army.DeciUpdateTime();
    }
    public static void EverySecondUpdate()
    {
        foreach (var army in Army.AllArmy)
            army.UpdateManpower();
    }
    public static void EveryDecaSecondUpdate()
    {
        GlobalTrade.DiscardCource();
    }

    public static List<(UnityAction,State)> first = new List<(UnityAction, State)>(),
        second = new List<(UnityAction, State)>();
    public static void AddListener(UnityAction action, State who)
    {
        if(who == Player.curPlayer)
        {
            first.Add((action,who));
        } 
        else
        {
            second.Add((action, who));
        }
    }
    public static void RemoveListener(UnityAction action, State who)
    {
        if (who == Player.curPlayer)
        {
            first.Remove((action, who));
        }
        else
        {
            second.Remove((action, who));
        }
    }
    public static void UpdateListenerQueue()
    {
        first.AddRange(second.FindAll((x) => x.Item2 == Player.curPlayer));
        second.RemoveAll((x) => x.Item2 == Player.curPlayer);
        second.AddRange(first.FindAll((x) => x.Item2 != Player.curPlayer));
        first.RemoveAll((x) => x.Item2 != Player.curPlayer);
    }

    public static event System.Action OftenUpdate;
}
