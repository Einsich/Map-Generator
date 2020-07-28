using System;
using System.Collections.Generic;
using UnityEngine;

public class TechnologyPanel : MonoBehaviour
{
    public TechInterface Era, armyBranch, economyBranch;
    public ListFiller armyTech, economyTech, regimentTech;
    public void ShowTechnology(Technology technology)
    {
        if (technology.owner != Player.curPlayer)
            return;
        Era.Init(technology.EraTech);
        armyBranch.Init(technology.ArmyBranchTech);
        if (!technology.ArmyBranch)
        {
            armyTech.UpdateList(new List<object>());
        }
        else
        {
            armyTech.UpdateList(technology.armyTeches.ConvertAll((x) => (object)x));
        }
        economyBranch.Init(technology.EconomyBranchTech);
        if (!technology.EconomyBranch)
        {
            economyTech.UpdateList(new List<object>());
        }
        else
        {
            economyTech.UpdateList(technology.economyTeches.ConvertAll((x) => (object)x));
        }
        regimentTech.UpdateList(Array.ConvertAll(technology.regiments, (x) => (object)x));
    }
}
