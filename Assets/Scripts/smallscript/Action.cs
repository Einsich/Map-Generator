using System;
using System.Collections.Generic;
using UnityEngine;

public class GameAction: IComparable<GameAction> {
    readonly public float time,startTime;
    protected float T;
    public bool actually;
    //protected delegate void Method();
    public System.Action  onAction;
    public float progress => (GameTimer.time - startTime) * T;

    public int CompareTo(GameAction other)
    {
        return time.CompareTo(other.time);
    }
    public GameAction(float dt)
    {
        time = GameTimer.time + dt;
        actually = true;
        startTime = GameTimer.time;
        T = 1f / dt;
        GameTimer.actionQueue.Enqueue(this);
    }
    public GameAction(float dt, System.Action GameAction)
    {
        onAction = GameAction;
        time = GameTimer.time + dt;
        actually = true;
        startTime = GameTimer.time;
        T = 1f / dt;
        GameTimer.actionQueue.Enqueue(this);
    }
    public void DoAction()
    {
        onAction();
    }

}
public enum TypeGameAction
{
    BuildGameAction,
    RecruitGameAction,
    PersonAliveGameAction,
    SiegeGameAction
}
public class RecruitAction : GameAction
{
    public BaseRegiment regiment;
    public RecruitAction(ProvinceData prov,BaseRegiment reg, float dt) : base(dt)
    {
        //type = TypeGameAction.RecruitGameAction;
        regiment = reg;
        onAction += () =>  prov.RecruitRegiment(this);
    }
}
