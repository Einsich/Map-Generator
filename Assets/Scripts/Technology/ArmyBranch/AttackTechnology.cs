using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTechnology : Technology
{
    public int regimentOffset;
    public override void LevelUp() { tree.regiments[regimentOffset].damageLvl++; tree.regiments[regimentOffset].AllUpdate(); }
}