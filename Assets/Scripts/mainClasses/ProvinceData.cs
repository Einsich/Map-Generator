using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvinceData {
    
     static Treasury Cost(Building type,int lvl)
    {
        Treasury cost;
        switch(type)
        {
            case Building.Infrastructure:cost = new Treasury(100, 400, 100, 100, 0);break;
            case Building.Agricultural:cost = new Treasury(50, 2000, 20, 20, 0);break;
            case Building.Military:cost = new Treasury(1050, 1000, 200, 100, 0);break;
            case Building.Industry:cost = new Treasury(150, 2000, 100, 150, 0);break;
            case Building.Trade:cost = new Treasury(300, 100, 20, 200, 0);break;
            case Building.Towers:cost = new Treasury(300, 3000, 1000, 500, 0);break;
            default: cost = new Treasury(500) { Science=10};break;
        }

        return cost * (1 + lvl);
    }
    static Treasury Income(Building type, int lvl)
    {
        Treasury income;
        switch (type)
        {
            case Building.Infrastructure: income = new Treasury(10, 0, 0, 0, 0); break;
            case Building.Agricultural: income = new Treasury(10, 0, 0, 0, 0); break;
            case Building.Military: income = new Treasury(0, 100, 0, 0, 0); break;
            case Building.Industry: income = new Treasury(0, 0, 6, 6, 0); break;
            case Building.Trade: income = new Treasury(10, 0, 3, 3, 1); break;
            case Building.Towers:income = new Treasury(0);break;
            default: income = new Treasury(0) { Science = 10 }; break;
        }

        return income * lvl;
    }
    static int Time(Building type, int lvl)
    {
        int t = 600;
        switch (type)
        {
            case Building.Infrastructure:
            case Building.Agricultural: 
            case Building.Military:
            case Building.Industry: 
            case Building.Trade:
            case Building.Towers: t = 200; break;
        }
        return t;
    }
    public FractionName fraction;
    public const int maxLvl = 4, buildCount = 6, specialCount = buildCount + 6;
    public int[]buildings;
    public int isBuildInd;

    public Treasury income = new Treasury();
    static Treasury defaultTreasure = new Treasury(10, 50, 0, 0, 0);
    public List<RecruitAction> recruitQueue = new List<RecruitAction>();
    public void AddRecruit(RecruitAction act)
    {
        region.owner.treasury -= act.regiment.cost;
        recruitQueue.Add(act);
    }
    public void RemoveRecruit(RecruitAction act)
    {
        region.owner.treasury += act.regiment.cost * 0.5f;
        recruitQueue.Remove(act);
    }
    public BuildAction action;
    public Region region;
    public ProvinceData(FractionName fraction, Region reg)
    {
        region = reg;
        isBuildInd = -1;
        this.fraction = fraction;
        buildings = new int[specialCount];
        for (int i = 0; i < buildCount; i++)
            buildings[i] = Random.Range(0, 2);
        for (int i = buildCount; i < specialCount; i++)
            buildings[i] = 0;
    }
    static int Type(FractionName fraction)
    {
        return (int)fraction;
    }
    public void Save(BinaryWriter writer)
    {

    }
    public void Load(BinaryReader reader)
    {

    }
    public void ClickBuilding(int ind)
    {
        Building build = (Building)ind;
        int lvl = buildings[ind];
        if (isBuildInd<0)
        {
            isBuildInd = ind;
            region.owner.treasury -= Cost(build,lvl);
            action = new BuildAction(this, Time(build, lvl));
        }
        else
        {
            region.owner.treasury += Cost(build, lvl);
            isBuildInd = -1;
            if (action != null)
                action.actually = false;
            action = null;
        }
        if (Player.curPlayer == null || Player.curPlayer == region.owner) 
        MainStatistic.instance.ShowResources(region.owner);
        ProvinceMenu.needUpdate = true;
    }
    public void BuildComplete()
    {
        buildings[isBuildInd]++;
        if (isBuildInd == 0 || isBuildInd >= buildCount)
            region.RebuildTown();
        
        isBuildInd = -1;
        action = null;
        if (ProvinceMenu.current == region) 
        ProvinceMenu.needUpdate = true;
    }
    public void RecruitRegiment(RecruitAction recact)
    {
        recruitQueue.Remove(recact);
        garnison.Add(new Regiment(recact.regiment));
        if (Player.curRegion == region)
            ProvinceMenu.needUpdate = true;
    }
    bool CanBuild(Building build)
    {
        int ind = (int)build;
        int lvl = buildings[ind];
        if (ind < buildCount) 
        {
            if (lvl == maxLvl ||  !(Cost(build, lvl)<= region.owner.treasury))
                return false;
        }
        else
        {
            if (build == Building.FervieSpesial && region.portIdto < 0)
                return false;
            int sum = 0;
            for (int i = buildCount; i < specialCount; i++)
                sum |= buildings[i];
            if (sum > 0 || lvl == 1 || !(Cost(build, lvl) <= region.owner.treasury))
                return false;
        }
        return true;
    }
    public BuildState Stateof(Building build)
    {
        if ((int)build == isBuildInd)
            return BuildState.isBuilding;

        if ((Player.curPlayer!=null && Player.curPlayer!=region.owner) || !CanBuild(build) || (isBuildInd >= 0))
            return BuildState.CantBuild;
        return BuildState.CanBuild;
    }

    public void CalculateIncome()
    {
        income = defaultTreasure;
        for(int build = 0;build<specialCount;build++)
            income += Income((Building)build, buildings[build]);
      
    }
    public void EconomyUpdate()
    {
        if (region.ocptby != null)
            return;
        CalculateIncome();
        region.owner.treasury += income;

    }
    public List<Regiment> garnison = new List<Regiment>();


}
public enum Building
{
    Infrastructure,
    Agricultural,
    Military,
    Industry,
    Trade,
    Towers,
    FarmSpecial,
    WeaponSpesial,
    IndustrySpesial,
    MagistratSpesial,
    UniversitySpecial,
    FervieSpesial
}
public enum BuildState
{
    CantBuild,
    CanBuild,
    isBuilding
}