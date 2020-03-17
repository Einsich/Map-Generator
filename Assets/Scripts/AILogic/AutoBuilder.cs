using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBuilder 
{
    private StateAI state;
    private List<Region> provinces;
    private Queue<(ProvinceData, BuildingType)> queue = new Queue<(ProvinceData, BuildingType)>();
    public bool IsOn { get; private set; } = false;
    public AutoBuilder(StateAI state)
    {
        this.state = state;
        provinces = state.Data.regions;
    }
    
    public void IncludeBuildTask(ProvinceData prov, BuildingType building)
    {
        queue.Enqueue((prov, building));
    }

    public void AutoBuilding(bool On)
    {
        if(On)
        {
            BestBuild();
            GameTimer.AddListener(BestBuild, state.Data);

        } else
        {
            GameTimer.RemoveListener(BestBuild, state.Data);
            queue.Clear();
        }
        IsOn = On;

    }
    void BestBuild()
    {
        while (queue.Count > 0)
        {
            var pair = queue.Dequeue();

            ProvinceData prov = pair.Item1;
            BuildingType building = pair.Item2;

            CheckCostAndForwardBuild(prov, building);
        }
        foreach (var region in provinces)
        {
            ProvinceData prov = region.data;
            BuildingType building = prov.GetBestBuilding();

            if (building != BuildingType.Count)
            {
                CheckCostAndForwardBuild(prov, building);
            }
        }
    }

    private void CheckCostAndForwardBuild(ProvinceData prov, BuildingType building)
    {
            Treasury cost;
            if (prov.CanPhysicalyBuild(building) && !prov.IsBuilding(building))
            {
                if ((cost = prov.Cost(building)) <= state.GetBuildingBudget)
                {
                    prov.ForwardBuild(building);
                    state.AutoManagersSpentResources(cost, BudgetType.BuildingBudget);
                    state.Data.TreasureChange?.Invoke();
                    prov.SomeChanges?.Invoke();
                }
            }
    }
}
