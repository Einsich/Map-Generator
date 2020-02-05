using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpriteHandler
{

    static Sprite[] RegimentTypes = new Sprite[3];
    static Sprite[] PersonIcons;
    static Sprite[] Dots = new Sprite[13];
    static SpriteHandler()
    {
        Texture2D unitIcons = Resources.Load<Texture2D>("Texture/UnitTypes");
        Texture2D dots = Resources.Load<Texture2D>("Texture/Dots");
        for (int i = 0; i < RegimentTypes.Length; i++)        
            RegimentTypes[i] = Sprite.Create(unitIcons, new Rect(47 * i, 0, 47, 47), new Vector2(0.5f, 0.5f));
        for (int i = 0; i < Dots.Length; i++)
            Dots[i] = Sprite.Create(dots, new Rect(32 * i, 0, 32, 20), new Vector2(0.5f, 0.5f));

        PersonIcons = Resources.LoadAll<Sprite>("PersonIcons");

    }
    public static Sprite GetRegimentSprite(BaseRegiment regiment)
    {
        Sprite s = null;
        RegimentType type = regiment.type;
        switch(type)
        {
            case RegimentType.Artillery:s = RegimentTypes[2];break;
            case RegimentType.Cavalry:s = RegimentTypes[1];break;
            case RegimentType.Infantry:s = RegimentTypes[0];break;
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
