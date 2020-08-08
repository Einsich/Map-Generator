using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diplomacy {
    public List<WarData> war;
    private List<Diplomacy> forceAccess;
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
    public bool haveWar(Diplomacy dip) => dip != this && war.Exists((x) => x.Contains(dip.state));
    public WarData getWar(Diplomacy dip) => war.Find((x) => x.Contains(dip.state));
    public bool haveAccess(Diplomacy dip) => forceAccess.Contains(dip);
    public bool haveDeal(Diplomacy dip) => tradeDeals.Exists((x) => x.State1 == dip.state  || x.State2 == dip.state );
    public TradeDeal findDeal(Diplomacy dip) => tradeDeals.Find((x) => x.State1 == dip.state || x.State2 == dip.state);
    public bool fabricatingCasus(Diplomacy dip) => fabricateCB.Exists((x) => x.Item1 == dip);
    public bool havePatronage(Diplomacy dip) => patronage.Exists((x) => x.Item1 == dip);
    public static void DeclareWar(Diplomacy attacker, Diplomacy defender, bool declare)
    {
        
        if(declare)
        {
            WarData war = new WarData(attacker.state, defender.state);
            attacker.war.Add(war);
            defender.war.Add(war);
            attacker.state.DeclareWarPenalty(Mathf.Clamp01(attacker.relation[defender.state.ID] / warDeclareRelation));
            Debug.Log(string.Format("{0} declare war to {1} !", attacker.state.name, defender.state.name));
            if (defender.state == Player.curPlayer)
                Debug.Log("Вам объявили войну!");
        }
        attacker.DeclaredWar(defender, declare);
        defender.DeclaredWar(attacker, declare);
        attacker.DiplomacyAction?.Invoke();
        defender.DiplomacyAction?.Invoke();
    }
    private void DeclaredWar(Diplomacy dip, bool declare)
    {
        if (declare)
        {
            relation[dip.state.ID] = -100;
            state.stateAI.autoArmyCommander.DeclaredWar();
        }
        else
        {
            war.RemoveAll((x) => x.Contains(dip.state));
            var target = state.GetWarTarget();
            if (target != null)
                state.diplomacy.uniqueCB.Add((target.diplomacy, 1));
        }

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
        war = new List<WarData>();
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
public class WarData
{
    public State[] states = new State[2];
    private int[] occupated = new int[2];
    private float startWarTime;
    public float WarProgress(State state) => (float)OccupatedBy(state) / state.regions.Count - (float)OccupatedIn(state) / (Enemy(state).regions.Count);
    public int OccupatedBy(State occupator) => OccupatedIn(Enemy(occupator));
    public int OccupatedIn(State target)
    {
        target = target == states[0] ? states[0] : states[1];
        State occupator = target == states[0] ? states[1] : states[0];
        int occupate = 0;
        foreach (var reg in target.regions)
            occupate += reg.ocptby == occupator ? 1 : 0;
        return occupate;
    }
    public int OccupatedTotalIn(State target)
    {
        target = target == states[0] ? states[0] : states[1];
        int occupate = 0;
        foreach (var reg in target.regions)
            occupate += reg.ocptby != null ? 1 : 0;
        return occupate;
    }
    public bool canAcceptWhitePeace(State who)//for AI 
    {
        return GameTimer.time - startWarTime > 5 && (OccupatedBy(who) < 3 || WarProgress(who) < 0.2f);
    }
    public bool canWasAnnexated(State who)
    {
        return OccupatedTotalIn(who) == who.regions.Count;
    }
    public bool Contains(State state) => state == states[0] || state == states[1];
    public State Enemy(State state)=>state == states[0] ? states[1] : states[0];
    public WarData(State attacker, State defender)
    {
        states[0] = attacker;
        states[1] = defender;
        startWarTime = GameTimer.time;

    }
}