using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBuilder :AutoManager
{
    private StateAI state;
    private List<Region> provinces;
    private PriorityQueue<BuildTask> queue = new PriorityQueue<BuildTask>();
    private int updateQueue;
    public Treasury NeedTreasure => queue.Count == 0 ? new Treasury() : queue.Peek().Prov.Cost(queue.Peek().Building);
    private class BuildTask : IComparable<BuildTask>
    {
        private float priority;
        public ProvinceData Prov { get; private set; }
        public BuildingType Building { get; private set; }

        public BuildTask(float priority, ProvinceData prov, BuildingType building)
        {
            this.priority = priority;
            Prov = prov;
            Building = building;
        }

        int IComparable<BuildTask>.CompareTo(BuildTask other)
        {
            return priority.CompareTo(other.priority);
        }
    }
    public AutoBuilder(StateAI state)
    {
        this.state = state;
        provinces = state.Data.regions;
    }
    
    private bool isOn = false;
    public bool IsOn
    {
        get => isOn; set
        {
            if (value)
            {
                BestBuild();
                GameTimer.AddListener(BestBuild, state.Data);
            }
            else
            {
                GameTimer.RemoveListener(BestBuild, state.Data);
                queue.Clear();
            }
            isOn = value;

        }
    }
    public void IncludeBuildTask(ProvinceData prov, BuildingType building)
    {
        queue.Enqueue(new BuildTask(float.NegativeInfinity, prov, building));
    }

   
    void BestBuild()
    {
        if (updateQueue-- == 0)
        {
            UpdateQueueAndResetCounter();
        }

        bool success = true;
        while (success && queue.Count > 0)
        {
            var buildTask = queue.Peek();

            ProvinceData prov = buildTask.Prov;
            BuildingType building = buildTask.Building;

            Treasury cost;
            if ((cost = prov.Cost(building)) <= state.GetBuildingBudget)
            {
                if (prov.CanPhysicalyBuild(building) && !prov.IsBuilding(building))
                {
                    queue.Dequeue();
                    prov.ForwardBuild(building);
                    state.AutoManagersSpentResources(cost, BudgetType.BuildingBudget);
                    state.Data.TreasureChange?.Invoke();
                    prov.SomeChanges?.Invoke();

                    var tmpCortege = prov.GetBestBuilding();
                    if (tmpCortege.Item2 != BuildingType.Count)
                    {
                        queue.Enqueue(new BuildTask(tmpCortege.Item1, prov, tmpCortege.Item2));
                    }
                }
                else
                {
                    queue.Dequeue();
                }
            }
            else
            {
                success = false;
            }
        }
    }

    private void UpdateQueueAndResetCounter()
    {
        updateQueue = 5;
        queue.Clear();
        foreach (var region in provinces)
        {
            ProvinceData prov = region.data;
            var tmpCortege = prov.GetBestBuilding();

            float profit = tmpCortege.Item1;
            BuildingType building = tmpCortege.Item2;

            if (building != BuildingType.Count)
            {
                queue.Enqueue(new BuildTask(profit, prov, building));
            }
        }
    }
}
