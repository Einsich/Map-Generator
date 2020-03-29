using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Deal
{
    public readonly State State;
    public readonly ResourcesType SellRes, BuyRes;
    public readonly float Sell, Buy;
    public Deal(State state, ResourcesType sellres, float sell,  ResourcesType byeres, float bye) =>
        (State, SellRes, Sell, BuyRes, Buy) = (state, sellres, sell,  byeres, bye);
}
public class TradeDeal
{
    public readonly
        State State1, State2;
    ResourcesType ResType1, ResType2;
    float Res1, Res2;
    public TradeDeal(State seller, ResourcesType sellres, float sell, State byer, ResourcesType byeres, float bye) =>    
        (State1, ResType1, Res1, State2, ResType2, Res2) = (seller, sellres, sell, byer, byeres, bye);
      
    
    public void StartDeal()
    {

        State1.stateAI.autoTrader.deals.Add(this);
        State2.stateAI.autoTrader.deals.Add(this);
        GameTimer.AddListener(DoDeal1, State1);
        GameTimer.AddListener(DoDeal2, State2);
        Debug.Log($"{State1.name} и {State2.name} заключили сделку");
    }
    public void BreakDeal()
    {
        GameTimer.RemoveListener(DoDeal1, State1);
        GameTimer.RemoveListener(DoDeal2, State2);
        State1.stateAI.autoTrader.deals.Remove(this);
        State2.stateAI.autoTrader.deals.Remove(this);
    }
    public bool isValid(bool state1) => state1? new Treasury(ResType1, Res1) <= State1.treasury : new Treasury(ResType2, Res2) <= State2.treasury;
    public void DoDeal(bool state1) { if (state1) DoDeal1(); else DoDeal2(); }
    private void DoDeal1()
    {
        State1.Income += new Treasury(ResType1, -Res1);
        State1.Income += new Treasury(ResType2, Res2);
    }
    private void DoDeal2()
    {
        State2.Income += new Treasury(ResType2, -Res2);
        State2.Income += new Treasury(ResType1, Res1);
    }
    public bool WantTrade(bool state1)
    {
        return state1 ?
            State1.WantTrade(ResType1, Res1, ResType2, Res2) :
            State2.WantTrade(ResType2, Res2, ResType1, Res1);
    }
}
