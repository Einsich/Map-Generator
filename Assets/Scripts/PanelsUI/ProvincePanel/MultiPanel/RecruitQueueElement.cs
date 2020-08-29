﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecruitQueueElement : InitGO,IHelpBaseRegiment
{
    public Image fill,icon;
    public Text descr;
    public RecruitAction action;
    public BaseRegiment BaseRegiment { get => action.regiment; set { } }
    public override void Init(object act)
    {
        gameObject.SetActive(true);
        action = (RecruitAction)act;
        icon.sprite = action.regiment.Icon;
        fill.fillAmount = 0;
        descr.text = action.regiment.type.ToString();
    }
    public void DeRecruit()
    {
        if (Player.PlayerCheckRegion())
            RecruitMenu.RemoveFromQueue(action);
    }
    private void Update()
    {
       fill.fillAmount = action.progress;
    }
}
