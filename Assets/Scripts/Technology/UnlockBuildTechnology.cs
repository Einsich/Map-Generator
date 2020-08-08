using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockBuild", menuName = "Technology/UnlockBuild", order = 1)]
public class UnlockBuildTechnology : Technology
{
    public BuildingType[] unlockType;
    public override void LevelUp() { foreach(var type in unlockType)tree.buildingsMaxLevel[(int)type] = ProvinceData.maxBuildingLevel; }

    public override string getDescription()
    {
        string buildings = "";
        foreach (var b in unlockType)
            buildings += b.ToString() + ", ";
        return string.Format("Позволяет строить {0}.", buildings);
    }
}
