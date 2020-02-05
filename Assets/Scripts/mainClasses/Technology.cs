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

    public List<Tech> armyTeches, economyTeches;

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
    static GetCost EraCost = (lvl) => { return new TimeCost(100, 100 + lvl * 50); };

    public Tech ArmyBranchTech;
    public bool ArmyBranch = false;
    static GetCost ArmyBranchCost = (lvl)=>{ return new TimeCost(100, 100); };

    public List<BaseRegiment> regiments;
    public int Pips;
    static GetCost PipsCost = (lvl) => { return new TimeCost(4 + lvl * 10, 10 + lvl * 5); };
    public float pipsCostBonus = 1f;
    public int MaxPipsInEra => (1 << Era) * 10 + (ArmyBranch ? 10 * Era : 0);

    public Tech UpkeepBonusTech;
    public float UpkeepBonus = 1f;
    static GetCost UpkeepBonusCost = (lvl) => { return new TimeCost(100, 50 + lvl * 20); };
    static float[] UpkeepBonusData = new float[maxUpkeepBonus] { 0.9f, 0.8f, 0.7f };

    public Tech MoralBonusTech;
    public float MoralBonus = 0f;
    static GetCost MoralBonusCost = (lvl) => { return new TimeCost(100, 50 + lvl * 20); };
    static float[] MoralBonusData = new float[maxMoralBonus] { 0.5f, 1f, 1.25f };

    public Tech EconomyBranchTech;
    public bool EconomyBranch = false;
    static GetCost EconomyBranchCost = (lvl) => { return new TimeCost(100, 100); };

    public Tech BuildCostBonusTech;
    public float BuildCostBonus = 1f;
    static GetCost BuildCostBonusCost = (lvl) => { return new TimeCost(100, 50 + lvl * 20); };
    static float[] BuildCostBonusData = new float[maxBuildCostBonus] { 0.9f, 0.8f, 0.7f };

    public Tech[] TreasureBonusTech;
    public Treasury TreasureBonus = new Treasury(1f);
    static GetCost TreasureBonusCost = (lvl) => { return new TimeCost(100, 50 + lvl * 20); };
    static float[] TreasureBonusData = new float[maxTreasureBonus] { 1.2f, 1.35f, 1.45f, 1.5f };


    public Technology(State state, int era)
    {
        Pips = 0;
        Era = Mathf.Clamp(era, 0, maxEra);
        owner = state;
        regiments = owner.regiments;
        
        EraTech = new Tech("Технологическая эра",EraCost, maxEra, (lvl) => { Era++;MenuManager.ShowTechnology(); }, this);
        for (int i = 0; i < specialBuildCount; i++)
            BuildTech[i] = new Tech(((Building)(ProvinceData.buildCount + i)).ToString(), BuildCost, maxSpecBuildLvl, 
                (lvl) => BuildLvl[ProvinceData.buildCount + i] = 1, this) ;
        ArmyBranchTech = new Tech("Военная ветка", ArmyBranchCost, 1, (lvl) => { ArmyBranch = true; pipsCostBonus = 0.5f;
            Pips += Era * 10; MenuManager.ShowTechnology(); }, this);
        
        /*for (int k = 0; k < regiments.Count; k++)
            regiments[k].teches = new Tech[BaseRegiment.NM];
        for (int k = 0; k < regiments.Count; k++)
            for (int i = 0; i < BaseRegiment.PipsN; i++)
                for (int j = 0; j < BaseRegiment.PipsM; j++)
                {
                    int q = k, w = i, e = j ;
                    regiments[k].teches[i * BaseRegiment.PipsM + j] =
                              new Tech("", PipsCost, maxPips - regiments[k].pips[i, j], 
                              (lvl) => { regiments[q].pips[w, e]++; MenuManager.ShowTechnology(); }, this) { pips = true };

                }
                */
        UpkeepBonusTech = new Tech("Снижение содержания", UpkeepBonusCost, maxUpkeepBonus, (lvl) => UpkeepBonus = UpkeepBonusData[lvl], this);
        MoralBonusTech = new Tech("Увеличение морали", MoralBonusCost, maxMoralBonus, (lvl) => MoralBonus = MoralBonusData[lvl], this);
        BuildCostBonusTech = new Tech("Удешевление зданий", BuildCostBonusCost, maxBuildCostBonus, (lvl) => BuildCostBonus = BuildCostBonusData[lvl], this);
        EconomyBranchTech = new Tech("Экономическая ветка", EconomyBranchCost, 1, (lvl) => { EconomyBranch = true; MenuManager.ShowTechnology(); }, this);

        TreasureBonusTech = new Tech[Treasury.ResourceCount];
        for (int i = 0; i < Treasury.ResourceCount; i++)
            TreasureBonusTech[i] = new Tech(Treasury.ToString(i), TreasureBonusCost, maxTreasureBonus, (lvl) => TreasureBonus[i] = TreasureBonusData[lvl], this);

        armyTeches = new List<Tech>() { UpkeepBonusTech, MoralBonusTech };
        economyTeches = new List<Tech>() { BuildCostBonusTech };
        for (int i = 0; i < Treasury.ResourceCount; i++)
            economyTeches.Add(TreasureBonusTech[i]);


    }



}
public struct TimeCost
{
    public int time;
    public float science;
    public TimeCost(int time, float science)
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
    string descr;
    GetCost cost;
    public int lvl, maxlvl;
    public bool able => lvl < maxlvl && (pips ? lvl <= (Technology.maxPips / Technology.maxEra) * technology.Era : lvl <= technology.Era);
    public bool ableResearch => lvl < maxlvl;
    public TimeCost timeScience => cost(lvl);
    public int time => cost(lvl).time;
    public float science => cost(lvl).science;
    Technology technology;
    public bool pips;

    public Tech(string descr, GetCost cost, int maxlvl, LevelUp action, Technology technology)
    {
        this.cost = cost;
        this.maxlvl = maxlvl;
        this.descr = descr;
        lvl = 0;
        research += () => { action(lvl++); researchAction = null; buttonEvent?.Invoke(); };
        this.technology = technology;
    }
    public override string ToString()
    {
        return string.Format("{0} ({1} {2}ур.)", descr, lvl, ableResearch ? "" : "*");
    }
}
