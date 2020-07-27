using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BurnedLand", menuName = "Technology/BurnedLand", order = 1)]
public class BurnedLandTechnology : Technology
{
    public float[] burnedLand;
    public override void LevelUp() => tree.burnedLandEffect = burnedLand[lvl - 1];
}
