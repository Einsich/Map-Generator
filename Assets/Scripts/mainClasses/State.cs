using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class State
{
    public string name;
    public FractionName fraction;
    public List<Diplomacy> diplomacy;
    public List<Region> regions;
    public Texture2D flag;
    Sprite flagS = null;
    public Sprite flagSprite => flagS ? flagS : Sprite.Create(flag, new Rect(0, 0, flag.width, flag.height), new Vector2(0.5f, 0.5f));        
    
    public List<Army> army;
    public List<Person> persons;
    public Color mainColor;
    Treasury treasury_ = new Treasury();
    public Treasury treasury { get => treasury_; set { treasury_ = value; if (this == Player.curPlayer) MenuManager.ShowResources(this); } }
    public Region Capital;
    public GameObject Text;
    public State() { regions = new List<Region>(); army = new List<Army>(); diplomacy = new List<Diplomacy>();persons = new List<Person>();
        treasury_.Gold = 100; treasury_.Manpower = 1000; }
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
    public Diplomacy GetDiplomacyWith(State other)
    {
        if (other == this)
            return null;
        foreach (Diplomacy d in diplomacy)
            if (d.s1 == other || d.s2 == other)
                return d;
        return null;
    }
    public void RecalculArmyPath()
    {
        foreach (Army a in army)
            a.navAgent.RecalculatePath();
    }
    public float ShockPower(RegimentType type)
    {
        float r = 0;
        switch (type)
        {
            case RegimentType.Infantry: r = 0.3f; break;
            case RegimentType.Cavalry: r = 0.8f; break;
            case RegimentType.Artillery: r = 0f; break;
        }
        return r;
    }
    public float FirePower(RegimentType type)
    {
        float r = 0;
        switch (type)
        {
            case RegimentType.Infantry: r = 0.35f; break;
            case RegimentType.Cavalry: r = 0f; break;
            case RegimentType.Artillery: r = 1f; break;
        }
        return r;
    }
    public List<BaseRegiment> regiments =
        new List<BaseRegiment>() { new BaseRegiment() { pips = new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 } },type  = RegimentType.Infantry,cost = new Treasury(100,1000,10,10,0),
            name = RegimentName.Пехота,time = 30 },
            new BaseRegiment() { pips = new int[,] { { 0, 0 }, { 1, 1 }, { 1, 0 } }, type = RegimentType.Cavalry,cost = new Treasury(400,1000,10,10,0),
                name = RegimentName.Кавалерия,time = 50 },
        new BaseRegiment() { pips = new int[,] {{ 1, 1 }, { 0, 1 }, { 1, 0 }},type  = RegimentType.Artillery,cost = new Treasury(500,1000,50,50,10),
            name = RegimentName.Пушки,time = 80 }};
    public BaseRegiment infantry => regiments[0];
    public BaseRegiment cavalry => regiments[1];
    public BaseRegiment artillery => regiments[2];
    public List<Regiment> defaultArmy()
    {
        List<Regiment> list = new List<Regiment>();
        int i = regions.Count;
        int a = i / 10;
        i -= a;
        int c = i / 4;
        i -= c;
        for (int j = 0; j < i; j++)
            list.Add(new Regiment(infantry));
        for (int j = 0; j < c; j++)
            list.Add(new Regiment(cavalry));
        for (int j = 0; j < a; j++)
            list.Add(new Regiment(artillery));
        return list;
    }
    public Person defaultLeader()
    {
        switch(Random.Range(0,2))
        {
            default:return new Knight();
            case 1:return new Trader();
        }
    }
}

