using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateAI 
{
    private State Data;
    public StateAI(State state)
    {
        Data = state;
    }
    float armyBudget, buildingBudget, technologyBudget, otherBudget;
    static Treasury scienceMask = new Treasury(1,1,1,1,0);
    public Treasury ArmyBudget, BuildingBudget, OtherBudget,TechnologyBudget;
    public void SomeOneSpentResources(Treasury delta)
    {
        delta = Delta(delta, ref OtherBudget);
        
        if(!(Treasury.zero <= delta)) 
        {
            delta = Delta(delta, ref BuildingBudget);
            if(!(Treasury.zero <= delta))
            {
                delta = Delta(delta, ref ArmyBudget);
                if(!(Treasury.zero <= delta))
                {
                    TechnologyBudget -= delta;
                }
            }
        }
    }

    private Treasury Delta(Treasury delta, ref Treasury treasury)
    {
        delta = treasury -= delta;
        for (int i = 0; i < 5; i++)
        {
            if (treasury[i] < 0)
                treasury[i] = 0;
            if (delta[i] > 0)
                delta[i] = 0;
        }
        return delta;
    }
}
