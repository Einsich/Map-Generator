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
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 2; j++)
                pips[j * 3 + i].sprite = ProvinceMenu.instance.pips[regiment.pips[i, j]];
        name.text = regiment.name.ToString();
        descr.text = $"Стоимость {regiment.cost.ToArmyCost()}\nСодержание {regiment.upkeep.ToArmyCost()}\nВремя {regiment.time}";
        bye.onClick.RemoveAllListeners();
        bye.onClick.AddListener(() => ProvinceMenu.instance.recruitMenu.AddToQueue(regiment));
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
