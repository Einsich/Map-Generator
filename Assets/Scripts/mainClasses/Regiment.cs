using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Regiment
{

    public float count, loses;
    public BaseRegiment baseRegiment;

    public float NormalCount => count / baseRegiment.maxcount;

    public Regiment()
    {

    }
    public Regiment(BaseRegiment t)
    {
        count = t.maxcount;
        baseRegiment = t;
    }
}

public enum DamageType
{
    Melee,
    Charge,
    Range,
    Siege,
    Count
}
public enum RegimentType
{
    Infantry,
    Cavalry,
    Artillery,
    Count
}
public enum RegimentName
{
    SimpleMelee,
    SimpleCavalry,
    SimpleRanger,
    SimpleArta,
    Skeletons
}
