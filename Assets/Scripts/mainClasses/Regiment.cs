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
public class BaseRegiment
{
    public int[] armor;
    public Tech[] teches;
    public RegimentType type;
    public DamageType damageType;
    public float maxcount = 1000;
    public float damage;

    public int time;
    public Treasury cost, upkeep;
    public Sprite Icon => SpriteHandler.GetRegimentSprite(this);
    public State owner;
    public BaseRegiment(State state, RegimentType type, DamageType damageType,int MeleeArmor,int ChargeArmor, int RangeArmor, int SiegeArmor, float damage, float maxcount,
        Treasury cost, Treasury upkeep, int time)
    {
        owner = state;
        this.type = type;
        this.damageType = damageType;
        this.damage = damage;
        armor = new int[(int)DamageType.Count] { MeleeArmor, ChargeArmor, RangeArmor, SiegeArmor };
        this.maxcount = maxcount;
        this.cost = cost;
        this.upkeep = upkeep;
        this.time = time;
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
