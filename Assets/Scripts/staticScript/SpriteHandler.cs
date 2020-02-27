using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class SpriteHandler
{
    static Sprite[] RegimentTypes;
    static Sprite[] PersonIcons;
    static Sprite[] Dots = new Sprite[13];
    static Sprite[] Skills;
    static SpriteDataBase SpriteDataBase;
    static Sprite[,] Buildings;
    static Sprite[] BuildFrames;
    static SpriteHandler()
    {
        SpriteDataBase = Resources.Load<SpriteDataBase>("Texture/SpriteDataBase");
        Texture2D dots = Resources.Load<Texture2D>("Texture/Dots");
        for (int i = 0; i < Dots.Length; i++)
            Dots[i] = Sprite.Create(dots, new Rect(32 * i, 0, 32, 20), new Vector2(0.5f, 0.5f));

        float height = SpriteDataBase.Buildings[0].height;
        int[] dx = { 0, 1, 2, 4, 5, 7, 11, 13, 14, 15, 16, 12 };
        Buildings = new Sprite[(int)FractionType.Count, dx.Length];
        for (int frac = 0; frac < (int)FractionType.Count; frac++)
            for (int i = 0; i < dx.Length; i++)
                Buildings[frac, i] = Sprite.Create(SpriteDataBase.Buildings[frac], new Rect(height * dx[i], 0, height, height), new Vector2(0.5f, 0.5f));

        RegimentTypes = SpriteDataBase.RegimentTypes;
        PersonIcons = SpriteDataBase.PersonTypes;
        Skills = SpriteDataBase.Skills;
        BuildFrames = SpriteDataBase.BuildFrames;
    }
    public static Sprite GetRegimentSprite(BaseRegiment regiment)
    {
        RegimentName name = regiment.name;
        Sprite s = null;
        switch(name)
        {
            case RegimentName.SimpleArta:s = RegimentTypes[3];break;
            case RegimentName.SimpleCavalry:s = RegimentTypes[2];break;
            case RegimentName.SimpleRanger:s = RegimentTypes[1] ; break;
            case RegimentName.SimpleMelee:s = RegimentTypes[0]; break;
            case RegimentName.Skeletons:s = RegimentTypes[4]; break;
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
            case PersonType.Warrior:s = PersonIcons[0];break;
            case PersonType.Knight:s = PersonIcons[1];break;
            case PersonType.Jaeger:s = PersonIcons[2];break;
            case PersonType.Archer:s = PersonIcons[3];break;
            case PersonType.Wizard:s = PersonIcons[4];break;
            case PersonType.Engineer:s = PersonIcons[5];break;
            case PersonType.Necromancer:s = PersonIcons[6];break;
            case PersonType.DeathKnight: s = PersonIcons[7]; break;

        }
        return s;
    }
    public static Sprite GetSkillSprite(SkillType type)
    {
        return Skills[(int)type];
    }
    public static Sprite[] GetCursors => SpriteDataBase.Cursors;
    public static Sprite GetBuilding(int buildingType, int fractionType) => Buildings[fractionType, buildingType];
    public static Sprite GetBuildFrame(BuildState state) => state == BuildState.CanBuild ? BuildFrames[0] : BuildFrames[1];
}

