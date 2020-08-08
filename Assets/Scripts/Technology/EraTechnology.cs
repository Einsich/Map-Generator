using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EraTech", menuName = "Technology/EraTech", order = 1)]
public class EraTechnology : Technology
{
    public override void LevelUp() => tree.technologyEra++;
    public override string getDescription()
    {
        return string.Format("Новая технологическая эра, дает доступ к многоуровневым исследованиям.");
    }
}


