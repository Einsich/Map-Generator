using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action:System.IComparable<Action> {
    public int time,startTime;
    protected float T;
    public bool actually;
    public TypeAction type;
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
        Date.actionQueue.Enqueue(this);
    }

}
public enum TypeAction
{
    BuildAction,
    RecruitAction,
    PersonAliveAction
}
public class BuildAction:Action
{
   public ProvinceData prdata;
    public BuildAction(ProvinceData reg,int dt):base(dt)
    {
        type = TypeAction.BuildAction;
        prdata = reg;
    }
}
public class RecruitAction : Action
{
    public ProvinceData prdata;
    public BaseRegiment regiment;
    public RecruitAction(ProvinceData prov,BaseRegiment reg, int dt) : base(dt)
    {
        type = TypeAction.RecruitAction;
        prdata = prov;
        regiment = reg;
    }
}

public class PersonAliveAction : Action
{
    public Person person;
    public PersonAliveAction(Person person, int dt) : base(dt)
    {
        type = TypeAction.PersonAliveAction;
        this.person = person;
    }
}