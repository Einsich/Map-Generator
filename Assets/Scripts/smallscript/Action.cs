using System;
using System.Collections.Generic;
using UnityEngine;

public class Action: IComparable<Action> {
    public float time,startTime;
    protected float T;
    public bool actually;
    protected delegate void Method();
    protected event Method onAction;
    public float progress => (GameTimer.time - startTime) * T;

    public int CompareTo(Action other)
    {
        return time.CompareTo(other.time);
    }
    public Action(float dt)
    {
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
public enum TypeAction
{
    BuildAction,
    RecruitAction,
    PersonAliveAction,
    SiegeAction
}
public class BuildAction:Action
{
    public BuildAction(ProvinceData reg, float dt):base(dt)
    {
        //type = TypeAction.BuildAction;
        onAction += () => reg.BuildComplete();
    }
}
public class RecruitAction : Action
{
    public BaseRegiment regiment;
    public RecruitAction(ProvinceData prov,BaseRegiment reg, float dt) : base(dt)
    {
        //type = TypeAction.RecruitAction;
        regiment = reg;
        onAction += () =>  prov.RecruitRegiment(this);
    }
}

public class PersonAliveAction : Action
{
    public PersonAliveAction(Person person, float dt) : base(dt)
    {
        //type = TypeAction.PersonAliveAction;
        onAction += () => person.Alive();
    }
}
public class SiegeAction : Action
{
    public SiegeAction(Region region, float dt) : base(dt)
    {
        //type = TypeAction.SiegeAction;
        onAction += () => region.WinSiege();
    }
}
public class ResearchAction : Action
{
    public ResearchAction(Tech tech, float dt):base(dt)
    {
        onAction += () => tech.research();
    }
}