using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class SpriteHandler
{
    static Sprite[] PersonIcons;
    static Sprite[] Dots = new Sprite[13];
    static SpriteDataBase SpriteDataBase;
     static SpriteHandler()
    {
        SpriteDataBase = Resources.Load<SpriteDataBase>("SpriteDataBase");
        Texture2D dots = Resources.Load<Texture2D>("Texture/Dots");
        for (int i = 0; i < Dots.Length; i++)
            Dots[i] = Sprite.Create(dots, new Rect(32 * i, 0, 32, 20), new Vector2(0.5f, 0.5f));

        PersonIcons = Resources.LoadAll<Sprite>("PersonIcons");

    }
    public static Sprite GetRegimentSprite(BaseRegiment regiment)
    {
        Sprite[] RegimentTypes = SpriteDataBase.RegimentTypes;
        RegimentType type = regiment.type;
        Sprite s = null;
        switch(type)
        {
            case RegimentType.Artillery:s = RegimentTypes[3];break;
            case RegimentType.Cavalry:s = RegimentTypes[2];break;
            case RegimentType.Infantry:s =regiment.damageType == DamageType.Range? RegimentTypes[1] : RegimentTypes[0]; break;
        }
        return s;
    }
    public static Sprite GetPipsSprite(int level)
    {
        if (level < 0)
            return Dots[0];
        if (level >= Dots.Length)
            return Dots[Dots.Length - 1];
        return Dots[level];
    }
    public static Sprite GetPersonIcon(PersonType type)
    {
        Sprite s = null;
        switch(type)
        {
            case PersonType.Knight:s = PersonIcons[0];break;
            case PersonType.Trader:s = PersonIcons[1];break;
        }
        return s;
    }
}

