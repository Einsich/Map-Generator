using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill 
{
    public bool Active, Castable;
    protected bool Use;
    public float ActionTime, CullDown;
    public SkillType type;
    protected int Level = 0, MaxLevel;
    public Person Person;
    public bool CanUse => Active && !Use;
    public int GetLevel => Level;
    public int GetMaxLevel => MaxLevel;
    virtual public void LevelUp()
    {
        Level += Level == MaxLevel ? 0 : 1;
        PassiveEffect?.LevelUp();
    }
    public Action SkillAction;
    public EffectType EffectType;
    public Effect PassiveEffect;
    public System.Action ButtonUpdate;
    public Skill(Person person)
    {
        Person = person;
    }
    virtual public void Cast()
    { }
    virtual public void UseSkill(Person who)
    {
        if (Use)
            return;
        Use = true;
        Effect effect = GetEffect();
        if (Active)
        {
            effect.EffectAction = new Action(ActionTime, () => who.StopUseSkillOnHim(effect));
            SkillAction = new Action(CullDown, () => StopUse());
        }
        who.UseSkillOnHim(effect);
    }
    virtual public void StopUse()
    {
        if (!Use)
            return;
        Use = false;
        SkillAction = null;
        Person.SkillCulldowned?.Invoke();
    }
    virtual public Effect GetEffect() { return null; }
    public static string FormatArray(int[] a)
    {
        string s="";
        for (int i = 1; i < a.Length; i++)
            s += a[i].ToString() + (i == a.Length - 1 ? "" : " / ");
        return s;
    }
    public static string FormatArray(float[] a, bool percent = false)
    {
        string s = "";
        for (int i = 1; i < a.Length; i++)
            s += (!percent?a[i].ToString("N2"):((a[i]*100).ToString("N0") + " %")) + (i == a.Length - 1 ? "" : " / ");
        return s;
    }
}
public class BraveSpeechSkill:Skill
{
    public BraveSpeechSkill(Person person) :base(person)
    {
        type = SkillType.BraveSpeech;
        EffectType = EffectType.BraveSpeech;
        Active = true;
        ActionTime = 2f;
        CullDown = 5f;
        MaxLevel = BraveSpeech.MaxLevel;
    }
    public override void Cast()
    {
        UseSkill(Person);
    }
    public override Effect GetEffect()
    {
        BraveSpeech speech = new BraveSpeech(Level);
        return speech;
    }
    public override string ToString()
    {
        return string.Format("Воодущевляющая речь.\nПовышает уровень защиты от всех атак на {0}.\n А так же уровень атаки на {1}.\nДействует {2} сек, перезарядка {3} сек.",
            FormatArray(BraveSpeech.ArmBuff), FormatArray(BraveSpeech.DamBuff), ActionTime, CullDown);
    }
}
public class SiegeSkill : Skill,IRegionCastable
{

    public SiegeSkill(Person person) : base(person)
    {
        type = SkillType.Siege;
        EffectType = EffectType.Siege;
        Active = true;
        Castable = true;
        ActionTime = 2f;
        CullDown = 5f;
        MaxLevel = SiegeTime_.Length - 1;
    }
    static float[] SiegeTime_ = { 1000, 5, 4, 3, 2 };
    public float SiegeTime => SiegeTime_[Level];
    public float SiegeRange => Level * 0.4f + 1.5f;
    public override void Cast()
    {
        Player.StartCastSkill(this);
    }
    public bool CanCastOnRegion(Region region)
    {
        Diplomacy Owner = Person.owner.diplomacy;
        return region!=null && region.curOwner!=null&& region.curOwner.diplomacy!=null && Owner.haveWar(region.curOwner.diplomacy);
    }

    public bool CastOnRegion(Region region)
    {
        if(!CanUse || (Person.curArmy.position - region.position).sqrMagnitude >SiegeRange* SiegeRange)
        {
            return false;
        }
        Use = true;
        Siege effect = new Siege(Level, Person.curArmy);
        effect.EffectAction = new Action(SiegeTime, () => region.WinSiege());
        SkillAction = new Action(CullDown, () => StopUse());
        region.SiegeBegin(effect);
        return true;

    }
    public override string ToString()
    {
        return string.Format("Осада.\nПозволяет осадить вражеский город, на время {0} сек, после которого, если осада не была снята, город захватывается. " +
            "Если в осажденном городе находится герой, то он не может покинуть город, а при захвате погибает.\n Перезарядка {1} сек.",
            FormatArray(SiegeTime_), CullDown);
    }

}
public class DisciplineSkill : Skill
{
    public DisciplineSkill(Person person) : base(person)
    {
        type = SkillType.Discipline;
        EffectType = EffectType.Discipline;
        MaxLevel = Discipline.MaxLevel;
    }
    public override Effect GetEffect()
    {
        PassiveEffect = new Discipline(Level);
        return PassiveEffect;
    }
    public override string ToString()
    {
        return string.Format("Дисциплина.\nДействует тольок на пехоту: повышает уровень защиты от всех атак на {0}.\n А так же уровень атаки на {1}.\n Пассивная способность.",
            FormatArray(Discipline.ArmBuff), FormatArray(Discipline.DamBuff), ActionTime, CullDown);
    }
}
public class ChargeSkill:Skill
{
    public ChargeSkill(Person person):base(person)
    {
        type = SkillType.Charge;
        EffectType = EffectType.Charge;
        Active = true;
        ActionTime = 2f;
        CullDown = 5f;
        MaxLevel = Charge.MaxLevel;
    }
    public override Effect GetEffect()
    {
        return new Charge(Level);
    }
    public override void Cast()
    {
        Use = true;
        Charge effect = new Charge(Level);
        effect.EffectAction = new Action(ActionTime, () => Person.StopUseSkillOnHim(effect));
        effect.EffectAction.onAction += () => Person.curArmy.UpdateSpeed();
        SkillAction = new Action(CullDown, () => StopUse());        
        Person.UseSkillOnHim(effect);
        Person.curArmy.UpdateSpeed();
    }
    public override string ToString()
    {
        return string.Format("Таранный удар.\nУвеличивает скорость передвижения армии и урон от наскока кавалерии, при атаке оглушает противника.  Ускорение {0}.\n Улучшение уровня наскока {1}.\n" +
            " Время действия {2} сек., перезарядка {3} сек.\n Активная способность.",
            FormatArray(Charge.Speed,true), FormatArray(Charge.ChargeBuff), ActionTime, CullDown);
    }
}
public class VassalDebtSkill : Skill
{
    public VassalDebtSkill(Person person) : base(person)
    {
        type = SkillType.VassalDebt;
        EffectType = EffectType.VassalDebt;
        MaxLevel = VassalDebt.MaxLevel;
    }
    public override Effect GetEffect()
    {
        PassiveEffect = new VassalDebt(Level);
        return PassiveEffect;
    }
    public override string ToString()
    {
        return string.Format("Вассальная клятва.\n Уменьшает содержание пехоты и, особенно, кавалерии.  Удешевление содержания пехоты {0}.\n Удешевление содержания кавалерии {1}.\n" +
            " Пассивная способность.",
            FormatArray(VassalDebt.Infanty, true), FormatArray(VassalDebt.Cavalry, true));
    }
}
public class HeavyArmorSkill : Skill
{
    public HeavyArmorSkill(Person person) : base(person)
    {
        type = SkillType.HeavyArmor;
        EffectType = EffectType.HeavyArmor;
        MaxLevel = HeavyArmor.MaxLevel;
    }
    public override Effect GetEffect()
    {
        PassiveEffect = new HeavyArmor(Level);
        return PassiveEffect;
    }
    public override void LevelUp()
    {
        base.LevelUp();
        Person.curArmy.UpdateSpeed();
    }
    public override string ToString()
    {
        return string.Format("Тяжелая броня.\n Увеличивает уровень защиты всей армии от стрелковых атак, но снижает скорость передвижения(не ниже скорости осадных отрядов).\n" +
            "Увеличение защиты {0}.\n Замедление {1}.\n" +
            " Пассивная способность.",
            FormatArray(HeavyArmor.RangeArm), FormatArray(HeavyArmor.SpeedReduction, true));
    }
}
public class ForestTrailsSkill : Skill
{
    public ForestTrailsSkill(Person person) : base(person)
    {
        type = SkillType.ForestTrails;
        EffectType = EffectType.ForestTrails;
        MaxLevel = ForestTrails.MaxLevel;
    }
    public override Effect GetEffect()
    {
        PassiveEffect = new ForestTrails(Level);
        return PassiveEffect;
    }
    public override void LevelUp()
    {
        base.LevelUp();
        Person.curArmy.UpdateSpeed();
        Person.curArmy.UpdateAttackSpeed();
    }

    public override string ToString()
    {
        return string.Format("Лесные тропы.\n Увеличивает Скорость передвижения и скорость атаки героя, если тот находится в лесу.\n" +
            "Увеличение скорости передвижения {0}.\n Увеличения скорости атаки {1}.\n" +
            " Пассивная способность.",
            FormatArray(ForestTrails.Speed, true), FormatArray(ForestTrails.AttackSpeed, true));
    }
}
public class SaboteursSkill : Skill, IArmyCastable
{
    public SaboteursSkill(Person person) : base(person)
    {
        type = SkillType.Saboteurs;
        EffectType = EffectType.Sabotage;
        CullDown = 5f;
        Active = true;
        Castable = true;

        MaxLevel = Sabotage.MaxLevel;
    }
    static float CastRange = DamageInfo.AttackRange(DamageType.Range);
    public bool CanCastOnArmy(Army army)
    {
        return Person.owner.diplomacy.haveWar(army.curOwner.diplomacy);

    }
    public override void LevelUp()
    {
        base.LevelUp();
        ActionTime = Sabotage.SabotageTime(Level);
    }
    public bool CastOnArmy(Army army)
    {
        if (!CanUse || (Person.curArmy.position - army.position).sqrMagnitude > CastRange * CastRange)
        {
            return false;
        }
        UseSkill(army.Person);
        army.UpdateSpeed();
        SkillAction.onAction += () => army.UpdateSpeed();
        return true;
    }
    public override void Cast()
    {
        Player.StartCastSkill(this);
    }
    public override Effect GetEffect()
    {
        return new Sabotage(Level);
    }
    public override string ToString()
    {
        return string.Format("Сабботаж.\n Замедляет и наносит периодический урон армии противника.\n" +
            "Замедление {0}.\nПериодический урон {1}.\nВремя действия {2} сек\n, перезарядка {3} сек.\n" +
            "Активная способность.",
            FormatArray(Sabotage.Speed, true), FormatArray(Sabotage.Depletions), FormatArray(Sabotage.SabotageTime_), CullDown);
    }
}
public class ForestBrothersSkill : Skill
{
    public ForestBrothersSkill(Person person) : base(person)
    {
        type = SkillType.ForestBrothers;
        EffectType = EffectType.ForestBrothers;
        MaxLevel = ForestBrothers.MaxLevel;
    }
    public override Effect GetEffect()
    {
        PassiveEffect = new ForestBrothers(Level);
        return PassiveEffect;
    }
        public override string ToString()
        {
            return string.Format("Лесное братство.\n Восстанавливает пехотные подразделения героя (каждый отряд), если он находится в лесу.\n" +
                "Восстановление {0}.\n" +
                " Пассивная способность.",
                FormatArray(ForestBrothers.InfGrow));
        }
    }
public class AccuracySkill : Skill
{
    public AccuracySkill(Person person) : base(person)
    {
        type = SkillType.Accuracy;
        EffectType = EffectType.Accuracy;
        MaxLevel = Accuracy.MaxLevel;
    }
    public override Effect GetEffect()
    {
        PassiveEffect = new Accuracy(Level);
        return PassiveEffect;
    }
    public override string ToString()
    {
        return string.Format("Меткость.\n Увеличивает уровень урона для всех стрелковых подразделений героя.\n" +
            "Увеличение дистанционного боя {0}.\n" +
            " Пассивная способность.",
            FormatArray(Accuracy.Rangers));
    }
}
public class FireArrowsSkill : Skill
{
    public FireArrowsSkill(Person person) : base(person)
    {
        type = SkillType.FireArrows;
        EffectType = EffectType.FireArrows;
        Active = true;
        ActionTime = 2f;
        CullDown = 4f;
        MaxLevel = FireArrows.MaxLevel;
    }
    public override void Cast()
    {
        UseSkill(Person);
    }
    public override Effect GetEffect()
    {
        return new FireArrows(Level);
    }
    public override string ToString()
    {
        return string.Format("Огненные стрелы.\n Добавляет урон к урону стрелковых отрядов.\n" +
            "Добавочный урон {0}.\n" +
            "Активная способность.",
            FormatArray(FireArrows.Damage));
    }
}
public class PalingSkill : Skill
{
    public PalingSkill(Person person) : base(person)
    {
        type = SkillType.Paling;
        EffectType = EffectType.Paling;
        Active = true;
        ActionTime = 10f;
        CullDown = 15f;
        MaxLevel = Paling.MaxLevel;
    }
    public override void Cast()
    {
        UseSkill(Person);
        Person.curArmy.navAgent.Stop();
    }
    public override Effect GetEffect()
    {
        return new Paling(Level);
    }
    public override string ToString()
    {
        return string.Format("Защитный частокол.\n Применение способности останавливает героя и дает отрядам дополнительный уровень защиты в ближнем бою, прерывается если герой будет передвигаться.\n" +
            "Броня {0}.\n Время действия {1} сек, перезарядка {2} сек.\n" +
            "Активная способность.",
            FormatArray(Paling.Armor), ActionTime, CullDown);
    }
}
public class LightingSkill : Skill
{
    public LightingSkill(Person person) : base(person)
    {
        type = SkillType.Lighting;
        EffectType = EffectType.Lighting;
    }
    public override Effect GetEffect()
    {
        return new Lighting(Level);
    }
}
public class BootsWalkersSkill : Skill
{
    public BootsWalkersSkill(Person person) : base(person)
    {
        type = SkillType.BootsWalkers;
        EffectType = EffectType.BootsWalkers;
    }
    public override Effect GetEffect()
    {
        return new BootsWalkers(Level);
    }
}
public class HypnosisSkill : Skill
{
    public HypnosisSkill(Person person) : base(person)
    {
        type = SkillType.Hypnosis;
        EffectType = EffectType.Hypnosis;
    }
    public override Effect GetEffect()
    {
        return new Hypnosis(Level);
    }
}
public class TowerSkill : Skill
{
    public TowerSkill(Person person) : base(person)
    {
        type = SkillType.Tower;
        EffectType = EffectType.Tower;
    }
    public override Effect GetEffect()
    {
        return new Tower(Level);
    }
}
public class SiegeTeamSkill : Skill
{
    public SiegeTeamSkill(Person person) : base(person)
    {
        type = SkillType.SiegeTeam;
        EffectType = EffectType.SiegeTeam;
    }
    public override Effect GetEffect()
    {
        return new SiegeTeam(Level);
    }
}
public class EngineeringSkill : Skill
{
    public EngineeringSkill(Person person) : base(person)
    {
        type = SkillType.Engineering;
        EffectType = EffectType.Engineering;
    }
    public override Effect GetEffect()
    {
        return new Engineering(Level);
    }
}
public class TheBatsSkill : Skill,IArmyCastable
{
    public TheBatsSkill(Person person) : base(person)
    {
        type = SkillType.TheBats;
        EffectType = EffectType.TheBats;
        Active = true;
        Castable = true;
        ActionTime = 5;
        CullDown = 7;
        MaxLevel = TheBats.MaxLevel;
    }
    public override void Cast()
    {
        Player.StartCastSkill(this);
    }
    public bool CanCastOnArmy(Army army)
    {
       return Person.owner.diplomacy.haveWar(army.curOwner.diplomacy);
    }

    public bool CastOnArmy(Army army)
    {
        if (!CanUse || (army.position - Person.curArmy.position).magnitude > TheBats.BatsRadius(Level))
            return false;

        Use = true;
        TheBats effect = new TheBats(Level, Person);
        Person who = army.Person;
        effect.EffectAction = new Action(ActionTime, () => who.StopUseSkillOnHim(effect));
        SkillAction = new Action(CullDown, () => StopUse());
        
        who.UseSkillOnHim(effect);
        return true;
    }

    public override Effect GetEffect()
    {
        return new TheBats(Level, Person);
    }
    public override string ToString()
    {
        return string.Format("Летучие мыши.\n Высасывает жизнь из армии врага (каждого отряда), передавая эту жизненную силу с некоторым коэффициентом герою." +
            " Чем больше броня вражеского отряда, тем меньше будет похищено у него жизненной силы.\n" +
            "Базовый урон {0}.\nКоэффициент вампиризма {1}\n. Радиус применения способности {2}.\nВремя действия {1} сек, перезарядка {2} сек.\n" +
            "Активная способность.",
            FormatArray(TheBats.BaseVampirism), FormatArray(TheBats.Quality, true), FormatArray(TheBats.Radius), ActionTime, CullDown);
    }
}
public class SkeletonArmySkill : Skill
{
    public int KilledRegiment = 0;
    public SkeletonArmySkill(Person person) : base(person)
    {
        type = SkillType.SkeletonArmy;
        EffectType = EffectType.SkeletonArmy;
        Active = true;
        ActionTime = 10;
        CullDown = 10;
        MaxLevel = SkeletonArmy.MaxLevel;
    }
    public override void Cast()
    {
        int count = SkeletonArmy.SkeletCount(Level);
        if(count <= KilledRegiment)
        {
            KilledRegiment -= count;
        } else
        {
            count = KilledRegiment;
            KilledRegiment = 0;
        }
        if (count == 0)
            return;
        Regiment[] skeleton = new Regiment[count];
        for (int i = 0; i < count; i++)
        {
            skeleton[i] = SkeletonArmy.GetSkelet(Level);
            Person.curArmy.army.Add(skeleton[i]);
        }
        Person.curArmy.ArmyListChange();
        Use = true;
        Army cur = Person.curArmy;

             new Action(ActionTime, () => {
                 for (int i = 0; i < count; i++)
                 {
                     cur.army.Remove(skeleton[i]);
                 }
                 cur.ArmyListChange();
             });
            SkillAction = new Action(CullDown, () => StopUse());
        
    }
    public override Effect GetEffect()
    {
        return new SkeletonArmy(Level);
    }
    public override string ToString()
    {
        return string.Format("Армия скелетов.\n Призывает несколько отрядов скелетов, на каждый отряд скелетов нужен один заряд некромантии." +
            " Эти заряды герой получает при уничтожении отрядов в армии вражеского героя, по одному заряду за отряд\n" +
            "Количество отрядов {0}.\n Время действия {1} сек, перезарядка {2} сек.\n" +
            "Активная способность.",
            FormatArray(SkeletonArmy.Count), ActionTime, CullDown);
    }
}
public class FearSkill : Skill
{
    public FearSkill(Person person) : base(person)
    {
        type = SkillType.Fear;
        EffectType = EffectType.Fear;
        MaxLevel = Fear.MaxLevel;
    }
    public override Effect GetEffect()
    {
        PassiveEffect =  new Fear(Level);
        return PassiveEffect;
    }
    public override string ToString()
    {
        return string.Format("Ужас.\nВид армии нежити настолько отвратителен, что вражеский герой может оцепенеть из-за атаки героя.\n" +
            "Шанс оцепенения {0}.\n" +
            "Пассивная способность.",
            FormatArray(Fear.Stun, true));
    }
}
public class DeadMarchSkill : Skill
{
    public DeadMarchSkill(Person person) : base(person)
    {
        type = SkillType.DeadMarch;
        EffectType = EffectType.DeadMarch;
        MaxLevel = DeadMarch.MaxLevel;
    }
    public override Effect GetEffect()
    {
        PassiveEffect = new DeadMarch(Level);
        return PassiveEffect;
    }
    public override void LevelUp()
    {
        base.LevelUp();
        Person.curArmy.UpdateSpeed();
    }
    public override string ToString()
    {
        return string.Format("Марш мертвых.\nАура рыцаря смерти заставляет армию мертвых быстрее перемещаться и восстанавливать потери в любом месте.\n" +
            "Ускорение передвижения {0}.\n" +
            "Пассивное востановление {1}.\n" +
            "Пассивная способность.",
            FormatArray(DeadMarch.Speed, true), FormatArray(DeadMarch.Regen));
    }
}
public class ImmortalySkill : Skill
{
    public ImmortalySkill(Person person) : base(person)
    {
        type = SkillType.Immortaly;
        EffectType = EffectType.Immortaly;
        Active = true;
        ActionTime = 0;
        CullDown = 10;
        MaxLevel = Immortaly.MaxLevel;
    }
    public override void LevelUp()
    {
        base.LevelUp();
        ActionTime = Immortaly.ImmortalTime(Level);
    }
    public override void Cast()
    {
        UseSkill(Person);
    }
    public override Effect GetEffect()
    {
        return new Immortaly(Level);
    }
    public override string ToString()
    {
        return string.Format("Бессмертие.\nРыцарь смерти делает свою армию невосприимчивой к урону.\n" +
            "Время действия бессмертия {0} сек.\n" +
            "Перезарядка {1} сек.\n" +
            "Активная способность.",
            FormatArray(Immortaly.Time), CullDown);
    }
}
public class SacrificeSkill : Skill
{
    public SacrificeSkill(Person person) : base(person)
    {
        type = SkillType.Sacrifice;
        EffectType = EffectType.Sacrifice;
        Active = true;
        ActionTime = 0;
        CullDown = 5;
        MaxLevel = Sacrifice.MaxLevel;
    }
    public override void Cast()
    {

        Use = true;
        var list = Person.curArmy.army;
        float heal = 0;
        Regiment sacrifice = null;
        for(int i=0;i<list.Count;i++)
            if(list[i].count > heal)
            {
                heal = list[i].count;
                sacrifice = list[i];
            }

        list.Remove(sacrifice);
        Person.curArmy.Heal(heal* Sacrifice.SacrificeEffect(Level));
        SkillAction = new Action(CullDown, () => StopUse());
    }
    public override Effect GetEffect()
    {
        return new Sacrifice(Level);
    }
    public override string ToString()
    {
        return string.Format("Жертвоприношение.\nГерой приносит в жертву отряд с самым большим запасом здоровья, излечивая другие отряды своей армии на полученную жизненную силу с некоторым множителем.\n" +
            "Множитель {0} %.\n" +
            "Перезарядка {1} сек.\n" +
            "Активная способность.",
            FormatArray(Sacrifice.Multiplue, true), CullDown);
    }
}

public enum SkillType
{
    BraveSpeech,//active, to myself+
    Siege,//active, to enemy town+
    Discipline,//passive, to myself+
    Charge,//active, to myself and stunned enemy
    VassalDebt,//passive, to myself+-
    HeavyArmor,//passive, to myself+-
    ForestTrails,//passive, to myself+-
    Saboteurs,//active, to enemy army
    ForestBrothers,//passive, to myself+-
    Accuracy,//passive, to myself+-
    FireArrows,//active, to enemy
    Paling,//active, to myself+-
    Lighting,//active, to enemy
    BootsWalkers,//active, to myself+-
    Hypnosis,//active, to enemy stunned
    Tower,//active, to map
    SiegeTeam,//active, to enemy town
    Engineering,//active, to friend town
    TheBats,//passive, to myself+-
    SkeletonArmy,//active, to myself+-
    Fear,//active, to enemy army
    DeadMarch,//passive, to myself to near enemy
    Immortaly,//active, to myself+-
    Sacrifice//active, to map
}
public interface IMapCastable
{
    bool CanCastOnMap(Vector2Int pos);
    bool CastOnMap(Vector2Int pos);
}
public interface IRegionCastable
{
    bool CanCastOnRegion(Region region);
    bool CastOnRegion(Region region);
}
public interface IArmyCastable
{
    bool CanCastOnArmy(Army army);
    bool CastOnArmy(Army army);
}
