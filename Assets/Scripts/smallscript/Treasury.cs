using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Treasury
{
    public const int ResourceCount = 5;
    public float Gold, Manpower, Wood, Iron, Science;
    public static Treasury zero => new Treasury();
    public Treasury(float Gold, float Manpower, float Wood, float Iron, float Science)
    {
        this.Gold = Gold;
        this.Manpower = Manpower;
        this.Wood = Wood;
        this.Iron = Iron;
        this.Science = Science;
    }

    public float this[int i]
    {
        get
        {
            switch (i)
            {
                case 0: return Gold;
                case 1: return Manpower;
                case 2: return Wood;
                case 3: return Iron;
                default: return Science;
            }
        }
        set
        {
            switch (i)
            {
                case 0: Gold = value; break;
                case 1: Manpower = value; break;
                case 2: Wood = value; break;
                case 3: Iron = value; break;
                default: Science = value; break;
            }
        }
    }
    public float this[ResourcesType i]
    {
        get => this[(int)i];
        set => this[(int)i] = value;
    }
    public static string ToString(int i)
    {
        switch (i)
        {
            case 0: return "Золото";
            case 1: return "Рекруты";
            case 2: return "Дерево";
            case 3: return "Железо";
            default: return "Наука";
        }
    }
    public static TechType ToTechType(int i)
    {
        switch (i)
        {
            case 0: return TechType.GoldBonus;
            case 1: return TechType.ManPowerBonus;
            case 2: return TechType.WoodBonus;
            case 3: return TechType.IronBonus;
            default: return TechType.ScienceBonus;
        }
    }
    public void NormalizeToGold()
    {
        Manpower *= GlobalTrade.GetCource(ResourcesType.Gold, ResourcesType.Manpower);
        Wood *= GlobalTrade.GetCource(ResourcesType.Gold, ResourcesType.Wood);
        Iron *= GlobalTrade.GetCource(ResourcesType.Gold, ResourcesType.Iron);
        Science *= GlobalTrade.GetCource(ResourcesType.Gold, ResourcesType.Science);
    }
    public float Sum => Gold + Manpower + Wood + Iron + Science;
    public string ToArmyCost()
    {
        return $"({Gold}, {Wood}, {Iron})";
    }
    public Treasury(float value = 0)
    {
        Gold = Manpower = Wood = Iron = Science = value;
    }
    public Treasury(ResourcesType type, float value)
    {
        Gold = Manpower = Wood = Iron = Science = 0;
        switch (type)
        {
            case ResourcesType.Gold: Gold = value; break;
            case ResourcesType.Manpower: Manpower = value; break;
            case ResourcesType.Wood: Wood = value; break;
            case ResourcesType.Iron: Iron = value; break;
            case ResourcesType.Science: Science = value; break;
        }
    }
    public static Treasury operator + (Treasury a,Treasury b)
    {
        return new Treasury(a.Gold + b.Gold, a.Manpower + b.Manpower, a.Wood + b.Wood, a.Iron + b.Iron, a.Science + b.Science);
    }
    public static Treasury operator -(Treasury a, Treasury b)
    {
        return new Treasury(a.Gold - b.Gold, a.Manpower - b.Manpower, a.Wood - b.Wood, a.Iron - b.Iron, a.Science - b.Science);
    }
    public static Treasury operator -(Treasury b)
    {
        return new Treasury(-b.Gold, -b.Manpower, -b.Wood, -b.Iron, -b.Science);
    }
    public static Treasury operator *(Treasury a, float b)
    {
        return new Treasury(a.Gold * b, a.Manpower * b, a.Wood * b, a.Iron * b, a.Science * b);
    }
    public static Treasury operator *(Treasury a, Treasury b)
    {
        return new Treasury(a.Gold * b.Gold, a.Manpower * b.Manpower, a.Wood * b.Wood, a.Iron * b.Iron, a.Science * b.Science);
    }
    public static bool operator <=(Treasury a, Treasury b)
    {
        return (a.Gold <= b.Gold && a.Manpower <= b.Manpower && a.Wood <= b.Wood && a.Iron <= b.Iron && a.Science <= b.Science);
    }
    public static bool operator >=(Treasury a, Treasury b)
    {
        return (a.Gold >= b.Gold && a.Manpower >= b.Manpower && a.Wood >= b.Wood && a.Iron >= b.Iron && a.Science >= b.Science);
    }
    public static bool operator <(Treasury a, Treasury b)
    {
        return (a.Gold < b.Gold && a.Manpower < b.Manpower && a.Wood < b.Wood && a.Iron < b.Iron && a.Science < b.Science);
    }
    public static bool operator >(Treasury a, Treasury b)
    {
        return (a.Gold > b.Gold && a.Manpower > b.Manpower && a.Wood > b.Wood && a.Iron > b.Iron && a.Science > b.Science);
    }
    public bool isEmpty => Gold == 0 && Manpower == 0 && Wood == 0 && Iron == 0 && Science == 0;
    static string[] toStr = { "зол", "рек", "древ", "жел", "науки" };
    public override string ToString()
    {
        string s = "";
        bool z = false;
        for (int i = 0; i < 5; i++)
            if (this[i] != 0)
            {
                if (z)
                    s += ", ";
                s += $"{this[i].ToString("n1")} {toStr[i]}";
                z = true;
            }
        return s;
    }
}
public enum ResourcesType
{
    Gold, Manpower, Wood, Iron, Science
}