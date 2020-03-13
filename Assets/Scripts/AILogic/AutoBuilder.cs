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

    public void AutoBuilding(bool On)
    {
        if(On)
        {
           foreach(var prov in provinces)
                for(BuildingType type = BuildingType.Infrastructure;type != BuildingType.Count;type++)
                {
                    if (prov.data.CanPhysicalyBuild(type))
                        queue.Enqueue((prov.data, type));
                }
            RandomBuild();
            GameTimer.AddListener(RandomBuild, state.Data);

        } else
        {
            GameTimer.RemoveListener(RandomBuild, state.Data);
            queue.Clear();
        }
        IsOn = On;

    }
    void RandomBuild()
    {
        bool success = true;
        while (success && queue.Count > 0)
        {
            var pair = queue.Dequeue();
            ProvinceData prov = pair.Item1;
            BuildingType building = pair.Item2;

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
                else
                {
                    success = false;
                }
            }
        }
    }
}
