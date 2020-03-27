using System;
using System.Collections.Generic;
using UnityEngine;

public class AutoReasercher 
{
    private StateAI state;
    private Technology technology;
    private PriorityQueue<ResearchTask> queue = new PriorityQueue<ResearchTask>();
    private int queueCounter;
    public Treasury NeedTreasure => queue.Count == 0 ? new Treasury() : new Treasury(ResourcesType.Science,queue.Peek().tech.science);
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
            GameTimer.AddListener(RandomResearch, state.Data);
            technology.TechWasResearchedEvent += RandomResearch;

        }
        else
        {
            GameTimer.RemoveListener(RandomResearch, state.Data);
            technology.TechWasResearchedEvent -= RandomResearch;
        }
        IsOn = On;

    }
    void RandomResearch()
    {
        if(queueCounter-- == 0 || queue.Count == 0)
        {
            RebuildQueue();
        }
        bool change = false;
        while (queue.Count > 0)
        {
            Tech tech = queue.Peek().tech;
            if ( tech.science <= state.GetTechnologyBudget.Science )
            {
                state.AutoManagersSpentResources(new Treasury(0, 0, 0, 0, tech.science), BudgetType.TechnologyBudget);
                tech.researchAction = new ResearchAction(tech, tech.time);
                queue.Dequeue();
                change = true;
            } else
            {
                break;
            }
        }
        if (change)
            state.Data.TreasureChange?.Invoke();
    }
    private void RebuildQueue()
    {
        queueCounter = 5;
        queue.Clear();
        foreach (Tech tech in technology.allTeches)
            if (tech.canResearch && technology.TreeCondition(tech))
            {
                queue.Enqueue(new ResearchTask(tech));
            }
    }
    class ResearchTask:IComparable<ResearchTask>
    {
        public Tech tech;
        public ResearchTask(Tech tech) => this.tech = tech;

        public int CompareTo(ResearchTask other)
        {
            return tech.science.CompareTo(other.tech.science);
        }
    }
}
