using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "MoveSpeedBonusTech", menuName = "Technology/MoveSpeedBonusTech", order = 1)]
public class MoveSpeedBonusTechnology : Technology
{
    public float moveSpeedBonus;
    public override void LevelUp() => tree.moveSpeedBonus = moveSpeedBonus;
    public override string getDescription()
    {
        return string.Format("Увеличивает скорость передвижения всех ваших армий на {0}", moveSpeedBonus.ToPercent());
    }
}