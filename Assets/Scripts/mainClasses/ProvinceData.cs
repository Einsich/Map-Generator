using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvinceData {
    
      Treasury Cost(Building type,int lvl)
    {
        Treasury cost;
        switch(type)
        {
            case Building.Infrastructure:cost = new Treasury(100, 400, 100, 100, 0);break;
            case Building.Police:cost = new Treasury(200, 600, 20, 20, 0);break;
            case Building.Military:cost = new Treasury(1000, 1000, 200, 100, 0);break;
            case Building.Industry:cost = new Treasury(150, 800, 100, 150, 0);break;
            case Building.Trade:cost = new Treasury(300, 100, 100, 100, 0);break;
            case Building.Walls: cost = new Treasury(300, 3000, 200, 200, 0);break;
            case Building.WoodSpesial:cost = new Treasury(100, 500, 200, 100, 0);break;
            case Building.IronSpesial:cost = new Treasury(100, 500, 100, 200, 0); break;
            case Building.FarmSpecial:cost = new Treasury(100, 500, 50, 50, 0);break;
            case Building.UniversitySpecial:cost = new Treasury(500, 200, 100, 100, 100);break;
            default: cost = new Treasury(500) { Science=10};break;
        }

        return cost * levelCoef[1 + lvl] * region.owner.technology.BuildCostBonus;
    }
    static float[] levelCoef = { 0, 1, 1.5f, 1.75f, 2 };
     Treasury Income(Building type, int lvl)
    {
        Treasury income;
        switch (type)
        {
            case Building.Infrastructure: income = new Treasury(20, 0, 0, 0, 0); break;
            case Building.Military: income = new Treasury(0, 100, 0, 0, 0); break;
            case Building.Trade: income = new Treasury(10, 0, 2, 2, 1); break;
            case Building.UniversitySpecial: income = new Treasury(0, 0, 0, 0, 10);break;
            case Building.FarmSpecial: income = new Treasury(50, 0, 0, 0, 0); break;
            case Building.WoodSpesial: income = new Treasury(0, 0, 20, 0, 0); break;
            case Building.IronSpesial: income = new Treasury(0, 0, 0, 20, 0); break;
            default: income = new Treasury(0); break;
        }

        return income * levelCoef[lvl];
    }
    static int Time(Building type, int lvl)
    {
        return ((int)type) < 6 ? 100 * (1 + lvl) : 600;
    }
    public FractionName fraction;
    public const int buildCount = 6, specialCount = buildCount + 6;
    public int[]buildings;
    public int isBuildInd;
    public float order = 1f;
    public float traderImpact = 1f;
    public Treasury income = new Treasury(), incomeclear;
    static Treasury defaultTreasure = new Treasury(10, 50, 0, 0, 0);
    public List<RecruitAction> recruitQueue = new List<RecruitAction>();
    public int wallsLevel => buildings[(int)Building.Walls];
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
        MenuManager.ShowResources();
        ProvinceMenu.needUpdate = true;
    }
    public void BuildComplete()
    {
        buildings[isBuildInd]++;
        if (isBuildInd == (int)Building.Infrastructure || isBuildInd == (int)Building.Walls || isBuildInd >= buildCount)
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
        if (lvl >= region.owner.technology.BuildLvl[ind])
            return false;
        if (ind < buildCount) 
        {
            if (!(Cost(build, lvl) <= region.owner.treasury))
                return false;
        }
        else
        {
            for (int i = buildCount; i < specialCount; i++)
                if (buildings[i] > 0)
                    return false;
            if (build == Building.FervieSpesial && region.portIdto < 0)
                return false;
            if (build == Building.FarmSpecial && !region.haveGrassland)
                return false;
            if (build == Building.WoodSpesial && !region.haveWood)
                return false;
            if (build == Building.IronSpesial && !region.haveIron)
                return false;
            if (!(Cost(build, lvl) <= region.owner.treasury))
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
    public float IncomeCoefFromDistance()
    {
        return 1f / (1f + 0.01f * region.sqrDistanceToCapital);
    }
    public float IncomeCoefFromOrder()
    {
        return order;
    }

    public void CalculateIncome()
    {
        incomeclear = defaultTreasure;
        for (int build = 0; build < specialCount; build++)
            incomeclear += Income((Building)build, buildings[build]);

        income = incomeclear * IncomeCoefFromDistance() * IncomeCoefFromOrder() * traderImpact * region.owner.technology.TreasureBonus;
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
    Infrastructure,//+к голде
    Police,//увеличивает порядок
    Military,//увеличивает кол-во рекрутов
    Industry,//увеличивает на процент добычу дерева и железа
    Trade,//дает небольшое кол-во всех ресурсов
    Walls,//увеличивает защиту провинции
    FarmSpecial,//если много лугов/пашень, то добавляет к голде и рекрутам
    WeaponSpesial,
    WoodSpesial,//если много леса, то позволяет добывать дерево
    IronSpesial,//если много гор, то позволяет добывать железо
    UniversitySpecial,//дает науку
    FervieSpesial//позволяет строить корабли
}
public enum BuildState
{
    CantBuild,
    CanBuild,
    isBuilding
}