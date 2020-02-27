using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fraction {

    public FractionType fraction;
    public static GameObject[] TownPrefab;
    public static GameObject[] WallsPrefab;
}
public enum FractionType
{
    People,
    Orcs,
    Count
}