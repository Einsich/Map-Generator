using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WallsCostReduceTech", menuName = "Technology/WallsCostReduceTech", order = 1)]
public class WallsCostReduceTechnology : Technology
{
    public float[] percent;
    public override void LevelUp() { tree.wallsCostReduce = percent[lvl - 1]; }
    public override string getDescription()
    {
        return string.Format("Удешевляет стоимость строительства крепостей на {0}", percent[lvl].ToPercent());
    }
}