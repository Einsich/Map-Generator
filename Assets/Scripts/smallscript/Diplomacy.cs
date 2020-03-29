using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diplomacy {
    List<Diplomacy> war, forceAccess;
    public List<float> relation;
    public List<(Diplomacy, float)> fabricateCB, uniqueCB, patronage;
    public List<TradeDeal> tradeDeals;
    public System.Action DiplomacyAction;
    public const float tradeRelationDelta = 10, forceAccesRealtionDelta = 10f, warDeclareRelation = -50;

    static float Sign(float x) => x < 0 ? -1 : x > 0 ? 1 : 0;
    public static float RelationDelta(float relation) => -relation * 0.01f - Sign(relation) * 0.1f;

    public bool canDeclareWar(Diplomacy dip) => !haveAccess(dip) && relation[dip.state.ID] <= 0;
    public bool canForceAcces(Diplomacy dip) => !haveWar(dip) && relation[dip.state.ID] >= 0;
    public bool canTrade(Diplomacy dip) => !haveWar(dip) && relation[dip.state.ID] >= 0;
    public bool canFabricate(Diplomacy dip) => !haveWar(dip);
    public bool canInsulting(Diplomacy dip) => relation[dip.state.ID] > 0;
    public bool canPatronage(Diplomacy dip) => !haveWar(dip);
    public bool canUniate(Diplomacy dip) => !haveWar(dip) && relation[dip.state.ID] >= 70;
    public bool haveWar(Diplomacy dip) => war.Contains(dip);
    public bool haveAccess(Diplomacy dip) => forceAccess.Contains(dip);
    public bool haveDeal(Diplomacy dip) => tradeDeals.Exists((x) => x.State1 == dip.state  || x.State2 == dip.state );
    public TradeDeal findDeal(Diplomacy dip) => tradeDeals.Find((x) => x.State1 == dip.state || x.State2 == dip.state);
    public bool fabricatingCasus(Diplomacy dip) => fabricateCB.Exists((x) => x.Item1 == dip);
    public bool havePatronage(Diplomacy dip) => patronage.Exists((x) => x.Item1 == dip);
    public void DeclareWar(Diplomacy dip, bool declare)
    {
        DeclaredWar(dip, declare);
        dip.DeclaredWar(this, declare);
        DiplomacyAction?.Invoke();
        dip.DiplomacyAction?.Invoke();
    }
    private void DeclaredWar(Diplomacy dip, bool declare)
    {
        if (declare)
        {
            war.Add(dip);
            state.DeclareWarPenalty(Mathf.Clamp01(relation[dip.state.ID] / warDeclareRelation));
            relation[dip.state.ID] = -100;
        }
        else
        {
            war.Remove(dip);
        }

        DiplomacyAction?.Invoke();
        dip.DiplomacyAction?.Invoke();
    }
    public void ForceAccess(Diplomacy dip, bool access)
    {
        if (access)
            forceAccess.Add(dip);
        else
            forceAccess.Remove(dip);

        relation[dip.state.ID] += access ? forceAccesRealtionDelta : -forceAccesRealtionDelta;

        DiplomacyAction?.Invoke();
        dip.DiplomacyAction?.Invoke();
    }
    public void FabricateCasusBelli(Diplomacy dip, bool fabricate, float gold)
    {
        if (fabricate)
            fabricateCB.Add((dip, gold));
        else
            fabricateCB.RemoveAll((x) => x.Item1 == dip);

        DiplomacyAction?.Invoke();
        dip.DiplomacyAction?.Invoke();
    }
    public void SendOfferForceAccess(Diplomacy other)
    {
        if (other.state == Player.curPlayer)
        {

        } else
        {
            ForceAccess(other, true);
        }

    }
    public bool SendOfferTradeDeal(Diplomacy other, TradeDeal deal)
    {

        if (other.state.stateAI.autoTrader.IsOn)

        {
            if (deal.WantTrade(other.state == deal.State1))
            {
                MakeTradeDeal(deal);
                deal.StartDeal();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {   
            if (other.state == Player.curPlayer)
            {
            }
        }
        return false;
    }
    public void Insult(Diplomacy dip)
    {
        relation[dip.state.ID] = 0;
        DiplomacyAction?.Invoke();
        dip.DiplomacyAction?.Invoke();
    }
    public void BeginPatronage(Diplomacy dip, bool begin)
    {
        if (begin)
        {
            patronage.Add((dip, 5));
        }
        else
        {
            patronage.RemoveAll((x) => x.Item1 == dip);
        }

        DiplomacyAction?.Invoke();
        dip.DiplomacyAction?.Invoke();
    }
    public void Uniate(Diplomacy dip)
    {

        DiplomacyAction?.Invoke();
        dip.DiplomacyAction?.Invoke();
    }
    public void MakeTradeDeal(TradeDeal deal)
    {
        deal.State1.diplomacy.relation[deal.State2.ID] += tradeRelationDelta;
        deal.State2.diplomacy.relation[deal.State1.ID] += tradeRelationDelta;

        //tradeDeals.Add(deal);

        deal.State1.diplomacy.DiplomacyAction?.Invoke();
        deal.State2.diplomacy.DiplomacyAction?.Invoke();
    }
    public void BreakTradeDeal(Diplomacy dip)
    {
        for(int i=0;i<tradeDeals.Count;i++)
        
            if (tradeDeals[i].State1 == dip.state || tradeDeals[i].State2 == dip.state)
            {
                (State s1, State s2) = (tradeDeals[i].State1, tradeDeals[i].State2);
                s1.diplomacy.relation[s2.ID] -= tradeRelationDelta;
                s2.diplomacy.relation[s1.ID] -= tradeRelationDelta;

                s1.diplomacy.DiplomacyAction?.Invoke();
                s2.diplomacy.DiplomacyAction?.Invoke();
                tradeDeals[i].BreakDeal();
                i--;
            }
    }
    public bool canMove(Diplomacy dip) => haveWar(dip)|| haveAccess(dip);

    public float relationDelta(Diplomacy dip)
    {
        float d = -fabricateCB.Find((x) => x.Item1 == dip).Item2;
        d += patronage.Find((x) => x.Item1 == dip).Item2;
        d += RelationDelta(relation[dip.state.ID]);
        if (haveWar(dip))
            d = -100;
        return d;
    }
    public State state;
    public Diplomacy(State state)
    {
        this.state = state;
        tradeDeals = state.stateAI.autoTrader.deals;
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
                relation[i] =Mathf.Clamp(relation[i] +  relationDelta(diplomacies[i]),-100,100);
            else
                relation[i] = 0;
        
    }
}
