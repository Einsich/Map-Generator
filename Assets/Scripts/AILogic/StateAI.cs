using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateAI
{
    public State Data;
    public AutoBuilder autoBuilder;
    public AutoReasercher autoReasercher;
    public AutoTrader autoTrader;
    public AutoRegimentBuilder autoRegimentBuilder;
    public AutoArmyCommander autoArmyCommander;

    public StateAI(State state)
    {
        Data = state;
        autoBuilder = new AutoBuilder(this);
        autoReasercher = new AutoReasercher(this);
        autoTrader = new AutoTrader(this);
        autoRegimentBuilder = new AutoRegimentBuilder(this);
        autoArmyCommander = new AutoArmyCommander(this);
    }
    public float armyBudget { get; private set; } = 0.1f;
    public float buildingBudget { get; private set; } = 0.9f;
    public float technologyBudget { get; private set; } = 0.95f;

    static Treasury nonscienceMask = new Treasury(1, 1, 1, 1, 0), scienceMask = new Treasury(0, 0, 0, 0, 1);
    private Treasury ArmyBudget,BuildingBudget, TechnologyBudget, OtherBudget;
    /// <summary>
    /// Нехватка - положительные компоненты означают нехватку
    /// </summary>
    public Treasury Deficit => (autoBuilder.NeedTreasure + autoReasercher.NeedTreasure + autoRegimentBuilder.NeedTreasure)- GetTreasure;
    public Treasury GetArmyBudget => ArmyBudget;
    public Treasury GetBuildingBudget => BuildingBudget;
    public Treasury GetOtherBudget => OtherBudget;
    public Treasury GetTechnologyBudget => TechnologyBudget;
     
    public Treasury GetTreasure => ArmyBudget + BuildingBudget + OtherBudget + TechnologyBudget;

    public void ChangeBudget(float t, BudgetType budgetType)
    {
        float d;
        switch(budgetType)
        {
            case BudgetType.ArmyBudget: d = t - armyBudget;
                if (buildingBudget - 0.5f * d <= 0)
                    buildingBudget = 0;
                else
                    if (1 - buildingBudget - armyBudget - d * 0.5f > 0)
                    buildingBudget -= d * 0.5f;
                else
                    buildingBudget -=  buildingBudget + armyBudget + d - 1;
                armyBudget += d;
                break;
            case BudgetType.BuildingBudget: d = t - buildingBudget;

                if (armyBudget - 0.5f * d <= 0)
                    armyBudget = 0;
                else
                    if (1 - buildingBudget - armyBudget - d * 0.5f > 0)
                    armyBudget -= d * 0.5f;
                else
                    armyBudget -= buildingBudget + armyBudget + d - 1;

                buildingBudget += d;
                break;
            case BudgetType.TechnologyBudget: technologyBudget = t;break;
        }
    }
    private ref Treasury Budget(int k)
    {
        switch(k)
        {
            case 0: return ref ArmyBudget;
            case 1:return ref BuildingBudget;
            case 2:return ref TechnologyBudget;
            default:return ref OtherBudget;

        }
    }
    static int[,] Priority = { { 0, 3, 1, 2 }, { 1, 3, 0, 2 }, { 2, 3, 1, 0 }, { 3, 0, 1, 2 } };
    private ref Treasury BudgetPriority(BudgetType budgetType, int priority) => ref Budget(Priority[(int)budgetType,priority]);
    public void IncomeResources(Treasury treasury)
    {
        Treasury nonscince = treasury * nonscienceMask;
        Treasury da = nonscince * armyBudget, db = nonscince * buildingBudget, ds = treasury * scienceMask * technologyBudget;
        ArmyBudget += da;
        BuildingBudget += db;
        TechnologyBudget += ds;
        OtherBudget += treasury - da - db - ds;
    }
    public void IncomeResources(Treasury treasury, BudgetType budgetType = BudgetType.OtherBudget)
    {
        Budget((int)budgetType) += treasury;
    }
    public void AutoManagersSpentResources(Treasury delta, BudgetType type)
    {
        BudgetPriority(type, 0) -= delta;
    }
    public void SomeOneSpentResources(Treasury delta, BudgetType budgetType)
    {

        delta = Delta(delta, ref BudgetPriority(budgetType, 0));
        
        if(!delta.isEmpty)
        {
            delta = Delta(delta, ref BudgetPriority(budgetType, 1));
            if(!delta.isEmpty)
            {
                delta = Delta(delta, ref BudgetPriority(budgetType, 2));
                if(!delta.isEmpty)
                {
                    BudgetPriority(budgetType, 3) -= delta;
                }
            }
        }
    }

    private Treasury Delta(Treasury delta, ref Treasury treasury)
    {
        delta = -(treasury -= delta);
        for (int i = 0; i < 5; i++)
        {
            if (treasury[i] < 0)
                treasury[i] = 0;
            if (delta[i] < 0)
                delta[i] = 0;
        }
        return delta;
    }
}
public enum BudgetType
{
    ArmyBudget,
    BuildingBudget,
    TechnologyBudget,
    OtherBudget
}