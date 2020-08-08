using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroPull", menuName = "Technology/HeroPull", order = 1)]
public class HeroPullTechnology : Technology
{
    public override void LevelUp() => tree.maxPerson++;
    public override string getDescription()
    {
        return string.Format("Увеличивает доличество доступных для найма героев.");
    }
}