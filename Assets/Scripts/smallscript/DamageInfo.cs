using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageInfo 
{
    public float[] damage;

    public float MeleeDamage { get => damage[(int)DamageType.Melee]; set => damage[(int)DamageType.Melee] = value; }
    public float RangeDamage { get => damage[(int)DamageType.Range]; set => damage[(int)DamageType.Range] = value; }
    public float ChargeDamage { get => damage[(int)DamageType.Charge]; set => damage[(int)DamageType.Charge] = value; }
    public int SiegeDamage { get => (int)damage[(int)DamageType.Siege]; set => damage[(int)DamageType.Siege] = value; }
    public float TotalDamage => MeleeDamage + RangeDamage + ChargeDamage;
    public DamageInfo()
    {
        damage = new float[(int)DamageType.Count];
    }
    static float[] Ranges = new float[(int)DamageType.Count] { 2, 2, 4, 6 };
    public static float AttackRange(DamageType type)
    {
        return Ranges[(int)type];
    }
    public static float Armor(int armor)
    {
        return 1 / (0.4f * armor + 1);
    }
    
}
