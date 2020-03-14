using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diplomacy {
    List<Diplomacy> war,  forceAccess;
    public List<float> relation;
    public List<(Diplomacy, float)> fabricateCB, uniqueCB, patronage;
    static List<TradeDeal> tradeDeals = new List<TradeDeal>();
    /*
    bool war alliance,forceAcces
    Deal {int pr1,pr2,float dr}trade
    Gift {float gift, float dr}
    Fabracation{float dcb, float gold, float dr}
    Destabilisation{float gold,float dr}

    */
    static float Sign(float x) => x < 0 ? -1 : x > 0 ? 1 : 0;
    public static float RelationDelta(float relation) => -relation * 0.01f - Sign(relation) * 0.1f;
    
    public bool canDeclareWar(Diplomacy dip) => !haveAccess(dip) && relation[dip.state.ID] < 0;
    public bool canForceAcces(Diplomacy dip)=> !haveWar(dip)  && relation[dip.state.ID] >= 0;
    public  bool canTrade(Diplomacy dip) =>  !haveWar(dip) && relation[dip.state.ID] >= 0;
    public bool canFabricate(Diplomacy dip) => !haveWar(dip);
    public bool canInsulting(Diplomacy dip) => relation[dip.state.ID] > 0;
    public bool canPatronage(Diplomacy dip) => !haveWar(dip);
    public bool canUniate(Diplomacy dip) => !haveWar(dip) && relation[dip.state.ID] >= 70;
    public bool haveWar(Diplomacy dip) => war.Contains(dip);
    public bool haveAccess(Diplomacy dip) => forceAccess.Contains(dip);
    public bool haveDeal(Diplomacy dip) => tradeDeals.Exists((x) => x.State1 == dip.state && x.State2 == state || x.State2 == dip.state && x.State1 == state);
    public bool fabricatingCasus(Diplomacy dip) => fabricateCB.Exists((x) => x.Item1 == dip);
    public bool havePatronage(Diplomacy dip) => patronage.Exists((x) => x.Item1 == dip);
    public void DeclareWar(Diplomacy dip, bool declare)
    {
        DeclaredWar(dip, declare);
        dip.DeclaredWar(this, declare);
    }
    private void DeclaredWar(Diplomacy dip, bool declare)
    {
        if (declare)
        {
            war.Add(dip);
            relation[dip.state.ID] = -100;
        }
        else
        {
            war.Remove(dip);
        }

    }
    public void ForceAccess(Diplomacy dip, bool access)
    {
        if (access)
            forceAccess.Add(dip);
        else
            forceAccess.Remove(dip);

        relation[dip.state.ID] += access ? 10 : -10;
    }
    public void FabricateCasusBelli(Diplomacy dip, bool fabricate, float gold)
    {
        if (fabricate)
            fabricateCB.Add((dip, gold));
        else
            fabricateCB.RemoveAll((x) => x.Item1 == dip);
    }
    public void SendOfferForceAccess(Diplomacy other)
    {
        if(other.state == Player.curPlayer)
        {

        } else
        {
            ForceAccess(other, true);
        }
    }
    public void SendOfferTradeDeal(Diplomacy other, TradeDeal deal)
    {
        if (other.state == Player.curPlayer)
        {

        }
        else
        {
            if (deal.WantTrade(other.state == deal.State1))
            { MakeTradeDeal(deal); }
            else
            {
                Debug.Log("Сделка отклонена");
            }
        }
    }
    public void Insult(Diplomacy dip) => relation[dip.state.ID] = 0;
    public void BeginPatronage(Diplomacy dip, bool begin)
    {
        if(begin)
        {
            patronage.Add((dip, 1));
        }
        else
        {
            patronage.RemoveAll((x) => x.Item1 == dip);
        }
    }
    public void Uniate(Diplomacy dip)
    {

    }
    public void MakeTradeDeal(TradeDeal deal) => tradeDeals.Add(deal);
    public void BreakTradeDeal(Diplomacy dip) => tradeDeals.RemoveAll((x) => x.State1 == dip.state && x.State2 == state || x.State2 == dip.state && x.State1 == state);
    public bool canMove(Diplomacy dip) => haveWar(dip)|| haveAccess(dip);

    public float relationDelta(Diplomacy dip)
    {
        float d = -fabricateCB.Find((x) => x.Item1 == dip).Item2;
        d += RelationDelta(relation[dip.state.ID]);
        return d;
    }
    public State state;
    public Diplomacy(State state)
    {
        this.state = state;
        diplomacies[state.ID] = this;
        war = new List<Diplomacy>();
        forceAccess = new List<Diplomacy>();
        relation = new List<float>(diplomacies.Length);
        for(int i=0;i< diplomacies.Length;i++)
        {
            relation.Add(0);
        }
        fabricateCB = new List<(Diplomacy, float)>();
        uniqueCB = new List<(Diplomacy, float)>();
        patronage = new List<(Diplomacy, float)>();
    }


    public static Diplomacy GetDiplomacy(State state) => state != null ? diplomacies[state.ID]: null;
    public static Diplomacy[] diplomacies;
    public static void InitDiplomacy(int n)
    {
        diplomacies = new Diplomacy[n];
    }
    public  void DiplomacyUpdate()
    {
       
            foreach(var x in fabricateCB)
            {
                int i = x.Item1.state.ID;
                if(state.treasury.Gold >= x.Item2)
                {
                    state.SpendTreasure(new Treasury(x.Item2,0,0,0,0), BudgetType.OtherBudget);
                    relation[i] -= x.Item2;
                }

            }
        for (int i = 0; i < relation.Count; i++)
            if (Mathf.Abs(relation[i]) > 0.1f)
                relation[i] += RelationDelta(relation[i]);
            else
                relation[i] = 0;
        
    }
}
