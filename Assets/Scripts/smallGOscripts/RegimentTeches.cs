using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegimentTeches : InitGO
{
    public TechInterface[] teches;
    public Image[] pips;
    public Image icon;
    public override void Init(object initElement)
    {
        BaseRegiment regiment = (BaseRegiment)initElement;
        icon.sprite = regiment.Icon;
        /*for (int i = 0; i < BaseRegiment.NM; i++)
        {
            teches[i].Init(regiment.teches[i]);
            pips[i].sprite = SpriteHandler.GetPipsSprite(regiment.pips[i / BaseRegiment.PipsM, i % BaseRegiment.PipsM]);
            
        }*/
    }
}
