using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTrader 
{
    private StateAI state;
    private Diplomacy diplomacy;
    public List<TradeDeal> deals = new List<TradeDeal>();
    static List<(float, ResourcesType)> sortList = new List<(float, ResourcesType)>(5);
    public AutoTrader(StateAI stateAI) => (state, diplomacy) = (stateAI, stateAI.Data.diplomacy);
    public bool IsOn { get; private set; } = false;

    public bool DealsOverflow => deals.Count >= 4;
    public void AutoTrading(bool On)
    {
        if (On)
        {
            //FindTradeDeals();
            //GameTimer.AddListener(FindTradeDeals, state.Data);
            GameTimer.AddListener(TryTrade, state.Data);

        }
        else
        {
            //GameTimer.RemoveListener(FindTradeDeals, state.Data);
            GameTimer.RemoveListener(TryTrade, state.Data);
        }
        IsOn = On;
    }


    private void TryTrade()
    {
        Treasury deficit = state.GetTreasure;
        deficit.NormalizeToGold();
        sortList.Clear();
        for (int i = 0; i < (int)ResourcesType.Count; i++)
        {
            sortList.Add((deficit[i], (ResourcesType)i));
        }
        sortList.Sort();
        variants[0] = (sortList[0].Item2, sortList[4].Item2);
        variants[1] = (sortList[0].Item2, sortList[3].Item2);
        variants[2] = (sortList[1].Item2, sortList[4].Item2);
        variants[3] = (sortList[1].Item2, sortList[3].Item2);
        foreach(var p in variants)
        {
            (ResourcesType buy, ResourcesType sell) = p;
            float sellR = deficit[sell];
            if(sellR > 0 )
            {
                sellR = Mathf.Min(sellR, GlobalTrade.BufferRes(buy) * GlobalTrade.GetCource(ResourcesType.Gold, buy));
                Deal deal = new Deal(state.Data, sell, sellR * GlobalTrade.GetCource(sell, ResourcesType.Gold), buy, sellR * GlobalTrade.GetCource(buy, ResourcesType.Gold));
                if(sellR > 0 && GlobalTrade.TryMakeDeal(deal))
                {
                    GlobalTrade.MakeDeal(deal);
                    return;
                }
            }
        }

    }
    static (ResourcesType, ResourcesType)[] variants = new (ResourcesType, ResourcesType)[4];
    private void FindTradeDeals()
    {
        if (DealsOverflow)
            return;
        Treasury incom = state.Data.Income ;
        incom.NormalizeToGold();
        sortList.Clear();
        for (int i = 0; i < (int)ResourcesType.Count; i++)
        {
            sortList.Add((-incom[i], (ResourcesType)i));
        }
        sortList.Sort();
        GlobalTrade.UpdateWantToTrade(state,sortList);
        variants[0] = (sortList[0].Item2, sortList[4].Item2);
        variants[1] = (sortList[0].Item2, sortList[3].Item2);
        variants[2] = (sortList[1].Item2, sortList[4].Item2);
        variants[3] = (sortList[1].Item2, sortList[3].Item2);
        GlobalTrade.TryMakeDeals(variants, state);

    }
    public void DealsUpdate()
    {
        RemoveBadDeals();
        DoDeals();
    }
    public void RemoveBadDeals()
    {
        for (int i = 0; i < deals.Count; i++) 
            if(!deals[i].isValid(state.Data == deals[i].State1))
            {
                deals[i].BreakDeal();
                i--;
            }
        while (DealsOverflow)
            deals[0].BreakDeal();
    }
    public void DoDeals() => deals.ForEach((x) => x.DoDeal(state.Data == x.State1));
}
