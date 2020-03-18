using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacyInfo : InitGO
{
    [SerializeField] private Image Icon, War, forceAcces,Trade, CasusBelli, Patronage, UniqueCB;
    [SerializeField] private Text Relation, Delta;
    private DiplomacyProxi proxi;
    public override void Init(object initElement)
    {
        proxi = (DiplomacyProxi)initElement;
        Icon.sprite = proxi.state.flagSprite;
        War.color = proxi.war ? Color.white : Color.clear;
        forceAcces.color = proxi.forceAccess ? Color.white : Color.clear;
        Trade.color = proxi.deal != null ? Color.white : Color.clear;
        CasusBelli.color = proxi.fabrCB != null ? Color.white : Color.clear;
        Patronage.color = proxi.patronage != null ? Color.white : Color.clear;
        UniqueCB.color = proxi.uniqueCB != null ? Color.white : Color.clear;
        Relation.text = DiplomacyMenu.ColoredRelation(proxi.relation, proxi.delta);
    }

}
public class DiplomacyProxi
{
    public State state;
    public float relation, delta;
    public bool war, forceAccess;
    public float? fabrCB, patronage, uniqueCB;
    public TradeDeal deal;
    public DiplomacyProxi(Diplomacy we, Diplomacy they) =>
        (state, relation, delta, war, forceAccess, fabrCB, patronage, uniqueCB, deal) =
        (they.state, we.relation[they.state.ID], we.relationDelta(they), we.haveWar(they), we.haveAccess(they), we.fabricatingCasus(they) ? we.fabricateCB.Find((x) => x.Item1 == they).Item2 : (float?)null,
        we.havePatronage(they) ? we.patronage.Find((x) => x.Item1 == they).Item2 : (float?)null, 
        we.uniqueCB.Exists((x) => x.Item1 == they) ? we.uniqueCB.Find((x) => x.Item1 == they).Item2 : (float?)null, we.findDeal(they));
}
