using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoReasercher 
{
    private StateAI state;
    private Technology technology;
    public bool IsOn { get; private set; } = false;
    public AutoReasercher(StateAI state)
    {
        this.state = state;
        technology = state.Data.technology;
    }

    public void AutoResearching(bool On)
    {
        if (On)
        {
            RandomResearch();
            GameTimer.SeldomUpdate += RandomResearch;
            technology.TechWasResearchedEvent += RandomResearch;

        }
        else
        {
            GameTimer.SeldomUpdate -= RandomResearch;
            technology.TechWasResearchedEvent -= RandomResearch;
        }
        IsOn = On;

    }
    void RandomResearch()
    {
        bool change = false;
        foreach(Tech tech in technology.allTeches)
            if (tech.canResearch && tech.science <= state.GetTechnologyBudget.Science && technology.TreeCondition(tech))
            {
                state.AutoManagersSpentResources(new Treasury(0, 0, 0, 0, tech.science), BudgetType.TechnologyBudget);
                tech.researchAction = new ResearchAction(tech, tech.time);
                change = true;
            }
        if (change)
            state.Data.TreasureChange?.Invoke();
    }
}
