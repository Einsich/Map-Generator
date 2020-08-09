using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBuilder : AutoManager, AutoSpender
{
    private StateAI state;
    private List<Region> provinces;

    public PriorityQueue<BuildTask> queue = new PriorityQueue<BuildTask>();
    private int updateQueue = 0;

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

            if (isOn == value)
                return;
            isOn = value;
            if (value)
            {
                state.ProcessOnlyBuildAndRegimnts();
            }
            else
            {
                queue.Clear();
            }
        }
    }
    public void IncludeBuildTask(ProvinceData prov, BuildingType building)
    {
        queue.Enqueue(new BuildTask(float.NegativeInfinity, prov, building));
    }


    public AutoSpenderResult TrySpend(StateAI state)
    {
        if (updateQueue-- <= 0)
        {
            UpdateQueueAndResetCounter();
        }
        while (queue.Count > 0)
        {
            var buildTask = queue.Peek();

            ProvinceData prov = buildTask.Prov;
            BuildingType building = buildTask.Building;

            Treasury cost;
            if ((cost = prov.Cost(building)) <= state.treasury)
            {
                if (prov.CanPhysicalyBuild(building))
                {
                    queue.Dequeue();
                    prov.ForwardBuild(building);
                    state.SpentResources(cost);
                    state.Data.TreasureChange?.Invoke();
                    prov.SomeChanges?.Invoke();

                    var tmpCortege = prov.GetBestBuilding();
                    if (tmpCortege.Item2 != null)
                    {
                        queue.Enqueue(new BuildTask(tmpCortege.Item1, prov, tmpCortege.Item2.Value));
                    }
                    return AutoSpenderResult.Success;
                }
                else
                {
                    queue.Dequeue();
                }
            }
            else
            {
                return AutoSpenderResult.NeedMoreResources;
            }
        }
        return AutoSpenderResult.HasNotOrder;
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
            BuildingType? building = tmpCortege.Item2;

            if (building != null)
            {
                queue.Enqueue(new BuildTask(profit, prov, building.Value));
            }
        }
    }

    public class BuildTask : IComparable<BuildTask>
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
}
