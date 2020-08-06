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
            case DamageType.Range: return 30 + (damageLvl + buff) * 15;
            default: return 1 + damageLvl;
        }
    }

    public float time => bufTime.HasValue ? bufTime.Value : (bufTime = TimeUpdate()).Value;
    public Treasury cost => bufCost.HasValue ? bufCost.Value : (bufCost = CostUpdate()).Value;
    public Treasury upkeep => bufUpkeep.HasValue ? bufUpkeep.Value : (bufUpkeep = UpkeepUpdate()).Value;
    public State owner;
    private Treasury? bufCost, bufUpkeep;
    private float? bufTime;
    public void AllUpdate()
    {
        bufCost = CostUpdate();
        bufUpkeep = UpkeepUpdate();
        bufTime = TimeUpdate();
    }
    private Treasury CostUpdate()
    {
        int armor = MeleeArmor + ChargeArmor + RangeArmor;
        float gold = 0;
        float wood = (damageType == DamageType.Range? damageLvl * 1.0f : 0) + (type == RegimentType.Artillery ? 10 : 0);
        float iron = 0;
        switch(type)
        {
            case RegimentType.Infantry: gold = 5 + 1.0f * armor + 2.0f * damageLvl; iron = 1 + 0.5f * armor + 0.5f * damageLvl; break;
            case RegimentType.Cavalry: gold = 10 + 2.0f * armor + 4.0f * damageLvl; iron = 3 + 1.0f * armor + 1.6f * damageLvl; break;
            case RegimentType.Artillery: gold = 30 + 2.0f * armor + 10.0f * damageLvl;break;
        }
        return new Treasury(gold, maxcount, wood, iron, 0);
    }
    private Treasury UpkeepUpdate()
    {
        int armor = MeleeArmor + ChargeArmor + RangeArmor;
        float gold = 0;
        float wood = (damageType == DamageType.Range ? damageLvl * 0.1f : 0);
        float iron = 0;
        switch (type)
        {
            case RegimentType.Infantry: gold = 0.4f + 0.2f * armor + 0.2f * damageLvl; iron = 0.05f * (armor+ damageLvl); break;
            case RegimentType.Cavalry: gold = 0.8f + 0.3f * armor + .4f * damageLvl; iron = 0.1f + 0.1f * (armor + damageLvl); break;
            case RegimentType.Artillery: gold = 2 + 0.5f * armor + 1.0f * damageLvl; break;
        }
        return new Treasury(gold, 0, wood, iron, 0);
    }
    private float TimeUpdate()
    {
        int armor = MeleeArmor + ChargeArmor + RangeArmor;
        float time = 0;
        switch (type)
        {
            case RegimentType.Infantry: time = 1.4f + 0.2f * armor + 0.2f * damageLvl; break;
            case RegimentType.Cavalry: time = 1.8f + 0.3f * armor + .4f * damageLvl; break;
            case RegimentType.Artillery: time = 10 + 0.5f * armor + 1.0f * damageLvl; break;
        }
        switch(damageType)
        {
            case DamageType.Melee:break;
            case DamageType.Charge:time *= 2f; break;
            case DamageType.Range:time *= 1.5f; break;
        }
        return time;
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
