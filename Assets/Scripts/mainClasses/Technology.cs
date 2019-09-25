using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Technology
{
    public const int maxBuildLvl = 4, maxSpecBuildLvl = 1, maxEra = 4, maxPips = 12,
        maxUpkeepBonus = 3, maxMoralBonus = 3, maxBuildCostBonus = 3, maxTreasureBonus = 4,
        specialBuildCount = ProvinceData.specialCount - ProvinceData.buildCount;
    public State owner;
    public delegate TimeCost GetCost(int lvl);

    public Tech[] BuildTech = new Tech[specialBuildCount];
    public int[] BuildLvl = new int[ProvinceData.specialCount];
    static GetCost BuildCost = (lvl) => { return new TimeCost(100, 50); };


    public Tech EraTech;
    public int Era {
        get => Era_;
        set { Era_ = value; for (int i = 0; i < ProvinceData.buildCount; i++)
                BuildLvl[i] = Era;
        }
    }
    int Era_;
    static GetCost EraCost = (lvl) => { return new TimeCost(100, 100 + lvl * 50); };

    public Tech ArmyBranchTech;
    public bool ArmyBranch = false;
    static GetCost ArmyBranchCost = (lvl)=>{ return new TimeCost(100, 100); };

    public Tech[,] PipsTech;
    public List<BaseRegiment> regiments;
    public int Pips;
    static GetCost PipsCost = (lvl) => { return new TimeCost(50 + lvl * 10, 10 + lvl * 5); };
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
        Era = Mathf.Clamp(era, 1, maxEra);
        owner = state;
        regiments = owner.regiments;
        Pips = MaxPipsInEra;
        
        EraTech = new Tech(EraCost, maxEra, (lvl) => Era++);
        for (int i = 0; i < specialBuildCount; i++)
            BuildTech[i] = new Tech(BuildCost, maxSpecBuildLvl, (lvl) => BuildLvl[ProvinceData.buildCount + i] = 1);
        ArmyBranchTech = new Tech(ArmyBranchCost, 1, (lvl) => { ArmyBranch = true; pipsCostBonus = 0.5f; });
        
        PipsTech = new Tech[regiments.Count, BaseRegiment.PipsM * BaseRegiment.PipsN];
        for (int k = 0; k < regiments.Count; k++)
            for (int i = 0; i < BaseRegiment.PipsN; i++)
                for (int j = 0; j < BaseRegiment.PipsM; j++)
                    PipsTech[k, i * BaseRegiment.PipsM + j] =
                            new Tech(PipsCost, maxPips - regiments[k].pips[i, j], (lvl) => regiments[k].pips[i, j]++);
        UpkeepBonusTech = new Tech(UpkeepBonusCost, maxUpkeepBonus, (lvl) => UpkeepBonus = UpkeepBonusData[lvl]);
        MoralBonusTech = new Tech(MoralBonusCost, maxMoralBonus, (lvl) => MoralBonus = MoralBonusData[lvl]);
        BuildCostBonusTech = new Tech(BuildCostBonusCost, maxBuildCostBonus, (lvl) => BuildCostBonus = BuildCostBonusData[lvl]);
        EconomyBranchTech = new Tech(EconomyBranchCost, 1, (lvl) => { EconomyBranch = true; });

        TreasureBonusTech = new Tech[Treasury.ResourceCount];
        for (int i = 0; i < Treasury.ResourceCount; i++)
            TreasureBonusTech[i] = new Tech(TreasureBonusCost, maxTreasureBonus, (lvl) => TreasureBonus[i] = TreasureBonusData[lvl]);
    }
    
    public class Tech
    {
        public delegate void LevelUp(int lvl);
        public delegate void Research();
        public Research research;
        GetCost cost;
        int lvl, maxlvl;
        public bool able => lvl < maxlvl;
        public TimeCost timeScience => cost(lvl);
        public int time => cost(lvl).time;
        public float science => cost(lvl).science;

        public Tech(GetCost cost, int maxlvl, LevelUp action)
        {
            this.cost = cost;
            this.maxlvl = maxlvl;
            lvl = 0;
            research += () => action(lvl++);
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
}
