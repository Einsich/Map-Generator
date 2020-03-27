using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeDeal
{
    public readonly
        State State1, State2;
    ResourcesType ResType1, ResType2;
    float Res1, Res2;
    public TradeDeal(State seller, ResourcesType sellres, float sell, State byer, ResourcesType byeres, float bye)
    {
        (State1, ResType1, Res1, State2, ResType2, Res2) = (seller, sellres, sell, byer, byeres, bye);
        GameTimer.AddListener(DoDeal1, State1);
        GameTimer.AddListener(DoDeal2, State2);
    }
    public void BreakDeal()
    {
        GameTimer.RemoveListener(DoDeal1, State1);
        GameTimer.RemoveListener(DoDeal2, State2);
    }
    public bool isValid => new Treasury(ResType1, Res1) <= State1.treasury && new Treasury(ResType2, Res2) <= State2.treasury;

    private void DoDeal1()
    {
        State1.stateAI.SomeOneSpentResources(new Treasury(ResType1, -Res1), BudgetType.OtherBudget);
        State1.stateAI.IncomeResources(new Treasury(ResType2, Res2));
    }
    private void DoDeal2()
    {

        State2.stateAI.SomeOneSpentResources(new Treasury(ResType2, -Res2), BudgetType.OtherBudget);
        State2.stateAI.IncomeResources(new Treasury(ResType1, Res1));
    }
    public bool WantTrade(bool state1)
    {
        return state1 ?
            State1.WantTrade(ResType1, Res1, ResType2, Res2) :
            State2.WantTrade(ResType2, Res2, ResType1, Res1);
    }
}
