
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Region :ITarget, IFightable
{
    public static Material[] borderMaterial;
    public string name;
    State _owner;
    public Transform flag;
    MeshRenderer flagrenderer;
    MaterialPropertyBlock block = new MaterialPropertyBlock();
    public State owner { get => _owner;set {
            _owner = value;
            flag?.gameObject.SetActive(value != null);
            if (value != null && flag!=null)
            {
                flagrenderer.GetPropertyBlock(block);
                block.SetTexture("_MainTex", _owner.flag);
                flagrenderer.SetPropertyBlock(block);
            }
        } }
    public SiegeAction siegeAction;
    public State ocptby;
    public State curOwner => ocptby == null ? owner : ocptby;
    public Army siegeby;
    public int id, portIdto = -1, Continent;
    public Region[] neib;
    public bool haveWood, haveIron, haveGrassland;
    public List<Vector2Int> territory;
    public bool iswater,retreatused;
    public Vector2Int Capital;
    public float sqrDistanceToCapital => (Capital - _owner.Capital.Capital).sqrMagnitude;
    public Vector2 pos;
    public GameObject Text,Town,Port,Corona;
    public ProvinceData data;
    BorderState bordState;
    bool selected;
    public float LastAttack { get; set; } = 0;
    public float AttackPeriod { get; set; } = 1;
    public bool Destoyed { get; set; } = false;
    public Vector2 position => pos;
    public Vector2Int curPosition => Capital;

    public bool HiddenBord
    {
        get { return bordState == BorderState.HiddenBorder; }
        set
        {
            bordState = BorderState.HiddenBorder;
            UpdateBorder();
        }
    }
    public bool StateBorder
    {
        get { return bordState == BorderState.ShowStateBorder; }
        set
        {
            if (value)
                bordState = BorderState.ShowStateBorder;
            else
                bordState = BorderState.ShowBorder;
            UpdateBorder();
        }
    }
    public bool Selected
    {
        get { return selected; }
        set
        {
            Main.instance.terrainMat.SetColor("_Select", value ? MapMetrics.GetRegionColor(this) : Color.white);
            Main.instance.terrainMat.SetFloat("_SelectTime", Time.time);
            selected = value;
            
        }
    }

    public float AttackRange { get; set; } = 2f;

    public Region()
    {
        territory = new List<Vector2Int>();
    }
    public int neibIndex(Region reg)
    {
        for (int i = 0; i < neib.Length; i++)
            if (reg == neib[i])
                return i;
        return -1;
    }
    public void UpdateBorder()
    {
        //  Text.SetActive(!hiddenbord&&!HiddenFrom(Player.curPlayer));
        if (!iswater)
            Town.SetActive(!HiddenFrom(Player.curPlayer) && bordState != BorderState.HiddenBorder && bordState != BorderState.ShowStateBorder);
        if (Corona) Corona.SetActive(Town.activeSelf);
        Text.SetActive(false);
        for (int i=0;i<neib.Length;i++)
        {
            bool hidfrompl = neib[i].HiddenFrom(Player.curPlayer) && HiddenFrom(Player.curPlayer);
            if (portIdto == neib[i].id)
            {
                if (Port == null)
                {
                    Debug.Log("! " + owner.name + " " + id + " " + neib[i].id);
                }
                else
                    Port.SetActive(bordState != BorderState.ShowStateBorder && !hidfrompl);
            }
        }
    }
    public void SetName()
    {
        Vector2Int left, center, right;
        MapMetrics.CalculLCR(territory, out left, out center, out right);

        Vector3 l = MapMetrics.GetCellPosition(left,  true);
        Vector3 c = MapMetrics.GetCellPosition(center, true);
        Vector3 r = MapMetrics.GetCellPosition(right,  true);
        l -= c; r -= c;
        l = new Vector3(l.x, l.z, l.y);
        r = new Vector3(r.x, r.z, r.y);
        left -= center;
        right -= center;
        float maxd = left.magnitude + right.magnitude;
        int dS = (int)Mathf.Clamp((territory.Count * 0.025f), 0, 5);
        int fontSize = (int)Mathf.Min(1 + dS, Mathf.Max(1, maxd / (name.Length+5)));
        Text.transform.position = c;
        Text.transform.SetParent(Main.instance.Names);
        Text.GetComponent<CurvedText>().SetProperies(name+"("+id+")", l, r, fontSize);
        Text.GetComponent<Outline>().effectDistance = new Vector2(0.03f, -0.03f);
    }
    public void UpdateSplateState(State curPlayer)
    {
        if(curPlayer==null)
            MapMetrics.SetRegionState(territory, LandShowMode.Visible);
        else
        if (HiddenFrom(curPlayer))
            MapMetrics.SetRegionState(territory, LandShowMode.TerraIncognito);
        else
        if(InFogFrom(curPlayer))
            MapMetrics.SetRegionState(territory, LandShowMode.ForOfWar);
        else
        MapMetrics.SetRegionState(territory, LandShowMode.Visible);

    }

    public bool HiddenFrom(State state)
    {
        if (state == null) return false;
        return Vector2.Distance(Capital, state.Capital.Capital) > 75;
    }
    public bool InFogFrom(State state)
    {
        if (state == null) return false;
        if (!(NotNeib(state) && ocptby != state && owner != state))
            return false;
        foreach (Army army in state.army)
        {
            if (army.curReg == this)
                return false;
            foreach (Region neib in neib)
                if (army.curReg == neib)
                    return false;
        }
        return true;

    }
    public bool NotNeib(State st)
    {

        foreach (Region n in neib)
            if (n.owner == st) return false;
        return true;
    }
    public void MonthUpdate()
    {
        if (iswater) return;
        data.EconomyUpdate();
        CalculateUpkeep();
        
    }
    public void RebuildTown(bool newfraction = false)
    {
        int lvl = data.buildings[0];
        Transform town = Town.transform.GetChild(0);

        foreach (Transform go in town)
            GameObject.Destroy(go.gameObject);
        int ind = -1;
        for (int i = ProvinceData.buildCount; i < ProvinceData.specialCount; i++)
            if (data.buildings[i] != 0)
                ind = i;
        ind -= ProvinceData.buildCount;
        if (ind >= 0)
        {
            GameObject t1 = GameObject.Instantiate(ProvinceMenu.GetSpecialBuilding(ind), town);
            t1.transform.localPosition = Vector3.right * 1.4f;
        }
        GameObject t0 = GameObject.Instantiate(Fraction.TownPrefab[(int)data.fraction].transform.GetChild(lvl).gameObject, town);
        t0.transform.localPosition = Vector3.zero;
        if (data.wallsLevel > 0)
        {
            t0 = GameObject.Instantiate(Fraction.WallsPrefab[(int)data.fraction], town);
            t0.transform.localPosition = Vector3.zero;
        }
        flagrenderer = flag.GetChild(1).GetComponent<MeshRenderer>();
        owner = _owner;//для настройки флага
    }
    bool neibCellof(Vector2Int p, int neib)
    {
        Vector2Int t;
        foreach(var d in MapMetrics.Qd)
        {
            t = p + d;
            if (MapMetrics.InsideMap(t.y, t.x))
                if (MapMetrics.GetRegion(t).id == neib)
                    return true;
        }
        return false;
    }
    public Vector2Int GetPortPosition()
    {
        List<Vector2Int> list = ListPool<Vector2Int>.Get();

        foreach (var p in territory)
            if (neibCellof(p, portIdto))
                list.Add(p);
        Vector2Int port= list[list.Count/2];
        ListPool<Vector2Int>.Add(list);
        
        if (MapMetrics.GetRegion(port + Vector2Int.right)?.id == portIdto)
            return port + Vector2Int.right;
        if (MapMetrics.GetRegion(port + Vector2Int.up)?.id == portIdto)
            return port + Vector2Int.up;
        return port;

    }
    public void CalculateTerreinTypes()
    {
        int wood = 0, iron = 0, grass = 0, n = territory.Count;
        foreach (var v in territory)
        {
            TerrainType type = (TerrainType)Main.terrainIndexes[MapMetrics.Index(v.y, v.x)];
            switch(type)
            {
                case TerrainType.ForestLeaf:
                case TerrainType.ForestSpire:wood++;break;
                case TerrainType.Mountain:
                case TerrainType.MountainDesert:
                case TerrainType.MountainSnow:
                case TerrainType.MountainVerySnow:iron++;break;
                case TerrainType.Grass:
                case TerrainType.Steppe:
                case TerrainType.HalfSteppe:grass++;break;
                default:break;
            }
        }
        haveWood = wood * 10 > n;
        haveIron = iron * 10 > n;
        haveGrassland = grass * 5 > n;
    }
    public void CalculateUpkeep()
    {
        Treasury upkeep = new Treasury(0);
        foreach (var g in data.garnison)
            upkeep += g.baseRegiment.upkeep;
        _owner.treasury -= upkeep * GameConst.GarnisonUpkeepSale;
    }
    public void WasCatch(Army catcher)
    {
        if (siegeby == catcher)
            return;
        if (!curOwner.diplomacy.canAttack(catcher.owner.diplomacy))
            return;
        siegeby = catcher;
        siegeby.navAgent.Stop();
        siegeby.besiege = this;
        siegeAction = new SiegeAction(this, 30);
        Debug.Log("Началась осада " + name);
        siegeby.siegeModel.SetActive(true);
        siegeby.siegeModel.transform.position = MapMetrics.GetCellPosition(Capital);
    }
    public void SiegeEnd()
    {
        
        siegeby.besiege = null;
        siegeby.siegeModel.SetActive(false);
        siegeby = null;
        if(siegeAction!=null)
        siegeAction.actually = false;
    }
    public void WinSiege()
    {
        foreach (Army insider in curOwner.army)
            if (insider.inTown && insider.curReg == this)
                insider.DestroyArmy();
        curOwner.army.RemoveAll(a => a.Destoyed);
        Player.instance.Occupated(this, siegeby.owner);
        SiegeEnd();
    }
    public bool isBusy()
    {
        foreach (Army insider in curOwner.army)
            if (insider.inTown && insider.curReg == this)
                return true;
        return false;
    }

    public DamageInfo GetDamage(DamageType damageType)
    {
        DamageInfo info = new DamageInfo();
        foreach (Regiment regiment in data.garnison)
        {
            if (damageType <= regiment.baseRegiment.damageType)
            {
                float d = regiment.baseRegiment.damageType >= DamageType.Range ? 50 : 0;
                info.damage[(int)regiment.baseRegiment.damageType] += (regiment.NormalCount + 1) * 0.5f * (regiment.baseRegiment.damage+ d);
            }
        }
        int walls = data.buildings[(int)Building.Walls];
        info.RangeDamage += 50 * walls;
        info.SiegeDamage += 100* walls;
        return info;
    }

    public DamageInfo Hit(DamageInfo damage)
    {
        List<Regiment> garnison = new List<Regiment>(data.garnison);
        foreach (Army insider in curOwner.army)
            if (insider.inTown && insider.curReg == this)
                garnison.AddRange(insider.army);

        if (garnison.Count == 0)
        {
            Destoyed = true;

            return null;
        }
        Regiment[] targets = new Regiment[4];
        for (int i = 0; i < targets.Length; i++)
            targets[i] = garnison[Random.Range(0, garnison.Count)];
        float q = 1f / targets.Length;
        int walls = data.buildings[(int)Building.Walls];
        foreach (var target in targets)
        {
            float d = 0;
            for (int i = 0; i < (int)DamageType.Count; i++)
                d += damage.damage[i] * DamageInfo.Armor(target.baseRegiment.armor[i] + walls * 2);
            d *= q;
            target.count -= d;
        }
        data.garnison.RemoveAll(x => x.count <= 0);
        foreach (Army insider in curOwner.army)
            if (insider.inTown && insider.curReg == this)
            {
                insider.army.RemoveAll(x => x.count <= 0);
                insider.ArmyListChange();
            }
        if(damage.MeleeDamage>0||damage.ChargeDamage>0)
        {
            return GetDamage(DamageType.Melee);
            
        }
        return null;
    }
}

public enum BorderState
{
    HiddenBorder,
    ShowStateBorder,
    ShowBorder
}