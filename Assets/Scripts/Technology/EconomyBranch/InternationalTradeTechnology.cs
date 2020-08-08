using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InternationalTrade", menuName = "Technology/InternationalTrade", order = 1)]
public class InternationalTradeTechnology : Technology
{
    public float[] PortBonus;
    public override void LevelUp() => tree.portTradeBonus = PortBonus[lvl - 1];
    public override string getDescription()
    {
        return string.Format("Увеличивает бонус от порта на {0}", PortBonus[lvl].ToPercent());
    }
}
