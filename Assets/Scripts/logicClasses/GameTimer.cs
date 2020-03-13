﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;

public static class GameTimer 
{
    public static float time, timeScale = 1;
    public static void Start()
    {
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

    public static async void DecaSecondUpdate()
    {
        for (int j = 0; j < first.Count; j++)
            first[j].Item1();
        int i = 0;
        int n = Mathf.Max(second.Count / 50, 10);
        while (i < second.Count && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game")
        {
            for (int j = i; j < i + n && j < second.Count; j++)
                second[j].Item1();
            i += n;
            await Task.Delay((int)(Time.fixedDeltaTime * 1000));
        }
    }
    public static event System.Action OftenUpdate;
}
