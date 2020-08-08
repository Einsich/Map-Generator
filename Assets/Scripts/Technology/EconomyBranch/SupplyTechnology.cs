using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Supply", menuName = "Technology/Supply", order = 1)]
public class SupplyTechnology : Technology
{
    public float[] recruitInTownBonus;
    public float[] recruitInCountryBonus;
    public override void LevelUp() => (tree.recruitInTown, tree.recruitInCountry) = (GameConst.RecruitInTown + recruitInTownBonus[lvl - 1], GameConst.RecruitInCountry + recruitInCountryBonus[lvl - 1]);
    public override string getDescription()
    {
        (float a, float b) = (GameConst.RecruitInTown + recruitInTownBonus[lvl], GameConst.RecruitInCountry + recruitInCountryBonus[lvl]);
        return string.Format("Увеличивает восстановление армий до {0} в городах и до {1} вне.", a, b);
    }
}
