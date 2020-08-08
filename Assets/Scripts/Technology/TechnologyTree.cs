using System;
using System.Collections.Generic;
using UnityEngine;


public class TechnologyTree : ScriptableObject
{
    public Action TechWasResearchedEvent;
    public const int maximumEra = 3;
    public int technologyEra = 1;

    //military
    public BaseRegiment[] regiments;
    public float wallsCostReduce = 1;
    public float moveSpeedBonus = 1;
    //economica
    public int[] buildingsMaxLevel = new int[ProvinceData.buildingsCount];
    public float portTradeBonus = 1;
    public float regimentCostReduce = 1;
    public float regimentUpkeepReduce = 1;
    public float recruitInCountry = GameConst.RecruitInCountry;
    public float recruitInTown = GameConst.RecruitInTown;
    public Treasury treasureIncrease = new Treasury(1);

    //government
    public float passiveExperience = 0;
    public float activeExperienceBonus = 1;
    public float inquisitionBonus = 0;
    public int maxPerson = 1;
    public float burnedLandEffect = 0;
    public List<PersonType> openedPerson;


    //other parametrs//
    public State state;
    public List<Technology> technology = new List<Technology>();
    public List<int> parents = new List<int>();

    public void InitializeTree(State state)
    {
        this.state = state;
        state.technologyTree = this;
        buildingsMaxLevel[(int)(BuildingType.Infrastructure)] = ProvinceData.maxBuildingLevel;
        buildingsMaxLevel[(int)(BuildingType.Trade)] = ProvinceData.maxBuildingLevel;
        if (regiments != null)
            for (int i = 0; i < regiments.Length; i++)
                if (regiments[i] != null)
                    state.regiments[i] = Instantiate(regiments[i]);
        regiments = new BaseRegiment[GameConst.MaxRegimentCount];
        openedPerson = new List<PersonType>();
        BuildTree();
    }
    private void BuildTree()
    {
        for(int i=0;i<technology.Count;i++)
        {
            technology[i] = Instantiate(technology[i]);
            technology[i].parent = parents[i] < 0 ? null : technology[parents[i]];
            technology[i].tree = this;
            if (technology[i] is NewRegimentTechnology newRegiment)
                regiments[newRegiment.regimentOffset] = newRegiment.regimentInstance;
        }

    }
    public void TechnologyResearched(Technology technology) { TechWasResearchedEvent?.Invoke(); }
    public BaseRegiment GetRegimentInstance(int offset) => state.regiments[offset] ?? regiments[offset];
}

public class Technology : ScriptableObject
{
    [SerializeField] private float science, time;
    public int lvl = 0, lvlmax = 1;
    public new string name;
    public Sprite Icon;
    public GameAction researchAction = null;
    public Action buttonEvent;
    [HideInInspector] public Technology parent = null;
    [HideInInspector] public TechnologyTree tree;
    public int NodeID = -1;
    public ResearchType researchType => researchAction != null ? ResearchType.Researching :
        (lvl < lvlmax && (parent == null || parent.lvl != 0) && tree != null && lvl * TechnologyTree.maximumEra <= tree.technologyEra * lvlmax) ? ResearchType.CanResearch :
        lvl == lvlmax ? ResearchType.Limit : ResearchType.Blocked;
    public bool researchOrAbleResearch => researchAction != null || researchType == ResearchType.CanResearch;
    public bool notLimited => researchType != ResearchType.Limit;
    public float curScience => science * Mathf.Sqrt(lvl + 1);
    public float curTime => time * Mathf.Sqrt(lvl + 1);
    public void Research()
    {
        lvl++;
        LevelUp();
        researchAction = null;
        buttonEvent?.Invoke();
        tree.TechnologyResearched(this);
    }
    public virtual void LevelUp()
    {

    }
    public virtual string getDescription() { return ""; }

    public void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public enum ResearchType
{
    Researching,
    CanResearch,
    Blocked,
    Limit
}