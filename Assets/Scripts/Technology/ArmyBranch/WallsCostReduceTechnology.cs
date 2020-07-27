using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WallsCostReduceTech", menuName = "Technology/WallsCostReduceTech", order = 1)]
public class WallsCostReduceTechnology : Technology
{
    public float[] percent;
    public override void LevelUp() { tree.wallsCostReduce = percent[lvl - 1]; }
}