using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTechnology : Technology
{
    public int regimentOffset;
    public override void LevelUp() { tree.state.regiments[regimentOffset].damageLvl++; tree.state.regiments[regimentOffset].AllUpdate(); }
    public override string getDescription()
    {
        var regiment = tree.GetRegimentInstance(regimentOffset);
        return string.Format("Улучшает атаку, текущий чистый урон {0}, после исследования {1} ", regiment.damage(0), regiment.damage(1));
    }
}