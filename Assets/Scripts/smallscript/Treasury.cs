using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Treasury
{
    public const int ResourceCount = 5;
    public float Gold, Manpower, Wood, Iron, Science;
    public Treasury(float Gold, float Manpower, float Wood, float Iron, float Science)
    {
        this.Gold = Gold;
        this.Manpower = Manpower;
        this.Wood = Wood;
        this.Iron = Iron;
        this.Science = Science;
    }
    public float this [int i]
    {
        get
        {
            switch(i)
            {
                case 0: return Gold;
                case 1:return Manpower;
                case 2:return Wood;
                case 3:return Iron;
                default:return Science;                        
            }
        }
        set
        {
            switch (i)
            {
                case 0: Gold = value;break;
                case 1: Manpower = value; break;
                case 2: Wood = value; break;
                case 3: Iron = value; break;
                default: Science = value; break;
            }
        }
    }
    public string ToArmyCost()
    {
        return $"({Gold}, {Wood}, {Iron})";
    }
    public Treasury(float value = 0)
    {
        Gold = Manpower = Wood = Iron = Science = value;
    }
    public static Treasury operator + (Treasury a,Treasury b)
    {
        return new Treasury(a.Gold + b.Gold, a.Manpower + b.Manpower, a.Wood + b.Wood, a.Iron + b.Iron, a.Science + b.Science);
    }
    public static Treasury operator -(Treasury a, Treasury b)
    {
        return new Treasury(a.Gold - b.Gold, a.Manpower - b.Manpower, a.Wood - b.Wood, a.Iron - b.Iron, a.Science - b.Science);
    }
    public static Treasury operator *(Treasury a, float b)
    {
        return new Treasury(a.Gold * b, a.Manpower * b, a.Wood * b, a.Iron * b, a.Science * b);
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
    
}
