using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diplomacy {
    List<Diplomacy> war, alliance, trade, improveRelation, forceAccess;
    public List<float> casusbelli;
    public List<(Diplomacy, float)> 
    fabricateCB, destabilization, subsidies,
    coalicion;
/*
bool war alliance,forceAcces
Deal {int pr1,pr2,float dr}trade
Gift {float gift, float dr}
Fabracation{float dcb, float gold, float dr}
Destabilisation{float gold,float dr}

*/
public bool canDeclareWar(Diplomacy dip) => !haveWar(dip) && !haveAlliance(dip) && !haveAccess(dip);
    public bool haveWar(Diplomacy dip) => war.Contains(dip);
    public bool haveAlliance(Diplomacy dip) => alliance.Contains(dip);
    public bool haveAccess(Diplomacy dip) => forceAccess.Contains(dip);
    public bool fabricatingCasus(Diplomacy dip) => fabricateCB.Exists((x) => x.Item1 == dip);

    public void DeclareWar(Diplomacy dip, bool declare)
    {
        DeclaredWar(dip, declare);
        dip.DeclaredWar(this, declare);
    }
    public void DeclaredWar(Diplomacy dip, bool declare)
    {
        if (declare)
            war.Add(dip);
        else
            war.Remove(dip);
    }
    public void MakeAlliance(Diplomacy dip, bool make)
    {
        MakedAlliance(dip, make);
        dip.MakedAlliance(this, make);
    }
    public void MakedAlliance(Diplomacy dip, bool make)
    {
        if (make)
            alliance.Add(dip);
        else
            alliance.Remove(dip);
    }
    public void ForceAccess(Diplomacy dip, bool access)
    {
        if (access)
            forceAccess.Add(dip);
        else
            forceAccess.Remove(dip);
    }
    public void FabricateCasusBelli(Diplomacy dip, bool fabricate, float gold)
    {
        if (fabricate)
            fabricateCB.Add((dip, gold));
        else
            fabricateCB.RemoveAll((x) => x.Item1 == dip);
    }
    public bool canMove(Diplomacy dip) => haveWar(dip) || haveAlliance(dip) || haveAccess(dip);
    public bool canAttack(Diplomacy dip) => haveWar(dip);
    public State state;
    public Diplomacy(State state)
    {
        this.state = state;
        diplomacies[state.ID] = this;
        war = new List<Diplomacy>();
        alliance = new List<Diplomacy>();
        forceAccess = new List<Diplomacy>();
        casusbelli = new List<float>(diplomacies.Length);
        for(int i=0;i< diplomacies.Length;i++)
        {
            casusbelli.Add(0);
        }
        fabricateCB = new List<(Diplomacy, float)>();
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
                    casusbelli[i] += x.Item2;
                }

            }
            for (int i = 0; i < casusbelli.Count; i++)
                if (casusbelli[i] > 0.1f)
                    casusbelli[i] -= 0.1f;
                else
                    casusbelli[i] = 0;
        
    }
}
