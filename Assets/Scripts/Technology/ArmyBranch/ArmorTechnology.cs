using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorTechnology : Technology
{
    public int regimentOffset;
    public DamageType armorType;
    public override void LevelUp()
    {
        switch (armorType)
        {
            case DamageType.Range: tree.regiments[regimentOffset].RangeArmor++; break;
            case DamageType.Melee: tree.regiments[regimentOffset].MeleeArmor++; break;
            case DamageType.Charge: tree.regiments[regimentOffset].ChargeArmor++; break;
        }
    }
}
