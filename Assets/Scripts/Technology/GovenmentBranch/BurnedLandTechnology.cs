using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BurnedLand", menuName = "Technology/BurnedLand", order = 1)]
public class BurnedLandTechnology : Technology
{
    public float[] burnedLand;
    public override void LevelUp() => tree.burnedLandEffect = burnedLand[lvl - 1];

    public override string getDescription()
    {
        return string.Format("Тактика выженной земли, вражеские армии получают {0} урона при нахождении на нашей территории.", burnedLand[lvl]);
    }
}
