using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegimentTeches : InitGO
{
    public TechInterface[] ArmTeches;
    public Image[] ArmPips;
    public TechInterface DamTech;
    public Image DamPips;
    public Image icon;
    public override void Init(object initElement)
    {
        BaseRegiment regiment = (BaseRegiment)initElement;
        icon.sprite = regiment.Icon;
        for (int i = 0; i < (int)DamageType.Count; i++)
        {
            ArmTeches[i].Init(regiment.armTeches[i]);
            ArmPips[i].sprite = SpriteHandler.GetPipsSprite(regiment.armor[i]);            
        }

        DamTech.Init(regiment.damTech);
        DamPips.sprite = SpriteHandler.GetPipsSprite(regiment.damageLvl);
    }
}
