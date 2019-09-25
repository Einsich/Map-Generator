using System;
using System.Collections.Generic;
using UnityEngine;

public class Action: IComparable<Action> {
    public int time,startTime;
    protected float T;
    public bool actually;
    public TypeAction type;
    protected delegate void Method();
    protected event Method onAction;
    public float progress => (Date.curdate - startTime) * T;

    public int CompareTo(Action other)
    {
        return time - other.time;
    }
    public Action(int dt)
    {
        time = Date.curdate + dt;
        actually = true;
        startTime = Date.curdate;
        T = 1f / dt;
        GameTimer.actionQueue.Enqueue(this);
    }
    public void DoAction()
    {
        onAction();
    }

}
public enum TypeAction
{
    BuildAction,
    RecruitAction,
    PersonAliveAction,
    SiegeAction
}
public class BuildAction:Action
{
    public BuildAction(ProvinceData reg,int dt):base(dt)
    {
        type = TypeAction.BuildAction;
        onAction += () => reg.BuildComplete();
    }
}
public class RecruitAction : Action
{
    public BaseRegiment regiment;
    public RecruitAction(ProvinceData prov,BaseRegiment reg, int dt) : base(dt)
    {
        type = TypeAction.RecruitAction;
        regiment = reg;
        onAction += () =>  prov.RecruitRegiment(this);
    }
}

public class PersonAliveAction : Action
{
    public PersonAliveAction(Person person, int dt) : base(dt)
    {
        type = TypeAction.PersonAliveAction;
        onAction += () => person.Alive();
    }
}
public class SiegeAction : Action
{
    public SiegeAction(Region region, int dt) : base(dt)
    {
        type = TypeAction.SiegeAction;
        onAction += () => region.WinSiege();
    }
}