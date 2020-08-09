using System;
using System.Collections.Generic;
using UnityEngine;

public class AutoReasercher :AutoManager, AutoSpender
{
    private StateAI state;
    private TechnologyTree technologyTree;
    private PriorityQueue<ResearchTask> queue = new PriorityQueue<ResearchTask>();
    private int queueCounter;
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
            isOn = value;
            if (value)
            {
                state.ProcessOnlyScience();
                technologyTree.TechWasResearchedEvent += state.ProcessOnlyScience;

            }
            else
            {
                technologyTree.TechWasResearchedEvent -= state.ProcessOnlyScience;
            }

        }
    }
    public AutoSpenderResult TrySpend(StateAI state)
    {
        if(queueCounter-- <= 0 || queue.Count == 0)
        {
            RebuildQueue();
        }
        bool change = false;
        AutoSpenderResult result = AutoSpenderResult.Success;
        if (queue.Count > 0)
        {
            Technology tech = queue.Peek().tech;
            if (tech.curScience <= state.treasury.Science)
            {
                state.SpentResources(new Treasury(0, 0, 0, 0, tech.curScience));
                tech.researchAction = new ResearchAction(tech, tech.curTime);
                queue.Dequeue();
                change = true;
            }
            else
            {
                result = AutoSpenderResult.NeedMoreResources;
            }
        }
        else
            result = AutoSpenderResult.HasNotOrder;
        if (change)
            state.Data.TreasureChange?.Invoke();
        return result;
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
