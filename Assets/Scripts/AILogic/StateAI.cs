using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateAI
{
    public State Data;
    private AutoManager[] autoManagers;
    public AutoBuilder autoBuilder;
    public AutoReasercher autoReasercher;
    public AutoTrader autoTrader;
    public AutoRegimentBuilder autoRegimentBuilder;
    public AutoArmyCommander autoArmyCommander;
    public AutoPersonControl autoPersonControl;
    public AutoDiplomacy autoDiplomacy;
    public AutoSpenderManager buildAndregimentsManager;
    public AutoSpenderManager scienceManager;
    public Treasury treasury { get; private set; }
    public StateAI(State state)
    {
        Data = state;
        autoBuilder = new AutoBuilder(this);
        autoReasercher = new AutoReasercher(this);
        autoTrader = new AutoTrader(this);
        autoRegimentBuilder = new AutoRegimentBuilder(this);
        autoArmyCommander = new AutoArmyCommander(this);
        autoPersonControl = new AutoPersonControl(this);
        autoDiplomacy = new AutoDiplomacy(this);
        buildAndregimentsManager = new AutoSpenderManager((autoBuilder, 50), (autoRegimentBuilder, 35));
        scienceManager = new AutoSpenderManager((autoReasercher, 10));
    }

    public void IncomeResources(Treasury treasury)
    {
        this.treasury += treasury;
    }
    public void SpentResources(Treasury delta)
    {
        treasury -= delta;
    }
    public void ProcessOrders()
    {
        buildAndregimentsManager.Process(this);
        scienceManager.Process(this);
    }
    public void ProcessOnlyScience() => scienceManager.Process(this);
    public void ProcessOnlyBuildAndRegimnts() => buildAndregimentsManager.Process(this);

}
public enum BudgetType
{
    ArmyBudget,
    BuildingBudget,
    TechnologyBudget,
    OtherBudget
}

public class AutoSpenderManager
{
    AutoSpender[] spenders;
    int[] spenderIndex;
    int[] localCount;
    int offset = 0, total;
    public AutoSpenderManager(params (AutoSpender, int)[] spenders)
    {
        int n = spenders.Length;
        this.spenders = new AutoSpender[n];
        localCount = new int[n];
        total = 0;
        for(int i=0;i<n;i++)
        {
            this.spenders[i] = spenders[i].Item1;
            localCount[i] = spenders[i].Item2;
            total += localCount[i];
        }
        spenderIndex = new int[total];
        Perturb();
    }
    public void SetNewCounters(params int[] counters)
    {
        if (counters.Length != localCount.Length)
            throw new System.Exception("Bad size array");
        int total = 0;
        for(int i=0;i<counters.Length;i++)
        {
            localCount[i] = counters[i];
            total += localCount[i];
        }
        if (total != this.total)
            throw new System.Exception("Bad counters");
        Perturb();
    }
    private void Perturb()
    {

        for (int i = 0, j = 0; i < localCount.Length; i++)
            for (int k = 0; k < localCount[i]; k++, j++)
                spenderIndex[j] = i;
        for(int i = total - 1; i>=0;i--)
        {
            int j = Random.Range(0, i);
            int t = spenderIndex[i];
            spenderIndex[i] = spenderIndex[j];
            spenderIndex[j] = t;
        }
    }

    public void Process(StateAI state)
    {
        for(int guard = 0;guard < total;guard++,offset = (offset + 1)%total)
        {
            AutoSpender spender = spenders[spenderIndex[offset]];
            if(spender.IsOn)
            {
                var result = spender.TrySpend(state);
                if (result == AutoSpenderResult.NeedMoreResources)
                    return;
            }
        }
    }
}