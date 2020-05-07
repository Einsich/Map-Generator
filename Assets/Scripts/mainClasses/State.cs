using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class State
{
    public int ID;
    public string name;
    public FractionType fraction;
    public List<Region> regions;
    public Texture2D flag;
    Sprite flagS = null;
    public Sprite flagSprite => flagS ? flagS : flagS = Sprite.Create(flag, new Rect(0, 0, flag.width, flag.height), new Vector2(0.5f, 0.5f));

    public List<Army> army;
    public List<Ship> ships;
    public List<Person> persons;
    public Color mainColor;
    public Diplomacy diplomacy;
    public Technology technology;
    public StateAI stateAI;
    public Treasury Income;
    public Treasury allRegimentsUpkeep;
    public System.Action TreasureChange, IncomeChanges;
    public Treasury treasury => stateAI.GetTreasure;
    public void SpendTreasure(Treasury treasury, BudgetType budgetType) {stateAI.SomeOneSpentResources(treasury, budgetType);TreasureChange?.Invoke(); }
    public void IncomeTreasure(Treasury treasury, BudgetType budgetType) { stateAI.IncomeResources(treasury, budgetType); TreasureChange?.Invoke(); }
    
    public Region Capital;
    public GameObject Text;
    public State() {
        regions = new List<Region>();
        army = new List<Army>();
        persons = new List<Person>();
        ships = new List<Ship>();
        regiments =
        new List<BaseRegiment>() { new BaseRegiment(this,RegimentName.SimpleMelee, RegimentType.Infantry, DamageType.Melee, 3, 1, 0, 0, 1,1000, new Treasury(100,1000,10,10,0), new Treasury(10, 0, 0, 0, 0),1),
        new BaseRegiment(this,RegimentName.SimpleRanger, RegimentType.Infantry, DamageType.Range, 0, 0, 1, 0, 0,1000, new Treasury(400,1000,10,10,0), new Treasury(10, 0, 0, 0, 0),1.5f),
        new BaseRegiment(this,RegimentName.SimpleCavalry, RegimentType.Cavalry, DamageType.Charge, 1, 3, 0, 0, 4,1000, new Treasury(400,1000,10,10,0), new Treasury(20, 0, 0, 0, 0),2),
        new BaseRegiment(this,RegimentName.SimpleArta, RegimentType.Artillery, DamageType.Siege, 0, 0, 0, 0, 6,1000, new Treasury(500,1000,50,50,10), new Treasury(40, 0, 0, 0, 0),4) };
        
        technology = new Technology(this, 0);
        stateAI = new StateAI(this);
        stateAI.IncomeResources(new Treasury(10000));
        GameTimer.AddListener(StateDecaSecondUpdate,this);
    }
    public void SetName()
    {
        Vector2Int left, center, right;
        List<Vector2Int> ter = new List<Vector2Int>();
        for (int i = 0; i < regions.Count; i++)
            ter.Add(regions[i].Capital);
        MapMetrics.CalculLCR(ter, out left, out center, out right);

        Vector3 l = MapMetrics.GetCellPosition(left, true);
        Vector3 c = MapMetrics.GetCellPosition(center, true);
        Vector3 r = MapMetrics.GetCellPosition(right, true);

        l -= c; r -= c;
        l = new Vector3(l.x, l.z, l.y);
        r = new Vector3(r.x, r.z, r.y);


        left -= center;
        right -= center;
        float maxd = left.magnitude + right.magnitude;
        float maxS = maxd * 2.4f / name.Length;
        int S = Mathf.RoundToInt(regions.Count * 0.6f);
        if (maxS < 1 || S < 1)
        {
            Text.SetActive(false);
            return;
        }
        int fontSize = (int)Mathf.Clamp((4 + Mathf.Clamp(S, 0, 10)), 3, maxS);
        Text.transform.position = c;
        Text.transform.SetParent(Main.instance.Names);

        Text.GetComponent<CurvedText>().SetProperies(name, l, r, fontSize);
    }
    public void SetNameStatus(bool hid)
    {
        Text.SetActive(!hid);
        bool nonDiscover = true;
        for (int i = 0; i < regions.Count; i++)
            if (!regions[i].HiddenFrom(Player.curPlayer))
            {
                nonDiscover = false;
            }
        if (nonDiscover)
            Text.SetActive(false);
        

    }
    public void RecalculArmyPath()
    {
        foreach (Army a in army)
            a.navAgent.RecalculatePath();
    }
    public List<BaseRegiment> regiments;
    public BaseRegiment melee => regiments[0];
    public BaseRegiment ranger => regiments[1];
    public BaseRegiment cavalry => regiments[2];
    public BaseRegiment artillery => regiments[3];
    public List<Regiment> defaultArmy()
    {
        List<Regiment> list = new List<Regiment>();
        int i = regions.Count;
        int a = i / 10;
        i -= a;
        int c = i / 4;
        i -= c;
        for (int j = 0; j < i; j++)
            list.Add(new Regiment(ranger));
        for (int j = 0; j < c; j++)
            list.Add(new Regiment(cavalry));
        for (int j = 0; j < a; j++)
            list.Add(new Regiment(artillery));
        return list;
    }

    public Person defaultLeader()
    {
        switch(Random.Range(0,(int)PersonType.Count))
        {
            case 0: return new Warrior(this);
            case 1: return new Knight(this);
            case 2: return new Jaeger(this);
            case 3: return new Archer(this);
           // case 4: return new Wizard(this);
           // case 5: return new Engineer(this);
            case 6: return new Necromancer(this);
            case 7: return new DeathKnight(this);
            default:return new Knight(this);
        }
    }
    public void AddAllPerson()
    {
        new Warrior(this);
        new Knight(this);
        new Jaeger(this);
        new Archer(this);
       // new Wizard(this);
       // new Engineer(this);
        new Necromancer(this);
        new DeathKnight(this);
    }
    public void CalculateIncome()
    {
        stateAI.IncomeResources(Income);
        TreasureChange?.Invoke();
    }
    public void StateDecaSecondUpdate()
    {
        Income = new Treasury();
        for (int i = 0; i < regions.Count; i++)
            regions[i].MonthUpdate();
        GlobalTrade.AddIncome(Income);
        allRegimentsUpkeep = CountingUpkeep();
        SpendTreasure(allRegimentsUpkeep, BudgetType.ArmyBudget);
        stateAI.autoTrader.DealsUpdate();
        CalculateIncome();
        diplomacy.DiplomacyUpdate();
    }

    private Treasury CountingUpkeep()
    {
        var total = Treasury.zero;

        foreach (Army a in army)
            total += a.GetUpkeep();
        foreach (Region r in regions)
            total += r.data.GetUpkeep();

        return total;
    }

    public void DeclareWarPenalty(float penalty)
    {
        penalty = 1 - penalty;
        SpendTreasure(treasury * penalty, BudgetType.OtherBudget);
    }
    public bool WantTrade(ResourcesType sellType, float wesell, ResourcesType buyType, float webuy)
    {
        return !stateAI.autoTrader.DealsOverflow && Income[sellType] * GlobalTrade.MaxSellPercent >= wesell && GlobalTrade.WantToBuy(this, buyType) && !GlobalTrade.WantToBuy(this, sellType) ;
        return Mathf.Min(GlobalTrade.GetCource(ResourcesType.Gold, sellType) * treasury[sellType],
             GlobalTrade.GetCource(ResourcesType.Gold, buyType) * treasury[buyType]) <
          Mathf.Min(GlobalTrade.GetCource(ResourcesType.Gold, sellType) * (treasury[sellType] - wesell),
             GlobalTrade.GetCource(ResourcesType.Gold, buyType) * (treasury[buyType] + webuy));

    }
    public string ResourcesHelp(int t)
    {
        return string.Format("{0}. Ваш прирост {1}", Treasury.ToString(t),Income[t].ToString("N2"));
    }
}

