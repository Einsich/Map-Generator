using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveExp", menuName = "Technology/ActiveExp", order = 1)]
public class ActiveExpTechnology : Technology
{
    public float[] activeExp;
    public override void LevelUp() => tree.activeExperienceBonus = activeExp[lvl - 1];
}
