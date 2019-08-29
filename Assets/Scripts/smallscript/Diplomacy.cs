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
    public bool canMove => alliance_ || war_ || forceaccess_;
    public bool canAttack => war_;
    public State s1, s2;
    public Diplomacy(State state1,State state2)
    {
        s1 = state1;
        s2 = state2;
        alliance = war = forceaccess = false;
    }
}
