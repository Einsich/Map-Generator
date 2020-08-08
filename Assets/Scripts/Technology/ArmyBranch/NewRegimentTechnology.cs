using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewRegimentTechnology : Technology
{
    public int regimentOffset;
    public BaseRegiment regimentInstance;
    public override void LevelUp() => tree.state.regiments[regimentOffset] = Instantiate(regimentInstance);
}

