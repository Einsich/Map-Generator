using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewRegiment", menuName = "NewRegiment", order = 2)]
public class BaseRegiment : ScriptableObject
{
    public RegimentType type;
    public DamageType damageType;
    public string nameRegiment;
    public string description;
    public Sprite Icon;
    public float maxcount = 1000;
    public int MeleeArmor, ChargeArmor, RangeArmor;
    public int damageLvl;
    public float damage(int buff)
    {
        switch (damageType)
        {
            case DamageType.Melee: return 50 + (damageLvl + buff) * 20;
            case DamageType.Charge: return 70 + (damageLvl + buff) * 30;
            case DamageType.Range: return 30 + (damageLvl + buff) * 10;
            default: return damageLvl;
        }
    }

    public float time;
    public Treasury cost, upkeep;
    public State owner;

    public Treasury GetBonusUpkeep()
    {
        return upkeep * (owner != null ? owner.technologyTree.regimentUpkeepReduce : 1);
    }

    public int ArmorLvl(DamageType type)
    {
        switch (type)
        {
            case DamageType.Melee: return MeleeArmor;
            case DamageType.Charge: return ChargeArmor;
            case DamageType.Range: return RangeArmor;
            default: return 0;
        }
    }
    public BaseRegiment(State state, RegimentType type, DamageType damageType, int MeleeArmor, int ChargeArmor, int RangeArmor, int SiegeArmor, int damage, float maxcount,
        Treasury cost, Treasury upkeep, float time)
    {
        owner = state;
        this.type = type;
        this.damageType = damageType;
        this.damageLvl = damage;
        this.MeleeArmor = MeleeArmor;
        this.ChargeArmor = ChargeArmor;
        this.RangeArmor = RangeArmor;
        this.maxcount = maxcount;
        this.cost = cost;
        this.upkeep = upkeep;
        this.time = time;
    }
    public override string ToString()
    {
        return string.Format("{0}\nУменьшают урон в ближнем бою {5}({1}), от кавалерии {6}({2}), от стрелков {7}({3}). Чистый урон {8}({4}).",
            GetDescription(), DamageInfo.Armor(MeleeArmor).ToString("N2"), DamageInfo.Armor(ChargeArmor).ToString("N2"), DamageInfo.Armor(RangeArmor).ToString("N2"),
            damage(damageLvl).ToString("N2"),
            MeleeArmor, ChargeArmor, RangeArmor, damageLvl);
    }
    public string GetDescription()
    {
        return string.Format("{0}.\n{1}", nameRegiment, description);
        /*
        switch()
        {
            case RegimentName.SimpleMelee:return "Базовая пехота.\n Эти солдаты всегда составляют костяк армии из-за их дешевизны.";
            case RegimentName.SimpleRanger:return "Лучники.\n Ведение огня на дистанции может дать значительное тактическое преимущество.";
            case RegimentName.SimpleCavalry:return "Кавалерия.\n Обладают повышенной скоростью передвижения, могут быть использованы для преследования и нанесения сокрушительных ударов.";
            case RegimentName.SimpleArta:return "Осадные машины.\n Могут обстреливать врага с большой дистанции, но эффективны лишь против городов.";
            case RegimentName.Skeletons:return "Скелеты.\n Сила некроманта подняла мертвых снова на битву.";
            default: return "";
        }*/
    }

}
