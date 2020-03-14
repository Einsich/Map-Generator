using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalTrade 
{
    static Treasury Income;
    public static void DiscardCource() => Income = new Treasury();
    public static void AddIncome(Treasury income) => Income += income;

    public static float GetCource(ResourcesType bye, ResourcesType sell) => Mathf.Min(Income[(int)bye] / Income[(int)sell], 1000);

}
public class TradeDeal
{
    public readonly
        State State1, State2;
    ResourcesType ResType1, ResType2;
    float Res1, Res2;
    public TradeDeal(State seller, ResourcesType sellres, float sell, State byer, ResourcesType byeres, float bye) =>
        (State1, ResType1, Res1, State2, ResType2, Res2) = (seller, sellres, sell, byer, byeres, bye);
    public bool isValid => new Treasury(ResType1, Res1) <= State1.treasury && new Treasury(ResType2, Res2) <= State2.treasury;
    public void DoDeal()
    {
        Treasury d= new Treasury(ResType1, Res1);
        State1.stateAI.SomeOneSpentResources(-d, BudgetType.OtherBudget);
        State2.stateAI.IncomeResources(d);
        d = new Treasury(ResType2, Res2);
        State2.stateAI.SomeOneSpentResources(-d, BudgetType.OtherBudget);
        State1.stateAI.IncomeResources(d);
    }
    public bool WantTrade(bool state1)
    {
        return state1 ?
            State1.WantTrade(ResType1, Res1, ResType2, Res2) :
            State2.WantTrade(ResType2, Res2, ResType1, Res1);
    }
}