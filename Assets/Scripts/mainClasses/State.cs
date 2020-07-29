﻿using System.Collections;
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
    public TechnologyTree technologyTree;
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
        regiments = new BaseRegiment[8];
        GameObject.Instantiate(PrefabHandler.TechnologyTree).InitializeTree(this);
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
    public BaseRegiment[] regiments;
    public BaseRegiment melee => regiments[1];
    public BaseRegiment ranger => regiments[0];
    public List<Regiment> defaultArmy()
    {
        List<Regiment> list = new List<Regiment>();
        return list;
        int i = regions.Count;
        i = i / 2 + 2;
        int c = i / 10;
        i -= c;
        int r = i / 4;
        i -= c;
        for (int j = 0; j < i; j++)
            list.Add(new Regiment(melee));
        for (int j = 0; j < r; j++)
            list.Add(new Regiment(ranger));
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

        foreach (Army a in army)
            a.ResetTimeAndRecalcUpkeepBonuses();
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
        //SpendTreasure(treasury * penalty, BudgetType.OtherBudget);
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

