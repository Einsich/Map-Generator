using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecruitListElement : InitGO
{
    public Image icon;
    public Image[] pips;
    public Text name, descr;
    public Button bye;
    BaseRegiment regiment;
    public override void Init(object reg)
    {
        regiment = (BaseRegiment)reg;
        gameObject.SetActive(true);
        icon.sprite = regiment.Icon;
        //for (int i = 0; i < 3; i++)
          //  for (int j = 0; j < 2; j++)
            //    pips[j * 3 + i].sprite = SpriteHandler.GetPipsSprite(regiment.pips[i, j]);
        name.text = regiment.type.ToString();
        descr.text = $"Стоимость {regiment.cost.ToArmyCost()}\nСодержание {regiment.upkeep.ToArmyCost()}\nВремя {regiment.time}";
        bye.onClick.RemoveAllListeners();
        bye.onClick.AddListener(()=>RecruitMenu.AddToQueue(regiment));
        CheckCost();
    }
    public void CheckCost()
    {
        bye.interactable = regiment.cost <= Player.curPlayer.treasury;
    }
    private void Update()
    {
        CheckCost();
    }
}
