using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveExp", menuName = "Technology/PassiveExp", order = 1)]
public class PassiveExpTechnology : Technology
{
    public float[] passiveExp;
    public override void LevelUp() => tree.passiveExperience = passiveExp[lvl - 1];
}