using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Treasury
{
    public float Gold, Manpower, Wood, Iron, Science;
    public Treasury(float Gold, float Manpower, float Wood, float Iron, float Science)
    {
        this.Gold = Gold;
        this.Manpower = Manpower;
        this.Wood = Wood;
        this.Iron = Iron;
        this.Science = Science;
    }
    public string ToArmyCost()
    {
        return $"({Gold}, {Wood}, {Iron})";
    }
    public Treasury(int value = 0)
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
