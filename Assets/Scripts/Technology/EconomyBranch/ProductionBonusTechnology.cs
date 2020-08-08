using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductionBonus", menuName = "Technology/ProductionBonus", order = 1)]
public class ProductionBonusTechnology : Technology
{
    public float[] productionBonus;
    public ResourcesType resourcesType;
    public override void LevelUp() => tree.treasureIncrease[resourcesType] = productionBonus[lvl - 1];

    public override string getDescription()
    {
        string type = "";
            switch(resourcesType)
        {
            case ResourcesType.Gold:type = "золота";break;
            case ResourcesType.Manpower:type = "людских ресурсов";break;
            case ResourcesType.Wood:type = "дерева";break;
            case ResourcesType.Iron:type = "железа";break;
            case ResourcesType.Science:type = "науки";break;
        }
        return string.Format("Увеличивает производство {0} от всех источников на {1}", type ,productionBonus[lvl].ToPercent());
    }
}