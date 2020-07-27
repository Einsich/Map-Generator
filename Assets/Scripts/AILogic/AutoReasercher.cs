using System;
using System.Collections.Generic;
using UnityEngine;

public class AutoReasercher :AutoManager
{
    private StateAI state;
    private TechnologyTree technologyTree;
    private PriorityQueue<ResearchTask> queue = new PriorityQueue<ResearchTask>();
    private int queueCounter;
    public Treasury NeedTreasure => queue.Count == 0 ? new Treasury() : new Treasury(ResourcesType.Science, queue.Peek().tech.curScience);
     public AutoReasercher(StateAI state)
    {
        this.state = state;
        technologyTree = state.Data.technologyTree;
    }
    private bool isOn = false;
    public bool IsOn
    {
        get => isOn; set
        {

            if (isOn == value)
                return;
            if (value)
            {
                RandomResearch();
                GameTimer.AddListener(RandomResearch, state.Data);
                technologyTree.TechWasResearchedEvent += RandomResearch;

            }
            else
            {
                GameTimer.RemoveListener(RandomResearch, state.Data);
                technologyTree.TechWasResearchedEvent -= RandomResearch;
            }
            isOn = value;

        }
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
            Technology tech = queue.Peek().tech;
            if ( tech.curScience <= state.GetTechnologyBudget.Science )
            {
                state.AutoManagersSpentResources(new Treasury(0, 0, 0, 0, tech.curScience), BudgetType.TechnologyBudget);
                tech.researchAction = new ResearchAction(tech, tech.curTime);
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
        foreach (Technology tech in technologyTree.technology)
            if (tech.researchType == ResearchType.CanResearch)
            {
                queue.Enqueue(new ResearchTask(tech));
            }
    }
    class ResearchTask:IComparable<ResearchTask>
    {
        public Technology tech;
        public ResearchTask(Technology tech) => this.tech = tech;

        public int CompareTo(ResearchTask other)
        {
            return tech.curScience.CompareTo(other.tech.curScience);
        }
    }
}
