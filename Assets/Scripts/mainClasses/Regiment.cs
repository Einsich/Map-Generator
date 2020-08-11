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

    public static float GetMediumCount(IEnumerable<Regiment> formation)
    {
        float count = 0, max = 0;
        foreach (Regiment r in formation)
        {
            count += r.count;
            max += r.baseRegiment.maxcount;
        }
        return max == 0 ? 0 : count / max;
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
