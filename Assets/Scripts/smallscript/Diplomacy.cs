using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diplomacy {

    bool alliance_, war_, forceaccess_;
    public bool alliance
    {
        get { return alliance_; }
        set { alliance_ = value;
        }
    }
    public bool war
    {
        get { return war_; }
        set { war_ = value;
        }
    }
    public bool forceaccess
    {
        get { return forceaccess_; }
        set { forceaccess_ = value;
        }
    }
    public bool itsI;
    public bool canMove => alliance_ || war_ || forceaccess_ || itsI;
    public bool canAttack => war_ && !itsI;
    public State s1, s2;
    public Diplomacy(State state1,State state2)
    {
        s1 = state1;
        s2 = state2;
        alliance = war = forceaccess = false;
        itsI = state1 == state2;
        diplomacies[state1.ID, state2.ID] = diplomacies[state2.ID, state1.ID] = this;
    }
    public static Diplomacy GetDiplomacy(State state1, State state2) => state1 != null && state2 != null ? diplomacies[state1.ID, state2.ID]: null;
    public static Diplomacy[,] diplomacies;
    public static void InitDiplomacy(int n)
    {
        diplomacies = new Diplomacy[n, n];
    }
}
