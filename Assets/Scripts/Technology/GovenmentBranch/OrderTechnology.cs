using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Order", menuName = "Technology/Order", order = 1)]
public class OrderTechnology : Technology
{
    public float[] orderIncrease;
    public override void LevelUp() => tree.inquisitionBonus = orderIncrease[lvl - 1];
    public override string getDescription()
    {
        return string.Format("Увеличивает восстановление порядка до {0}.", orderIncrease[lvl]);
    }
}
