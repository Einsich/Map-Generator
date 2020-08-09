using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalTrade 
{
    public const float MaxSellPercent = 0.15f, StartBuffer = 0.1f;
    static Treasury Income = new Treasury(1), PrevIncome = new Treasury();
    static string s = "";static int p = 0;
    public static System.Action BufferChanged;
    public static void DiscardCource() { Income -= PrevIncome; PrevIncome = Income; s +=Income.ToString()+"\n";if (p++==10) Debug.Log(s); }
    public static void AddIncome(Treasury income) { Income += income; }

    public static float GetCource(ResourcesType bye, ResourcesType sell) => Mathf.Min(Income[(int)bye] / Income[(int)sell], 1000);

    static HashSet<StateAI>[] wantToBuy = new HashSet<StateAI>[(int)ResourcesType.Count] {new HashSet<StateAI>(), new HashSet<StateAI>() , new HashSet<StateAI>() ,
    new HashSet<StateAI>(),new HashSet<StateAI>()};
    public static bool WantToBuy(State state, ResourcesType type) => wantToBuy[(int)type].Contains(state.stateAI);
    public static void UpdateWantToTrade(StateAI state, List<(float, ResourcesType)> sortList)
    {
        for (int i = 0; i < (int)ResourcesType.Count; i++)
        {
            wantToBuy[i].Remove(state);
        }
        wantToBuy[(int)sortList[0].Item2].Add(state);
        wantToBuy[(int)sortList[1].Item2].Add(state);
    }

    static Treasury Buffer;
    public static float BufferRes(ResourcesType type) => Buffer[type];
    public static void StartGlobalTrade() => Buffer = Income * StartBuffer;

    public static bool TryMakeDeal(Deal deal) => deal.BuyRes != deal.SellRes && deal.Buy <= Buffer[deal.BuyRes];
    public static void MakeDeal(Deal deal)
    {
        Buffer[deal.BuyRes] -= deal.Buy;
        Buffer[deal.SellRes] += deal.Sell;
        deal.State.stateAI.IncomeResources(new Treasury(deal.BuyRes, deal.Buy));
        deal.State.stateAI.SpentResources(new Treasury(deal.SellRes, deal.Sell));
        if (Buffer[deal.BuyRes] < 0.01f)
            Buffer[deal.BuyRes] = 0;
        deal.State.TreasureChange?.Invoke();
        BufferChanged?.Invoke();
        Debug.Log($"Was trade {Buffer.ToString()}");
    }
    public static void TryMakeDeals((ResourcesType buy, ResourcesType sell)[] variants, StateAI trader )
    {
        if(GameTimer.time >=9)
        {
            int t = 0;
        }
        for (int i = 0; i < variants.Length; i++)
        {
            (ResourcesType buy, ResourcesType sell) = variants[i];
            foreach (StateAI other in wantToBuy[(int)sell])
                if (other != trader)
                {
                    float maxbuy = other.Data.Income[buy] * MaxSellPercent * GetCource(sell, buy);
                    float maxsell = trader.Data.Income[sell] * MaxSellPercent;
                    float sellR = Mathf.Min(maxbuy, maxsell);
                    TradeDeal deal;
                    if (other.Data.diplomacy.SendOfferTradeDeal(trader.Data.diplomacy, deal = new TradeDeal(trader.Data, sell, sellR, other.Data, buy, sellR * GetCource(buy, sell))))
                    {
                        deal.StartDeal();
                        if (trader.autoTrader.DealsOverflow)
                            return;
                    }
                }
        }
    }

}
