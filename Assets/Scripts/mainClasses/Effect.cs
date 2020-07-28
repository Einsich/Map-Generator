using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect 
{
    protected int Level;
    public EffectType type;
    public GameAction EffectAction;
    public Effect(int Level)
    {
        this.Level = Level;
    }
    public void LevelUp()
    {
        Level++;
    }
}
public class BraveSpeech:Effect
{
    public static int[] DamBuff = { 0, 1, 2, 4, 6 };
    public static int[] ArmBuff = { 0, 1, 3, 5, 7 };
    public BraveSpeech(int Level):base(Level)
    {
        type = EffectType.BraveSpeech;
    }
    public static int MaxLevel => DamBuff.Length -1;

    public int DamageBuff => DamBuff[Level];
    public int ArmorBuff => ArmBuff[Level];
}
public class Siege : Effect
{
    public Army sieger;
    public Siege(int Level, Army sieger) : base(Level)
    {
        type = EffectType.Siege;
        this.sieger = sieger;
    }

}
public class Discipline : Effect
{
    public static int[] DamBuff = { 0, 1, 2, 3 };
    public static int[] ArmBuff = { 0, 1, 2, 3 };
    public Discipline(int Level) : base(Level)
    {
        type = EffectType.Discipline;
    }
    public static int MaxLevel => DamBuff.Length -1;

    public int DamageBuff(RegimentType type) => type == RegimentType.Infantry ? DamBuff[Level] : 0;
    public int ArmorBuff(RegimentType type) => type == RegimentType.Infantry ? ArmBuff[Level] : 0;
}
public class Charge:Effect
{
    public Charge(int Level):base(Level)
    {
        type = EffectType.Charge;
    }
    public static float[] Speed = new float[] { 1f, 1.8f, 2.45f, 2.99f };
    public static int[] ChargeBuff = new int[] { 0, 3, 6, 9 };
    public static int MaxLevel => Speed.Length - 1;
    public float SpeedBonus => Speed[Level];
    public int ChargeBonus => ChargeBuff[Level];
    public Stun GetStun => new Stun(Level);
}
public class VassalDebt : Effect
{
    public VassalDebt(int Level) : base(Level)
    {
        type = EffectType.VassalDebt;
    }
    public static float[] Cavalry = new float[] { 1, 0.85f, 0.7f, 0.5f };
    public static float[] Infanty = new float[] { 1, 0.9f, 0.8f, 0.7f };
    public static int MaxLevel => Cavalry.Length - 1;
    public float ReductionUpkeep(RegimentType type) { return type == RegimentType.Cavalry ? Cavalry[Level] : type == RegimentType.Infantry ? Infanty[Level] : 1f; }
}
public class HeavyArmor : Effect
{
    public HeavyArmor(int Level) : base(Level)
    {
        type = EffectType.HeavyArmor;
    }
    public static float[] SpeedReduction = new float[] { 1, 0.9f, 0.75f, 0.65f };
    public static int[] RangeArm = new int[] { 0, 2, 4, 6 };
    public static int MaxLevel => SpeedReduction.Length - 1;

    public float ReductionSpeed(RegimentType type) => type == RegimentType.Artillery? 1f:SpeedReduction[Level];
    public int RangeArmor => RangeArm[Level];
}
public class ForestTrails : Effect
{
    public ForestTrails(int Level) : base(Level)
    {
        type = EffectType.ForestTrails;
    }
    public static float[] Speed = new float[] { 1, 1.5f, 2f, 3f };
    public static float[] AttackSpeed = new float[] { 1, 1.2f, 1.5f, 1.8f };
    public static int MaxLevel => Speed.Length - 1;
    public float SpeedBonus => Speed[Level];
    public float AttackSpeedBonus => AttackSpeed[Level];
}
public class Sabotage : Effect
{
    public Sabotage(int Level) : base(Level)
    {
        type = EffectType.Sabotage;
    }


    public static float[] SabotageTime_ = new float[] { 0, 5, 7, 10,15 };
    public static float SabotageTime(int Level) => SabotageTime_[Level];

    public static float[] Speed = new float[] { 1, 0.9f, 0.8f, 0.7f, 0.6f };
    public static int[] Depletions = new int[] { 0, 10, 15, 25, 40 };
    public static int MaxLevel => Speed.Length - 1;
    public float SpeedDebuff => Speed[Level];
    public int Deplention => Depletions[Level];
}
public class ForestBrothers : Effect
{
    public ForestBrothers(int Level) : base(Level)
    {
        type = EffectType.ForestBrothers;
    }
    public static int[] InfGrow = new int[] { 0, 15, 20, 30, 50 };
    public static int MaxLevel => InfGrow.Length - 1;
    public int ManpowerIncrease => InfGrow[Level];
}
public class Accuracy : Effect
{
    public Accuracy(int Level) : base(Level)
    {
        type = EffectType.Accuracy;
    }
    public static int[] Rangers = { 0, 2, 4, 6, 8 };
    public static int MaxLevel => Rangers.Length - 1;
    public int RangeBuff => Rangers[Level];
}
public class FireArrows : Effect
{
    public FireArrows(int Level) : base(Level)
    {
        type = EffectType.FireArrows;
    }
    public static int[] Damage = { 0, 100, 150, 200, 250 };
    public static int MaxLevel => Damage.Length - 1;

    public int BonusRangeDamage => Damage[Level];
}
public class Paling : Effect
{
    public Paling(int Level) : base(Level)
    {
        type = EffectType.Paling;
    }
    public static int[] Armor = { 0, 5, 10, 15, 20 };
    public static int MaxLevel => Armor.Length - 1;
    public int BonusAntiChargeArmor => Armor[Level];
}
public class Lighting : Effect
{
    public Lighting(int Level) : base(Level)
    {
        type = EffectType.Lighting;
    }
}
public class BootsWalkers : Effect
{
    public BootsWalkers(int Level) : base(Level)
    {
        type = EffectType.BootsWalkers;
    }
}
public class Hypnosis : Effect
{
    public Hypnosis(int Level) : base(Level)
    {
        type = EffectType.Hypnosis;
    }
}
public class Tower : Effect
{
    public Tower(int Level) : base(Level)
    {
        type = EffectType.Tower;
    }
}
public class SiegeTeam : Effect
{
    public SiegeTeam(int Level) : base(Level)
    {
        type = EffectType.SiegeTeam;
    }
}
public class Engineering : Effect
{
    public Engineering(int Level) : base(Level)
    {
        type = EffectType.Engineering;
    }
}
public class TheBats : Effect
{
    public TheBats(int Level, Person vampire) : base(Level)
    {
        type = EffectType.TheBats;
        Vampir = vampire;
    }
    public static int[] BaseVampirism = { 0, 20, 40, 60, 100 };
    public static float[] Quality = { 0, 0.4f, 0.7f, 0.9f, 1f };
    public static float[] Radius = { 0, 5, 6, 7, 8 };
    public static float BatsRadius(int lvl) => Radius[lvl];
    public Person Vampir { get; private set; }
    public static int MaxLevel => BaseVampirism.Length - 1;
    public float ArmoredVampirism(int armor) => BaseVampirism[Level] / (0.1f * armor + 1);
    public float VampirismQuality => Quality[Level];
}
public class SkeletonArmy : Effect
{
    public SkeletonArmy(int Level) : base(Level)
    {
        type = EffectType.SkeletonArmy;
    }
    static BaseRegiment[] Skelets =
    {
        null,
        new BaseRegiment(null,RegimentName.Skeletons, RegimentType.Infantry, DamageType.Melee, 2,0,10,0,2,500,default,default,0),
        new BaseRegiment(null,RegimentName.Skeletons, RegimentType.Infantry, DamageType.Melee, 4,2,10,0,5,700,default,default,0),
        new BaseRegiment(null,RegimentName.Skeletons, RegimentType.Infantry, DamageType.Melee, 7,4,15,0,8,900,default,default,0)
    };
    public static int MaxLevel => Skelets.Length - 1;
    public static int[] Count = { 0, 2, 4, 5 };
    public static int SkeletCount(int Level) => Count[Level];
    public static Regiment GetSkelet(int Level) => new Regiment(Skelets[Level]);

}
public class Fear : Effect
{
    public Fear(int Level) : base(Level)
    {
        type = EffectType.Fear;
    }
    public static float[] Stun = { 0, 0.05f, 0.10f, 0.2f, 0.3f };
    public static int MaxLevel => Stun.Length - 1;
    public float StunPropability(DamageType type) => type == DamageType.Melee || type == DamageType.Charge ? Stun[Level] : 0;
}
public class DeadMarch : Effect
{
    public DeadMarch(int Level) : base(Level)
    {
        type = EffectType.DeadMarch;
    }
    public static float[] Speed = { 1f, 1.5f, 2f, 2.5f };
    public static int[] Regen = { 0, 10, 20, 30 };
    public static int MaxLevel => Regen.Length - 1;
    public float SpeedBonus => Speed[Level];
    public int RegenBonus => Regen[Level];
}
public class Immortaly : Effect
{
    public Immortaly(int Level) : base(Level)
    {
        type = EffectType.Immortaly;
    }
    public static float[] Time = { 0, 2, 4, 6, 8 };
    public static int MaxLevel => Time.Length - 1;
    public static float ImmortalTime(int Level) => Time[Level];
}
public class Sacrifice : Effect
{
    public Sacrifice(int Level) : base(Level)
    {
        type = EffectType.Sacrifice;
    }
    public static float[] Multiplue = { 0, 2, 4, 6 };
    public static int MaxLevel => Multiplue.Length - 1;
    public static float SacrificeEffect(int Level) => Multiplue[Level];
}
public class Stun : Effect
{
    public Stun(int Level) : base(Level)
    {
        type = EffectType.Stun;
    }
}
public enum EffectType
{
    BraveSpeech,
    Siege,
    Discipline,
    Charge,//active, to myself and stunned enemy
    VassalDebt,//passive, to myself
    HeavyArmor,//passive, to myself
    ForestTrails,//passive, to myself
    Sabotage,//active, to enemy army
    ForestBrothers,//passive, to myself
    Accuracy,//passive, to myself
    FireArrows,//active, to enemy
    Paling,//active, to myself
    Lighting,//active, to enemy
    BootsWalkers,//active, to myself
    Hypnosis,//active, to enemy stunned
    Tower,//active, to map
    SiegeTeam,//active, to enemy town
    Engineering,//active, to friend town
    TheBats,//passive, to myself
    SkeletonArmy,//active, to myself
    Fear,//active, to enemy army
    DeadMarch,//passive, to myself to near enemy
    Immortaly,//active, to myself
    Sacrifice,//active, to map
    Stun
}