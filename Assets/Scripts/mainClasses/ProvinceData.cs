using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvinceData {
    
      Treasury Cost(BuildingType type,int lvl)
    {
        Treasury cost;
        switch(type)
        {
            case BuildingType.Infrastructure:cost = new Treasury(100, 400, 100, 100, 0);break;
            case BuildingType.Port:cost = new Treasury(200, 600, 20, 20, 0);break;
            case BuildingType.Military:cost = new Treasury(1000, 1000, 200, 100, 0);break;
            case BuildingType.Industry:cost = new Treasury(150, 800, 100, 150, 0);break;
            case BuildingType.Trade:cost = new Treasury(300, 100, 100, 100, 0);break;
            case BuildingType.Walls: cost = new Treasury(300, 1000, 200, 200, 0);break;
            case BuildingType.WoodSpesial:cost = new Treasury(100, 500, 200, 100, 0);break;
            case BuildingType.IronSpesial:cost = new Treasury(100, 500, 100, 200, 0); break;
            case BuildingType.FarmSpecial:cost = new Treasury(100, 500, 50, 50, 0);break;
            case BuildingType.UniversitySpecial:cost = new Treasury(500, 200, 100, 100, 100);break;
            default: cost = new Treasury(500) { Science=10};break;
        }

        return cost * levelCoef[1 + lvl] * region.owner.technology.BuildCostBonus;
    }
    static float[] levelCoef = { 0, 1, 1.5f, 1.75f, 2 };
     Treasury Income(BuildingType type, int lvl)
    {
        if (lvl >= levelCoef.Length)
            return new Treasury();
        Treasury income;
        switch (type)
        {
            case BuildingType.Infrastructure: income = new Treasury(20, 0, 0, 0, 0); break;
            case BuildingType.Military: income = new Treasury(0, 100, 0, 0, 0); break;
            case BuildingType.Trade: income = new Treasury(10, 0, 2, 2, 1); break;
            case BuildingType.UniversitySpecial: income = new Treasury(0, 0, 0, 0, 10);break;
            case BuildingType.FarmSpecial: income = new Treasury(50, 0, 0, 0, 0); break;
            case BuildingType.WoodSpesial: income = new Treasury(0, 0, 20, 0, 0); break;
            case BuildingType.IronSpesial: income = new Treasury(0, 0, 0, 20, 0); break;
            default: return new Treasury();
        }

        return income * levelCoef[lvl];
    }
    public int Time(BuildingType type, int lvl)
    {
        return ((int)type) < 6 ? 5 * (1 + lvl) : 10;
    }
    public FractionType fraction;
    public const int buildCount = 6, specialCount = buildCount + 6;
    public int[]buildings;
    public Action[] BuildingAction;
    //public int isBuildInd;
    public float order = 1f;
    public Treasury income = new Treasury(), incomeclear;
    static Treasury defaultTreasure = new Treasury(10, 50, 0, 0, 0);
    public List<RecruitAction> recruitQueue = new List<RecruitAction>();
    public int wallsLevel => buildings[(int)BuildingType.Walls];
    public int portLevel => buildings[(int)BuildingType.Port];
    public bool haveFervie=> buildings[(int)BuildingType.FervieSpesial]>0;
    public System.Action SomeChanges, TreasureChanges;
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
   // public BuildAction action;
    public Region region;
    public ProvinceData(FractionType fraction, Region reg)
    {
        region = reg;
        this.fraction = fraction;
        buildings = new int[specialCount];
        BuildingAction = new Action[specialCount];
        for (int i = 0; i < buildCount; i++)
            buildings[i] = Random.Range(0, 2);
        for (int i = buildCount; i < specialCount; i++)
            buildings[i] = 0;
    }
    static int Type(FractionType fraction)
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
        BuildingType building = (BuildingType)ind;
        int lvl = buildings[ind];
        bool build = BuildingAction[ind] != null;
        if (build)
        {
            region.owner.treasury += Cost(building, lvl);
            BuildingAction[ind].actually = false;
            BuildingAction[ind] = null;
        }
        else
        {
            region.owner.treasury -= Cost(building, lvl);
            BuildingAction[ind] = new Action(Time(building, lvl), () => BuildComplete(ind));
        }
        TreasureChanges?.Invoke();
        SomeChanges?.Invoke();
    }
    public void BuildComplete(int BuildIndex)
    {
        buildings[BuildIndex]++;
        BuildingAction[BuildIndex] = null;
        if (BuildIndex == (int)BuildingType.Infrastructure || BuildIndex == (int)BuildingType.Port || BuildIndex == (int)BuildingType.Walls || BuildIndex >= buildCount)
            region.RebuildTown();
        
        SomeChanges?.Invoke();
        region.owner.IncomeChanges?.Invoke();

    }
    public void RecruitRegiment(RecruitAction recact)
    {
        recruitQueue.Remove(recact);
        garnison.Add(new Regiment(recact.regiment));
        SomeChanges?.Invoke();
        region.owner.IncomeChanges?.Invoke();

    }
    bool CanBuild(BuildingType build)
    {
        int ind = (int)build;
        int lvl = buildings[ind];
        if (lvl > region.owner.technology.BuildLvl[ind])
            return false;
        if (ind < buildCount) 
        {
            if (build == BuildingType.Port && region.Port == null)
                return false;
            if (!(Cost(build, lvl) <= region.owner.treasury))
                return false;
        }
        else
        {
            for (int i = buildCount; i < specialCount; i++)
                if (buildings[i] > 0)
                    return false;
            if (build == BuildingType.FervieSpesial && region.portIdto < 0)
                return false;
            if (build == BuildingType.FarmSpecial && !region.haveGrassland)
                return false;
            if (build == BuildingType.WoodSpesial && !region.haveWood)
                return false;
            if (build == BuildingType.IronSpesial && !region.haveIron)
                return false;
            if (!(Cost(build, lvl) <= region.owner.treasury))
                return false;
        }
        return true;
    }
    public BuildState Stateof(BuildingType build)
    {
        if (BuildingAction[(int)build]!=null)
            return BuildState.isBuilding;

        if ((Player.curPlayer!=null && Player.curPlayer!=region.owner) || !CanBuild(build))
            return BuildState.CantBuild;
        return BuildState.CanBuild;
    }
    public float PortBonus => 1f + 0.15f * buildings[(int)BuildingType.Port];
    public float IndustryBonus => 1f + 0.1f * buildings[(int)BuildingType.Industry];
    public Treasury IndustryVectorBonus => new Treasury(1) + new Treasury(0, 0, 1, 1, 0) * (IndustryBonus - 1);
    public float IncomeCoefFromDistance()
    {
        return 1f / (1f + 0.01f * region.sqrDistanceToCapital);
    }
    public float IncomeCoefFromOrder()
    {
        return order;
    }

    public Treasury CalculateIncome()
    {
        incomeclear = defaultTreasure;
        for (int build = 0; build < specialCount; build++)
            incomeclear += Income((BuildingType)build, buildings[build])* PortBonus * IndustryVectorBonus;

        income = incomeclear * IncomeCoefFromDistance() * IncomeCoefFromOrder() * region.owner.technology.TreasureBonus;
        SomeChanges?.Invoke();
        return income;

    }
    public void EconomyUpdate()
    {
        if (region.ocptby != null)
            return;
        CalculateIncome();
        region.owner.Income += income;

    }
    public List<Regiment> garnison = new List<Regiment>();

    public string BuildingDescription(BuildingType type)
    {
        Treasury cost = Cost(type, buildings[(int)type]);
        Treasury income = Income(type, buildings[(int)type]);
        Treasury incomenext = Income(type, buildings[(int)type] + 1);
        string s = "";
        string d = "";
        switch(type)
        {
            case BuildingType.Infrastructure:s = "Инфраструктура города.\n Развитая инфраструктура позволяет собирать больше налогов.";break;
            case BuildingType.Port: s = "Порт.\n Увеличивает на некоторый процент добычу всех ресурсов, а также позволяет базироваться флоту.";
                d = $"Текуший бонус {PortBonus.ToPercent()}\n"; break;
            case BuildingType.Military: s = "Казармы.\n Увеличивают количество рекрутов, производимое провинцией.";break;
            case BuildingType.Industry: s = "Промышленные постройки.\n Повышают добычу древесины и железа.";d = $"Текуший бонус {IndustryBonus.ToPercent()}\n"; break;
            case BuildingType.Trade: s = "Рынок.\n Усиливает внутреннюю торговлю, из-за чего город получает небольшое количество всех ресурсов."; break;
            case BuildingType.Walls: s = "Стены.\n Главная основа обороны города, дают обороняющейся армии большие преимущества."; break;
            case BuildingType.FarmSpecial: s = "Фермерские плантации. \nЕще больше золота."; break;
            case BuildingType.WeaponSpesial: s = "Оружейня.\n Что-то военное."; break;
            case BuildingType.WoodSpesial: s = "Супер-лесопилка.\n Еще больше дерева."; break;
            case BuildingType.IronSpesial: s = "Супер-шахты.\n Еще больше железа."; break;
            case BuildingType.UniversitySpecial: s = "Университет.\n Еще больше очков науки."; break;
            case BuildingType.FervieSpesial: s = "Ферви.\n Позволяют строить флот."; break;
        }
        return string.Format("{0}\n Стоимость улучшения {1}.\n{4} {2} {3}", s, cost.ToString(), income.isEmpty ? "" : $"Текущий доход от здания {income.ToString()}.\n",
            incomenext.isEmpty?"":$"Доход после улучшения {incomenext.ToString()}.\n", d);

    }
}
public enum BuildingType
{
    Infrastructure,//+к голде
    Port,//увеличивает порядок
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