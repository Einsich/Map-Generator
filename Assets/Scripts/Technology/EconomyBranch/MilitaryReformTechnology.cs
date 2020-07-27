using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MilitaryReform", menuName = "Technology/MilitaryReform", order = 1)]
public class MilitaryReformTechnology : Technology
{
    public float[] RegimentCostReduce;
    public override void LevelUp() => tree.regimentCostReduce = RegimentCostReduce[lvl - 1];
}