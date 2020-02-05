using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageInfo 
{
    public float[] damage;
    public float MeleeDamage { get => damage[(int)DamageType.Melee]; set => damage[(int)DamageType.Melee] = value; }
    public float RangeDamage { get => damage[(int)DamageType.Range]; set => damage[(int)DamageType.Range] = value; }
    public float ChargeDamage { get => damage[(int)DamageType.Charge]; set => damage[(int)DamageType.Charge] = value; }
    public float SiegeDamage { get => damage[(int)DamageType.Siege]; set => damage[(int)DamageType.Siege] = value; }

    public DamageInfo()
    {
        damage = new float[(int)DamageType.Count];
    }
    
}
