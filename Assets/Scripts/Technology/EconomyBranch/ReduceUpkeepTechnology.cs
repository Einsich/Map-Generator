using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReduceUpkeep", menuName = "Technology/ReduceUpkeep", order = 1)]
public class ReduceUpkeepTechnology : Technology
{
    public float[] reduseUpkeepBonus;
    public override void LevelUp() => tree.regimentUpkeepReduce = reduseUpkeepBonus[lvl - 1];
}
