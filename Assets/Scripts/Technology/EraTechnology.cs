using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EraTech", menuName = "Technology/EraTech", order = 1)]
public class EraTechnology : Technology
{
    public override void LevelUp() => tree.technologyEra++;
}


