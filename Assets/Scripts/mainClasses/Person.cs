using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person
{
    public Sprite icon => SpriteHandler.GetPersonIcon(personType);
    static string[] names = {  "Yan", "Yanis", "Yanka", "Yanchik", "Yanushka", "Yandex" };

    public string name;
    public PersonType personType;
    public bool die, inTavern = true;
    public PersonAliveAction alive = null;
    public Army curArmy;
    public State owner;

    public int LeadershipLvl = 0;
    public int AttackSpeedLvl = 0;
    public int RangeBuffLvl = 0;
    public int MeleeBuffLvl = 0;
    public int MaxRegiment => LeadershipLvl * 2 + 4;
    public float AttackSpeed => 1f / (0.1f * (lvl+ AttackSpeedLvl) + 1f);

    public int DamageTypeBuff(DamageType type) => (type == DamageType.Melee || type == DamageType.Charge) ? MeleeBuffLvl : (type == DamageType.Range ? RangeBuffLvl: 0);
    /*
     сл. хар-ки имееют целую величину = lvl от 0 до какого-то предельного значения.
     Лидерство(макс. кол-во отрядов = lvl * 2 + 2)
    Скорость атаки = 1/(0.1f*lvl+1)
Баф на дальную атаку +lvl к damageLvl всех с Range атакой
Баф на ближнюю атаку +lvl к damageLvl всех с Melee|Charge атакой
Уменьшение урона
         */
    public List<Effect> Effects = new List<Effect>();
    public Skill[] Skills;
    
    public bool HaveEffect(EffectType type, out Effect effect)
    {
        for (int i = 0; i < Effects.Count; i++)
            if (Effects[i].type == type)
            {
                effect = Effects[i];
                return true;
            }
        effect = null;
        return false;
    }
    public bool HaveSkill(SkillType type, out Skill skill)
    {
        for (int i = 0; i < Skills.Length; i++)
            if (Skills[i].type == type)
            {
                skill = Skills[i];
                return true;
            }
        skill = null;
        return false;
    }
    public void UseSkillOnHim(Effect effect)
    {
        Effects.Add(effect);
    }
    public void StopUseSkillOnHim(Effect effect)
    {
        Effects.Remove(effect);
    }
    public void LearnSkill(int i)
    {
        Skills[i].LevelUp();
        if(!Skills[i].Active && Skills[i].GetLevel == 1)
        {
            Skills[i].UseSkill(this);
        }
        lvlPoint--;
    }
    public void LearnProperty(PropertyType type)
    {
        switch(type)
        {
            case PropertyType.Leadership:LeadershipLvl++;break;
            case PropertyType.AttackSpeed:AttackSpeedLvl++;break;
            case PropertyType.MeleeBuff:MeleeBuffLvl++;break;
            case PropertyType.RangeBuff:RangeBuffLvl++;break;
        }
        lvlPoint--;
    }
    public void Die()
    {
        die = true;
        inTavern = true;
    }
    public void Alive()
    {
        die = false;
        alive = null;
        //curArmy
    }
    public void SetToCapital()
    {
        if (curArmy == null)
            curArmy = Army.CreateArmy(owner.Capital, owner.defaultArmy(), this);
        else
            curArmy.InitArmy(new List<Regiment>(), owner.Capital, this);
        inTavern = false;
    }
    public Person(State state)
    {
        owner = state;
        owner.persons.Add(this);
        name = names[Random.Range(0, names.Length)];
        die = Random.Range(0, 2) == 1;
        die = false;

        GameTimer.OftenUpdate += () => exp += (1 + state.technologyTree.passiveExperience) * state.technologyTree.activeExperienceBonus;

    }
    virtual public void NewLevel() { lvlPoint++;}

    float exp_ = 0;
    public int lvlPoint { get; private set; } = 0;
    public float exp
    {
        get => exp_; set
        {
            exp_ = value;
            while (nextLvl <= exp_)
            {
                exp_ -= nextLvl;
                lvl++;
                NewLevel();
            }
            ExpChanged?.Invoke();
        }
    }
    public System.Action ExpChanged, SkillCulldowned;
    public int lvl { get; private set; } = 1;
    public int nextLvl => lvl * 100;
    public float expf => 1f * exp_ / nextLvl;
    public int DamageLvlBuff(RegimentType regimentType, DamageType damageType)
    {
        Effect effect;
        int buf = 0;
        if(HaveEffect(EffectType.BraveSpeech, out effect))
        {
            buf += (effect as BraveSpeech).DamageBuff;
        }
        if (HaveEffect(EffectType.Discipline, out effect))
        {
            buf += (effect as Discipline).DamageBuff(regimentType);
        }
        if (damageType == DamageType.Charge && HaveEffect(EffectType.Charge, out effect))
        {
            buf += (effect as Charge).ChargeBonus;
        }
        if (damageType == DamageType.Range && HaveEffect(EffectType.Accuracy, out effect))
        {
            buf += (effect as Accuracy).RangeBuff;
        }
        return buf;
    }
    public int DamageBuff(RegimentType regimentType, DamageType damageType)
    {
        Effect effect;
        int buf = 0;
        if(damageType == DamageType.Range && HaveEffect(EffectType.FireArrows, out effect))
        {
            buf += (effect as FireArrows).BonusRangeDamage;
        }
        return buf;
    }
    public int ArmorLvlBuff(RegimentType regimentType, DamageType damageType)
    {
        Effect effect;
        int buf = 0;
        if (HaveEffect(EffectType.BraveSpeech, out effect))
        {
            buf += (effect as BraveSpeech).ArmorBuff;
        }
        if (HaveEffect(EffectType.Discipline, out effect))
        {
            buf += (effect as Discipline).ArmorBuff(regimentType);
        }
        if (damageType == DamageType.Range && HaveEffect(EffectType.HeavyArmor, out effect))
        {
            buf += (effect as HeavyArmor).RangeArmor;
        }
        if ((damageType == DamageType.Charge|| damageType == DamageType.Melee) && HaveEffect(EffectType.Paling, out effect))
        {
            buf += (effect as Paling).BonusAntiChargeArmor;
        }
        return buf;
    }
    static string GetDescription(PersonType type)
    {
        switch(type)
        {
            case PersonType.Archer:return "Этот герой хорош при наличии в армии лучников, которым он дает много бонусов.";
            case PersonType.Jaeger:return "Настоящий лесной разбойник, невероятно опасен в лесу, но в другой местности теряет большинство своих преимуществ.";
            case PersonType.Warrior:return "Этот герой славится честью и отвагой, а кроме этого отличным умением командовать пехотой. Под его руководством она становится мощным оружием.";
            case PersonType.Knight:return "Герой создан для нанесения кавалерийских ударов, его вассальные рыцари имеют высокую мобильность и урон.";
            case PersonType.Necromancer:return "Глава нежити выкачивает жизнь из врагов и поднимает мертвецов.";
            case PersonType.DeathKnight:return "Этот рыцарь настоящая машина смерти, которую он несет везде где проносятся его мертвые легионы.";
            default:return "";
        }
    }
    public override string ToString()
    {
        return string.Format("{0} - {1}.\n{4}\nМаксимально отрядов под его руководством {2}, базовая скорость атаки {3} ударов / сек.",
            name, personType.ToString(), MaxRegiment, 1f / AttackSpeed, GetDescription(personType));
    }

}
public class Warrior:Person
{
    public Warrior(State state):base (state)
    {
        personType = PersonType.Warrior;
        Skills = new Skill[] { new BraveSpeechSkill(this), new SiegeSkill(this), new DisciplineSkill(this) };
    }
}
public class Knight : Person
{
    public Knight(State state) : base(state)
    {
        personType = PersonType.Knight;
        Skills = new Skill[] { new ChargeSkill(this), new VassalDebtSkill(this), new HeavyArmorSkill(this) };
    }
}
public class Jaeger : Person
{
    public Jaeger(State state) : base(state)
    {
        personType = PersonType.Jaeger;
        Skills = new Skill[] { new ForestTrailsSkill(this), new SaboteursSkill(this), new ForestBrothersSkill(this) };
    }
}
public class Archer : Person
{
    public Archer(State state) : base(state)
    {
        personType = PersonType.Archer;
        Skills = new Skill[] { new AccuracySkill(this), new FireArrowsSkill(this), new PalingSkill(this) };
    }
}
public class Wizard : Person
{
    public Wizard(State state) : base(state)
    {
        personType = PersonType.Wizard;
        Skills = new Skill[] { new LightingSkill(this), new BootsWalkersSkill(this), new HypnosisSkill(this) };
    }
}
public class Engineer : Person
{
    public Engineer(State state) : base(state)
    {
        personType = PersonType.Engineer;
        Skills = new Skill[] { new TowerSkill(this), new SiegeTeamSkill(this), new EngineeringSkill(this) };
    }
}
public class Necromancer : Person
{
    public Necromancer(State state) : base(state)
    {
        personType = PersonType.Necromancer;
        Skills = new Skill[] { new TheBatsSkill(this), new SkeletonArmySkill(this), new FearSkill(this) };
    }
}
public class DeathKnight : Person
{
    public DeathKnight(State state) : base(state)
    {
        personType = PersonType.DeathKnight;
        Skills = new Skill[] { new DeadMarchSkill(this), new SacrificeSkill(this), new ImmortalySkill(this) };
    }
}
public enum PropertyType
{
    Leadership,
    AttackSpeed,
    MeleeBuff,
    RangeBuff
}

public enum PersonType
{
    Warrior,
    Knight,
    Jaeger,
    Archer,
    Wizard,
    Engineer,
    Necromancer,
    DeathKnight,
    Count 
}

