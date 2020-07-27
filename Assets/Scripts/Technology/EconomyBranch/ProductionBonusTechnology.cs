using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductionBonus", menuName = "Technology/ProductionBonus", order = 1)]
public class ProductionBonusTechnology : Technology
{
    public float[] productionBonus;
    public ResourcesType resourcesType;
    public override void LevelUp() => tree.treasureIncrease[resourcesType] = productionBonus[lvl - 1];
}