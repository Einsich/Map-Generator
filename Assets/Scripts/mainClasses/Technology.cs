using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void LevelUp(int lvl);
public delegate TimeCost GetCost(int lvl);
public delegate void Research();
public class Technology
{
    public const int maxBuildLvl = 4, maxSpecBuildLvl = 1, maxEra = 4, maxPips = 12,
        maxUpkeepBonus = 3, maxMoralBonus = 3, maxBuildCostBonus = 3, maxTreasureBonus = 4,
        specialBuildCount = ProvinceData.specialCount - ProvinceData.buildCount;
    public State owner;
    public event System.Action TechWasResearchedEvent;
    public void TechWasResearched()=> TechWasResearchedEvent?.Invoke();
    public List<Tech> armyTeches, economyTeches,regimnetTeches, allTeches;

    public Tech[] BuildTech = new Tech[specialBuildCount];
    public int[] BuildLvl = new int[ProvinceData.specialCount];
    static GetCost BuildCost = (lvl) => { return new TimeCost(100, 50); };


    public Tech EraTech;
    public int Era {
        get => Era_;
        set { Era_ = value; for (int i = 0; i < ProvinceData.buildCount; i++)
                BuildLvl[i] = Era;
            Pips += MaxPipsInEra;
        }
    }
    int Era_;
    static GetCost EraCost = (lvl) => { return new TimeCost(15, 100 + lvl * 50); };

    public Tech ArmyBranchTech;
    public bool ArmyBranch = false;
    static GetCost ArmyBranchCost = (lvl)=>{ return new TimeCost(15, 100); };

    public List<BaseRegiment> regiments;
    public int Pips;
    static GetCost PipsCost = (lvl) => { return new TimeCost(2 + lvl * 2, 10 + lvl * 5); };
    public float pipsCostBonus = 1f;
    public int MaxPipsInEra => (1 << Era) * 10 + (ArmyBranch ? 10 * Era : 0);

    public Tech UpkeepBonusTech;
    public float UpkeepBonus = 1f;
    static GetCost UpkeepBonusCost = (lvl) => { return new TimeCost(15, 50 + lvl * 20); };
    static float[] UpkeepBonusData = new float[maxUpkeepBonus] { 0.9f, 0.8f, 0.7f };

    public Tech MoralBonusTech;
    public float MoralBonus = 0f;
    static GetCost MoralBonusCost = (lvl) => { return new TimeCost(15, 50 + lvl * 20); };
    static float[] MoralBonusData = new float[maxMoralBonus] { 0.5f, 1f, 1.25f };

    public Tech EconomyBranchTech;
    public bool EconomyBranch = false;
    static GetCost EconomyBranchCost = (lvl) => { return new TimeCost(15, 100); };

    public Tech BuildCostBonusTech;
    public float BuildCostBonus = 1f;
    static GetCost BuildCostBonusCost = (lvl) => { return new TimeCost(15, 50 + lvl * 20); };
    static float[] BuildCostBonusData = new float[maxBuildCostBonus] { 0.9f, 0.8f, 0.7f };

    public Tech[] TreasureBonusTech;
    public Treasury TreasureBonus = new Treasury(1f);
    static GetCost TreasureBonusCost = (lvl) => { return new TimeCost(15, 50 + lvl * 20); };
    static float[] TreasureBonusData = new float[maxTreasureBonus] { 1.2f, 1.35f, 1.45f, 1.5f };


    public Technology(State state, int era)
    {
        Pips = 0;
        Era = Mathf.Clamp(era, 0, maxEra);
        owner = state;
        regiments = owner.regiments;
        
        EraTech = new Tech(TechType.TechnologyEra,EraCost, maxEra, (lvl) => { Era++; }, this);
        for (int i = 0; i < specialBuildCount; i++)
            BuildTech[i] = new Tech(TechType.Buildings, BuildCost, maxSpecBuildLvl, 
                (lvl) => BuildLvl[ProvinceData.buildCount + i] = 1, this) ;
        ArmyBranchTech = new Tech(TechType.ArmyBranch, ArmyBranchCost, 1, (lvl) => { ArmyBranch = true; pipsCostBonus = 0.5f;
            Pips += Era * 10;  }, this);

        foreach (var regiment in regiments)
        {
           
                regiment.armTeches[0] =
                              new Tech( TechType.MeleeArmor, PipsCost, maxPips - regiment.MeleeArmor,
                              (lvl) => { regiment.MeleeArmor++; }, this)
                              { pips = true };
            regiment.armTeches[1] =
                             new Tech(TechType.ChargeArmor, PipsCost, maxPips - regiment.ChargeArmor,
                             (lvl) => { regiment.ChargeArmor++;  }, this)
                             { pips = true };
            regiment.armTeches[2] =
                              new Tech(TechType.RangeArmor, PipsCost, maxPips - regiment.RangeArmor,
                              (lvl) => { regiment.RangeArmor++;  }, this)
                              { pips = true };
            regiment.armTeches[3] =
                              new Tech(TechType.SiegeArmor, PipsCost, maxPips - regiment.SiegeArmor,
                              (lvl) => { regiment.SiegeArmor++;}, this)
                              { pips = true };

            regiment.damTech = new Tech(TechType.Damage, PipsCost, maxPips,
            (lvl) => { regiment.damageLvl++; }, this)
            { pips = true };
        }
        UpkeepBonusTech = new Tech(TechType.UpkeepBonus, UpkeepBonusCost, maxUpkeepBonus, (lvl) => UpkeepBonus = UpkeepBonusData[lvl], this);
        //MoralBonusTech = new Tech("Увеличение морали", MoralBonusCost, maxMoralBonus, (lvl) => MoralBonus = MoralBonusData[lvl], this);
        BuildCostBonusTech = new Tech(TechType.BuildCostBonus, BuildCostBonusCost, maxBuildCostBonus, (lvl) => BuildCostBonus = BuildCostBonusData[lvl], this);
        EconomyBranchTech = new Tech(TechType.EconomicBranch, EconomyBranchCost, 1, (lvl) => { EconomyBranch = true;  }, this);

        TreasureBonusTech = new Tech[Treasury.ResourceCount];
        for (int i = 0; i < Treasury.ResourceCount; i++)
            TreasureBonusTech[i] = new Tech(Treasury.ToTechType(i), TreasureBonusCost, maxTreasureBonus, (lvl) => TreasureBonus[i] = TreasureBonusData[lvl], this);

        armyTeches = new List<Tech>() { UpkeepBonusTech };
        economyTeches = new List<Tech>() { BuildCostBonusTech };
        for (int i = 0; i < Treasury.ResourceCount; i++)
            economyTeches.Add(TreasureBonusTech[i]);
        regimnetTeches = new List<Tech>();
        foreach(var x in regiments)
        {
            regimnetTeches.Add(x.armTeches[0]);
            regimnetTeches.Add(x.armTeches[1]);
            regimnetTeches.Add(x.armTeches[2]);
            regimnetTeches.Add(x.armTeches[3]);
            regimnetTeches.Add(x.damTech);
        }
        allTeches = new List<Tech> { EraTech, ArmyBranchTech, EconomyBranchTech };
        allTeches.AddRange(armyTeches);
        allTeches.AddRange(economyTeches);
        allTeches.AddRange(regimnetTeches);
    }
    
    public bool TreeCondition(Tech tech)
    {
        if (armyTeches.Contains(tech))
            return ArmyBranch;
        if (economyTeches.Contains(tech))
            return EconomyBranch;
        return true;
    }

}
public struct TimeCost
{
    public float time;
    public float science;
    public TimeCost(float time, float science)
    {
        this.time = time;
        this.science = science;
    }
}
public class Tech
{
    public Research research;
    public Research buttonEvent;
    public ResearchAction researchAction;
    //string descr;
    GetCost cost;
    public int lvl, maxlvl;
    public bool canResearch => researchAction == null && able;
    public bool able => lvl < maxlvl && (pips ? lvl <= (Technology.maxPips / Technology.maxEra) * technology.Era : lvl <= technology.Era);
    public bool ableResearch => lvl < maxlvl;
    public TimeCost timeScience => cost(lvl);
    public float time => cost(lvl).time;
    public float science => cost(lvl).science;
    Technology technology;
    public bool pips;
    TechType type;
    public Tech(TechType type, GetCost cost, int maxlvl, LevelUp action, Technology technology)
    {
        this.cost = cost;
        this.maxlvl = maxlvl;
        this.type = type;
        lvl = 0;
        research += () => { action(lvl++); researchAction = null; buttonEvent?.Invoke(); technology.TechWasResearched(); };
        this.technology = technology;
    }
    public override string ToString()
    {
        string s = "";
        switch (type)
        {
            case TechType.ArmyBranch:s = "Военные технологии";break;
            case TechType.EconomicBranch: s = "Экономические технологии"; break;
            case TechType.TechnologyEra: s = "Технологическая эра"; break;
            case TechType.UpkeepBonus: s = "Уменьшение содержания"; break;
            case TechType.BuildCostBonus: s = "Удешевление строительства"; break;
            case TechType.GoldBonus: s = "Золото"; break;
            case TechType.ManPowerBonus: s = "Рекруты"; break;
            case TechType.WoodBonus: s = "Древесина"; break;
            case TechType.IronBonus: s = "Железо"; break;
            case TechType.ScienceBonus: s = "Наука"; break;
            case TechType.MeleeArmor:s = "";break;
            case TechType.ChargeArmor:s = "";break;
            case TechType.RangeArmor:s = "";break;
            case TechType.SiegeArmor:s = "";break;
            case TechType.Damage:s = "";break;
            case TechType.Buildings:s = "Здания";break;
        }
        return string.Format("{0} ({1} {2}ур.)", s, lvl, ableResearch ? "" : "*");
    }
    public string GetDescribe()
    {
        switch(type)
        {
            case TechType.ArmyBranch: return "Позволяет исследовать технологии, которые сделают вашу армию сильнее."; 
            case TechType.EconomicBranch: return "Позволяет исследовать технологии, которые дадут вашей державе экономическое преимущество";
            case TechType.TechnologyEra: return "Технологическая эра поднимает максимальный уровень ваших технологий, а также дает некоторое количество очков улучшения отрядов.";
            case TechType.UpkeepBonus: return "Уменьшает содержание армии.";
            case TechType.BuildCostBonus: return "Уменьшает стоимость постройки зданий.";
            case TechType.GoldBonus: return "Увеличивает поступления золота в казну.";
            case TechType.ManPowerBonus: return "Увеличивает прирост рекрутов.";
            case TechType.WoodBonus: return "Увеличивает дереводобычу.";
            case TechType.IronBonus: return "Увеличивает добычу железа.";
            case TechType.ScienceBonus: return "Увеличивает выработку очков науки";
            case TechType.MeleeArmor: return "Повышает защиту от ближних атак всех ваших  подразделений этого типа.";
            case TechType.ChargeArmor: return "Повышает защиту от атак кавалерии всех ваших  подразделений этого типа.";
            case TechType.RangeArmor: return "Повышает защиту от стрелковых атак всех ваших  подразделений этого типа.";
            case TechType.SiegeArmor: return "Повышает защиту от осадных орудий всех ваших  подразделений этого типа.";
            case TechType.Damage: return "Повышает уровень урона всех ваших  подразделений этого типа.";
            case TechType.Buildings: return "Улучшает здание";
            default: return "";
        }
    }
}
public enum TechType
{
    TechnologyEra,
    ArmyBranch,
    EconomicBranch,
    UpkeepBonus,
    BuildCostBonus,
    GoldBonus,
    ManPowerBonus,
    WoodBonus,
    IronBonus,
    ScienceBonus,
    MeleeArmor,
    ChargeArmor,
    RangeArmor,
    SiegeArmor,
    Damage,
    Buildings
}