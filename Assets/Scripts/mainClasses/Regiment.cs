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
    public Tech[] armTeches;
    public RegimentType type;
    public DamageType damageType;
    public float maxcount = 1000;
    public int damageLvl;
    public float damage => 50 + damageLvl * 20;
    public Tech damTech;

    public float time;
    public Treasury cost, upkeep;
    public Sprite Icon => SpriteHandler.GetRegimentSprite(this);
    public State owner;
    public BaseRegiment(State state, RegimentType type, DamageType damageType,int MeleeArmor,int ChargeArmor, int RangeArmor, int SiegeArmor, int damage, float maxcount,
        Treasury cost, Treasury upkeep, float time)
    {
        owner = state;
        this.type = type;
        this.damageType = damageType;
        this.damageLvl = damage;
        armor = new int[(int)DamageType.Count] { MeleeArmor, ChargeArmor, RangeArmor, SiegeArmor };
        armTeches = new Tech[(int)DamageType.Count];
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
