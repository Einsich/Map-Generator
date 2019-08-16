using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GarnisonIcon : InitGO
{
    public Image count, moral,icon;
    public Regiment garnison;

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
}
