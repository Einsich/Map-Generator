using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action:System.IComparable<Action> {
    public int time,startTime;
    protected float T;
    public TypeAction type;
    public float progress => (Date.curdate - startTime) * T;
    public int CompareTo(Action other)
    {
        return time - other.time;
    }
}
public enum TypeAction
{
    BuildAction,
    RecruitAction
}
public class BuildAction:Action
{
   public ProvinceData prdata;
    public bool actually;
    public BuildAction(ProvinceData reg,int dt)
    {
        type = TypeAction.BuildAction;
        time = Date.curdate + dt;
        prdata = reg;
        actually = true;
        startTime = Date.curdate;
        T = 1f / dt;
        Date.actionQueue.Enqueue(this);
    }
}
public class RecruitAction : Action
{
    public ProvinceData prdata;
    public BaseRegiment regiment;
    public bool actually;
    public RecruitAction(ProvinceData prov,BaseRegiment reg, int dt)
    {
        type = TypeAction.RecruitAction;
        time = Date.curdate + dt;
        prdata = prov;
        regiment = reg;
        actually = true;
        startTime = Date.curdate;
        T = 1f / dt;
        Date.actionQueue.Enqueue(this);

    }
}