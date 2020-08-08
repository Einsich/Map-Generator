using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReduceUpkeep", menuName = "Technology/ReduceUpkeep", order = 1)]
public class ReduceUpkeepTechnology : Technology
{
    public float[] reduseUpkeepBonus;
    public override void LevelUp() => tree.regimentUpkeepReduce = reduseUpkeepBonus[lvl - 1];

    public override string getDescription()
    {
        return string.Format("Уменьшает затраты на содержание армии на {0}", reduseUpkeepBonus[lvl].ToPercent());
    }
}
