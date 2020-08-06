using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDiplomacy : AutoManager
{
    private StateAI state;
    public AutoDiplomacy(StateAI stateAI)
    {
        state = stateAI;
    }
    private bool isOn = false;
    public bool IsOn
    {
        get => isOn; set
        {

            if (isOn == value)
                return;
            if (value)
            {
                GameTimer.AddListener(DiplomacyUpdate, state.Data);
            }
            else
            {
                GameTimer.RemoveListener(DiplomacyUpdate, state.Data);
            }
            isOn = value;
        }
    }

    public Treasury NeedTreasure => throw new System.NotImplementedException();

    public void DiplomacyUpdate()
    {
        State state = this.state.Data;
        Diplomacy diplomacy = state.diplomacy;
        var war = diplomacy.war.Count > 0 ? diplomacy.war[0] : null;
        if(war != null)
        {
            if (war.canWasAnnexated(war.Enemy(state)))
            {
                state.Annexation(war.Enemy(state));
                Diplomacy.DeclareWar(state.diplomacy, war.Enemy(state).diplomacy, false);
                
            }
        }
        var dip = diplomacy.uniqueCB.Count > 0 ? diplomacy.uniqueCB[0] : (null, 0);

        if(dip.Item1 != null)
        {
            Diplomacy.DeclareWar(diplomacy, dip.Item1, true);
            diplomacy.uniqueCB.RemoveAt(0);
        }

    }
}
