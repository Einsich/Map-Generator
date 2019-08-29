using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyAI : MonoBehaviour
{
    Army army_;
    public Army army { set { army_ = value;owner = value.owner; }
        get => army_;
    }
    State owner;
    public void DoRandomMove()
    {
        if (army_.retreat)
            return;
        if (army_.curBattle != null)
            army_.curBattle.EndBattle(owner);
        else
            army_.TryMoveTo(owner.regions[Random.Range(0, owner.regions.Count)].Capital);
    }
}
