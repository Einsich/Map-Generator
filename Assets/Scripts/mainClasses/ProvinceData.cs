using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ProvinceData {
    
     public Treasury Cost(BuildingType type,int lvl= -1)
    {
        if (lvl == -1)
            lvl = buildings[(int)type];
        if (lvl >= maxBuildingLevel)
            return new Treasury(1000000);
        Treasury cost;
        switch(type)
        {
            case BuildingType.Infrastructure:cost = new Treasury(50, 50, 50, 0, 0);break;
            case BuildingType.Port:cost = new Treasury(50, 100, 20, 20, 0);break;
            case BuildingType.Trade:cost = new Treasury(200, 50, 0, 0, 0);break;
            case BuildingType.Walls: cost = new Treasury(100, 200, 100, 100, 0) * tree.wallsCostReduce;break;
            case BuildingType.Sawmill:cost = new Treasury(100, 100, 20, 20, 0);break;
            case BuildingType.Military:cost = new Treasury(100, 100, 10, 10, 0);break;
            case BuildingType.Pits:cost = new Treasury(100, 100, 20, 20, 0); break;
            case BuildingType.Farms:cost = new Treasury(100, 100, 10, 10, 0);break;
            case BuildingType.University:cost = new Treasury(400, 50, 10, 10, 0);break;
            default: throw new System.Exception("Bad Building type: "+ type.ToString()); break;
        }

        return cost * levelCoef[1 + lvl];
        
    }
    static float[] levelCoef = new float[maxBuildingLevel + 1]{ 0, 1, 1.25f, 1.5f};
     Treasury Income(BuildingType type, int lvl)
    {
        if (lvl >= levelCoef.Length)
            return new Treasury();
        Treasury income;
        switch (type)
        {

            case BuildingType.Infrastructure: income = new Treasury(10, 70, 0, 0, 2); break;
            case BuildingType.Trade: income = new Treasury(20, 0, 5, 5, 2) * (portBonus * tree.portTradeBonus); break;
            case BuildingType.Farms: income = new Treasury(50, 0, 0, 0, 0); break;
            case BuildingType.Military: income = new Treasury(0, 200, 0, 0, 0); break;
            case BuildingType.Sawmill: income = new Treasury(0, 0, 20, 0, 0); break;
            case BuildingType.Pits: income = new Treasury(0, 0, 0, 20, 0); break;
            case BuildingType.University: income = new Treasury(0, 0, 0, 0, 10);break;
            default: return new Treasury();
        }

        return income * levelCoef[lvl];
    }
    public int Time(BuildingType type, int lvl)
    {
        return 5 * (1 + lvl);
    }
    public FractionType fraction;
    public const int buildingsCount = 9, maxBuildingLevel = 3;
    public int[]buildings;
    public GameAction[] BuildingAction;
    //public int isBuildInd;
    public float order = 1f;
    public Treasury income = new Treasury(), incomeclear;
    private Treasury pieceUpkeep;
    static Treasury defaultTreasure = new Treasury(5, 60, 1, 1, 0);
    public List<RecruitAction> recruitQueue = new List<RecruitAction>();
    public int wallsLevel => buildings[(int)BuildingType.Walls];
    public int portLevel => buildings[(int)BuildingType.Port];
    public System.Action SomeChanges;
    public float portBonus => 1 + buildings[(int)BuildingType.Port] * 0.25f;
    public void AddRecruit(RecruitAction act)
    {
        region.owner.SpendTreasure(act.regiment.cost, BudgetType.ArmyBudget);
        recruitQueue.Add(act);
    }
    public void RemoveRecruit(RecruitAction act)
    {
        region.owner.IncomeTreasure(act.regiment.cost * 0.5f, BudgetType.ArmyBudget);
        recruitQueue.Remove(act);
    }
   // public BuildAction action;
    public Region region;
    public TechnologyTree tree => region.owner.technologyTree;
    public ProvinceData(FractionType fraction, Region reg)
    {
        region = reg;
        this.fraction = fraction;
        buildings = new int[buildingsCount];
        BuildingAction = new GameAction[buildingsCount];
        buildings[(int)BuildingType.Infrastructure] = Random.Range(0, 2);
        buildings[(int)BuildingType.Trade] = Random.Range(0, 2);
        buildings[(int)BuildingType.Walls] = Random.Range(0, 2);
        if (reg.owner.melee)
            for (int i = 0, n = 1 + wallsLevel * (1 + Random.Range(0, 2)); i < n; i++)
                garnison.Add(new Regiment(reg.owner.melee));
        if (reg.owner.ranger)
            for (int i = 0, n = 1 + wallsLevel * (1 + Random.Range(0, 2)); i < n; i++)
                garnison.Add(new Regiment(reg.owner.ranger));
    }
    static int Type(FractionType fraction)
    {
        return (int)fraction;
    }

    private PriorityQueue<(float, BuildingType)> buildPriorityQueue = new PriorityQueue<(float, BuildingType)>();
    private int updateQueue;

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
            region.owner.IncomeTreasure(Cost(building, lvl), BudgetType.BuildingBudget);
            BuildingAction[ind].actually = false;
            BuildingAction[ind] = null;
        }
        else
        {
            region.owner.SpendTreasure(Cost(building, lvl), BudgetType.BuildingBudget);
            BuildingAction[ind] = new GameAction(Time(building, lvl), () => BuildComplete(ind));
        }
        SomeChanges?.Invoke();
    }
    public void ForwardBuild(BuildingType building)
    {
        int ind = (int)building;
        int lvl = buildings[ind];
        BuildingAction[ind] = new GameAction(Time(building, lvl), () => BuildComplete(ind));
    }
    public void BuildComplete(int BuildIndex)
    {
        buildings[BuildIndex]++;
        BuildingAction[BuildIndex] = null;
        if (BuildIndex == (int)BuildingType.Infrastructure || BuildIndex == (int)BuildingType.Port || BuildIndex == (int)BuildingType.Walls)
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
    public bool CanPhysicalyBuild(BuildingType build)
    {
        int ind = (int)build;
        int lvl = buildings[ind];
        if (lvl >= tree.buildingsMaxLevel[ind])
            return false;

        if (BuildingAction[ind] != null)
            return false;
        if (build == BuildingType.Port && region.portIdto < 0)
            return false;
        if (build == BuildingType.Farms && !region.haveGrassland)
            return false;
        if (build == BuildingType.Sawmill && !region.haveWood)
            return false;
        if (build == BuildingType.Pits && !region.haveIron)
            return false;

        return true;
    }
    bool CanBuild(BuildingType build)
    {
        int lvl = buildings[(int)build];
        return CanPhysicalyBuild(build) && (Cost(build, lvl) <= region.owner.treasury);
        
    }
    public BuildState Stateof(BuildingType build)
    {
        if (BuildingAction[(int)build]!=null)
            return BuildState.isBuilding;

        if (!CanBuild(build))
            return BuildState.CantBuild;
        return BuildState.CanBuild;
    }
    public float IncomeCoefFromDistance()
    {
        return 1f / (1f + 0.01f * region.DistanceToCapital);
    }
    public float IncomeCoefFromOrder()
    {
        return order;
    }

    public Treasury CalculateIncome()
    {
        incomeclear = defaultTreasure;
        for (int build = 0; build < buildingsCount; build++)
            incomeclear += Income((BuildingType)build, buildings[build]) * tree.treasureIncrease;

        income = incomeclear * IncomeCoefFromDistance() * IncomeCoefFromOrder();// * region.owner.technology.TreasureBonus;
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

    public Treasury GetUpkeep()
    {
        Treasury upkeep = new Treasury(0);

        foreach (var g in garnison)
        {
            upkeep += g.baseRegiment.upkeep;
        }
        return upkeep * tree.regimentUpkeepReduce;
    }


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
            case BuildingType.Port: s = "Порт.\n Увеличивает на некоторый процент доход от рынка за счет морской торговли, а также позволяет базироваться флоту. Позволяют строить флот.";
                d = $"Текуший бонус {portBonus.ToPercent()}\n"; break;
            case BuildingType.Trade: s = "Рынок.\n Усиливает внутреннюю торговлю, из-за чего город получает небольшое количество всех ресурсов."; break;
            case BuildingType.Walls: s = "Стены.\n Главная основа обороны города, дают обороняющейся армии большие преимущества."; break;
            case BuildingType.Farms: s = "Фермерские плантации. \nЕще больше золота."; break;
            case BuildingType.Military: s = "Казармы.\n Увеличивают количество рекрутов, производимое провинцией.";break;
            case BuildingType.Sawmill: s = "Лесопилка.\n Добывают дерево."; break;
            case BuildingType.Pits: s = "Шахты.\n Добывают железо."; break;
            case BuildingType.University: s = "Университет.\n Увеличивает очки науки."; break;
        }
        return string.Format("{0}\n Стоимость улучшения {1}.\n{4} {2} {3}", s, cost.ToString(), income.isEmpty ? "" : $"Текущий доход от здания {income.ToString()}.\n",
            incomenext.isEmpty?"":$"Доход после улучшения {incomenext.ToString()}.\n", d);

    }

    public (float, BuildingType?) GetBestBuilding()
    {
        
        if (updateQueue-- == 0)
        {
            UpdateBuildQueueAndResetCounter();
        }

        if (buildPriorityQueue.Count > 0)
        {
            var prioryBuild = buildPriorityQueue.Dequeue();
            return prioryBuild;
        }
        else
        {
            return (0f, null);
        }
    }

    private void UpdateBuildQueueAndResetCounter()
    {
        updateQueue = 4;
        buildPriorityQueue.Clear();
        for (BuildingType type = BuildingType.Infrastructure; type != BuildingType.EndEnum; type++)
        {
            if (CanPhysicalyBuild(type))
            {
                float profit = Profitability(type);
                if (profit != 0f)
                {
                    buildPriorityQueue.Enqueue((-profit, type));
                }
            }
        }
    }
    private float Profitability(BuildingType type)
    {
        Treasury old = CalculateIncome();
        buildings[(int)type]++;
        Treasury now = CalculateIncome();
        buildings[(int)type]--;
        now -= old;
        now.NormalizeToGold();
        return now.Sum;
    }
}
public enum BuildingType
{
    Infrastructure,//+к голде
    Trade,//дает небольшое кол-во всех ресурсов
    Port,//увеличивает порядок
    Walls,//увеличивает защиту провинции
    Farms,//если много лугов/пашень, то добавляет к голде 
    Military,//увеличивает кол-во рекрутов
    Sawmill,//если много леса, то позволяет добывать дерево
    Pits,//если много гор, то позволяет добывать железо
    University,//дает науку
    EndEnum
}
public enum BuildState
{
    CantBuild,
    CanBuild,
    isBuilding
}