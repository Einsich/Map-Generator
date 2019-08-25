using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GarnisonIcon : InitGO,IPointerClickHandler
{
    public Image count, moral,icon;
    public Regiment garnison;
    public static bool canExchange=false;
    /// <param name="reg">class: Regiment</param>
    public override void Init(object reg)
    {
        garnison = (Regiment)reg;
        icon.sprite = garnison.baseRegiment.Icon;
        moral.fillAmount = garnison.moralF;
        count.fillAmount = garnison.countF;
    }
    void Update()
    {
        moral.fillAmount = garnison.moralF;
        count.fillAmount = garnison.countF;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(canExchange)
        {
            Player.army.ExchangeRegiment(garnison);
            MenuManager.UpdateArmyList();
            MenuManager.UpdateGarnisonList();
        }
    }
}
