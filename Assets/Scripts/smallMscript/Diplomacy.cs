using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diplomacy {

    bool privalliance, privwar, privforceaccess;
    public bool alliance
    {
        get { return privalliance; }
        set { privalliance = value;
        }
    }
    public bool war
    {
        get { return privwar; }
        set { privwar = value;
        }
    }
    public bool forceaccess
    {
        get { return privforceaccess; }
        set { privforceaccess = value;
        }
    }
    public State s1, s2;
    public Diplomacy(State state1,State state2)
    {
        s1 = state1;
        s2 = state2;
        alliance = war = forceaccess = false;
    }
}
