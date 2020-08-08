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
            case DamageType.Range: tree.state.regiments[regimentOffset].RangeArmor++; break;
            case DamageType.Melee: tree.state.regiments[regimentOffset].MeleeArmor++; break;
            case DamageType.Charge: tree.state.regiments[regimentOffset].ChargeArmor++; break;
        }
        tree.state.regiments[regimentOffset].AllUpdate();
    }
    public override string getDescription()
    {
        int armor = 0;
        string type = "";
        var regiment = tree.GetRegimentInstance(regimentOffset);
        switch (armorType)
        {
            case DamageType.Range: armor = regiment.RangeArmor; type = "стрелковых атак"; break;
            case DamageType.Melee: armor = regiment.MeleeArmor; type = "ближнего боя"; break;
            case DamageType.Charge: armor = regiment.ChargeArmor; type = "наскока кавалерии"; break;
        }
        string arm0 = DamageInfo.Armor(armor).ToString("N2");
        string arm1 = DamageInfo.Armor(armor + 1).ToString("N2");
        return string.Format("Улучшение защиты от {0}. Сейчас дает {1} защиты, после улучшения {2}.", type, arm0, arm1);
    }

}
