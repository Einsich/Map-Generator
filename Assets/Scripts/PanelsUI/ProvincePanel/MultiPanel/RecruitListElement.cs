using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecruitListElement : InitGO,IHelpBaseRegiment
{
    public Image icon;
    public Image[] ArmPips;
    public Image DamPips;
    public Text name, descr;
    public Button bye;
    public BaseRegiment BaseRegiment { get; set; }
    public override void Init(object reg)
    {
        BaseRegiment = (BaseRegiment)reg;
        gameObject.SetActive(true);
        icon.sprite = BaseRegiment.Icon;

        for (int i = 0; i < (int)DamageType.Count; i++)
        {
            ArmPips[i].sprite = SpriteHandler.GetPipsSprite(BaseRegiment.ArmorLvl((DamageType)i));
        }
        DamPips.sprite = SpriteHandler.GetPipsSprite(BaseRegiment.damageLvl);

        name.text = BaseRegiment.type.ToString();
        descr.text = $"Стоимость {BaseRegiment.cost.ToArmyCost()}\nСодержание {BaseRegiment.upkeep.ToArmyCost()}\nВремя {BaseRegiment.time}";
        bye.onClick.RemoveAllListeners();
        bye.onClick.AddListener(()=>RecruitMenu.AddToQueue(BaseRegiment));
        CheckCost();
    }
    public void CheckCost()
    {
        bye.interactable = BaseRegiment.cost <= Player.curPlayer.treasury;
    }
    private void Update()
    {
        CheckCost();
    }
}
