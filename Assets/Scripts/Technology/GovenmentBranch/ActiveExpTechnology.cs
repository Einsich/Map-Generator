using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveExp", menuName = "Technology/ActiveExp", order = 1)]
public class ActiveExpTechnology : Technology
{
    public float[] activeExp;
    public override void LevelUp() => tree.activeExperienceBonus = activeExp[lvl - 1];

    public override string getDescription()
    {
        return string.Format("Увеличивает получаемый опыт процентно на {0}.", activeExp[lvl].ToPercent());
    }
}
