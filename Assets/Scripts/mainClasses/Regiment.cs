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
    public Tech[] armTeches;
    public RegimentType type;
    public DamageType damageType;
    public RegimentName name;
    public float maxcount = 1000;
    public int MeleeArmor, ChargeArmor, RangeArmor, SiegeArmor;
    public int damageLvl;
    public float damage(int buff) => 50 + (damageLvl + buff) * 20;
    public Tech damTech;

    public float time;
    public Treasury cost, upkeep;
    public Sprite Icon => SpriteHandler.GetRegimentSprite(this);
    public State owner;

    public Treasury GetBonusUpkeep()
    {
        return upkeep * (owner != null ? owner.technology.UpkeepBonus : 1);
    }

    public int ArmorLvl(DamageType type)
    {
        switch(type)
        {
            case DamageType.Melee:return MeleeArmor;
            case DamageType.Charge:return ChargeArmor;
            case DamageType.Range:return RangeArmor;
            case DamageType.Siege:return SiegeArmor;
            default:return 0;
        }
    }
    public BaseRegiment(State state,RegimentName name, RegimentType type, DamageType damageType,int MeleeArmor,int ChargeArmor, int RangeArmor, int SiegeArmor, int damage, float maxcount,
        Treasury cost, Treasury upkeep, float time)
    {
        owner = state;
        this.type = type;
        this.name = name;
        this.damageType = damageType;
        this.damageLvl = damage;
        this.MeleeArmor = MeleeArmor;
        this.ChargeArmor = ChargeArmor;
        this.RangeArmor = RangeArmor;
        this.SiegeArmor = SiegeArmor;
        armTeches = new Tech[(int)DamageType.Count];
        this.maxcount = maxcount;
        this.cost = cost;
        this.upkeep = upkeep;
        this.time = time;
    }
    public override string ToString()
    {
        return string.Format("{0}\nУменьшают урон в ближнем бою {6}({1}), от кавалерии {7}({2}), от стрелков {8}({3}), от артиллерии {9}({4}). Чистый урон {10}({5}).",
            GetDescription(name), DamageInfo.Armor(MeleeArmor).ToString("N2"), DamageInfo.Armor(ChargeArmor).ToString("N2"), DamageInfo.Armor(RangeArmor).ToString("N2"),
            DamageInfo.Armor(SiegeArmor).ToString("N2"), damage(damageLvl).ToString("N2"),
            MeleeArmor, ChargeArmor, RangeArmor, SiegeArmor, damageLvl);
    }
    static string GetDescription(RegimentName name)
    {
        switch(name)
        {
            case RegimentName.SimpleMelee:return "Базовая пехота.\n Эти солдаты всегда составляют костяк армии из-за их дешевизны.";
            case RegimentName.SimpleRanger:return "Лучники.\n Ведение огня на дистанции может дать значительное тактическое преимущество.";
            case RegimentName.SimpleCavalry:return "Кавалерия.\n Обладают повышенной скоростью передвижения, могут быть использованы для преследования и нанесения сокрушительных ударов.";
            case RegimentName.SimpleArta:return "Осадные машины.\n Могут обстреливать врага с большой дистанции, но эффективны лишь против городов.";
            case RegimentName.Skeletons:return "Скелеты.\n Сила некроманта подняла мертвых снова на битву.";
            default: return "";
        }
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
