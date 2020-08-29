using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GarnisonIcon : InitGO,IPointerClickHandler,IHelpBaseRegiment
{
    public Image count, moral,icon;
    public Regiment garnison;
    bool delete;
    public static bool canExchange=false;
    public BaseRegiment BaseRegiment { get => garnison.baseRegiment; set { } }
    /// <param name="reg">class: Regiment</param>
    public override void Init(object reg)
    {
        garnison = (Regiment)reg;
        icon.sprite = garnison.baseRegiment.Icon;
       // moral.fillAmount = garnison.moralF;
        count.fillAmount = garnison.NormalCount;
        Delete(false);
    }
    void Update()
    {
        //moral.fillAmount = garnison.moralF;
        count.fillAmount = garnison.NormalCount;
    }
    void Delete(bool del)
    {
        delete = del;
        icon.color = del ? Color.red : Color.white;
    }
    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.pointerId == -1)
        {
            if (Player.PlayerCheckArmy())
            {
                if (canExchange)
                {
                    Player.army.ExchangeRegiment(garnison);
                    MenuManager.UpdateArmyList();
                    MenuManager.UpdateGarnisonList();
                }
                Delete(false);
            }
        }
        else if (eventData.pointerId == -2)
        {
            if (Player.army && Player.army.army.Contains(garnison))
            {
                if (Player.PlayerCheckArmy())
                {
                    if (delete)
                    {
                        Player.army.army.Remove(garnison);
                        Player.army.ArmyListChange();
                        MenuManager.UpdateArmyList();
                    }
                    else
                        Delete(true);
                }
            }
            if (Player.curRegion != null && Player.curRegion.data.garnison.Contains(garnison))
            {
                if (Player.PlayerCheckRegion())
                {
                    if (delete)
                    {
                        Player.curRegion.data.garnison.Remove(garnison);
                        MenuManager.UpdateGarnisonList();

                    }
                    else
                        Delete(true);
                }
            }
        }
    }
}
