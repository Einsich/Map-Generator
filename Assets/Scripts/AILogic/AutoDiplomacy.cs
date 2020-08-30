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
                DiplomacyUpdate();
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
        new GameAction(Random.Range(1, 10), RealUpdate); 

    }
    private void RealUpdate()
    {
        State state = this.state.Data;
        Diplomacy diplomacy = state.diplomacy;
        if (state.destroyed)
            return;
        for(int i = 0;i < diplomacy.war.Count; i++)
        {
            var war = diplomacy.war[i];
            if (war.canWasAnnexated(war.Enemy(state)))
            {
                state.Annexation(war.Enemy(state));
                Diplomacy.DeclareWar(state.diplomacy, war.Enemy(state).diplomacy, false);
                i--;
            }
        }
        var dip = diplomacy.war.Count == 0 && diplomacy.uniqueCB.Count > 0 ? diplomacy.uniqueCB[0] : (null, 0);

        if (dip.Item1 != null)
        {
            Diplomacy.DeclareWar(diplomacy, dip.Item1, true);
            diplomacy.uniqueCB.RemoveAt(0);
        }
    }
}
