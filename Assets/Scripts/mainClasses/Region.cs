using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Region 
{
    public static Material[] borderMaterial;
    public string name;
    State _owner;
    Transform flag;
    MeshRenderer flagrenderer;
    MaterialPropertyBlock block=new MaterialPropertyBlock();
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
    public State ocptby;
    public int id,portIdto=-1, Continent;
    public Region[] neib;
    //public GameObject[] arrow;
    //public List<GameObject>[] border;
    public List<Vector2Int> territory;
    public bool iswater,retreatused;
    public Vector2Int Capital;
    public GameObject Text,Town,Port,Corona;
    public ProvinceData data;
    BorderState bordState;
    bool selected;

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
        Text.transform.SetParent(Main.ChunkWithPosition((int)c.z, (int)c.x).Names);
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
    public void MonthUpdate(int month)
    {
        if (iswater) return;
        
            data.EconomyUpdate();
        
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
            GameObject t1 = GameObject.Instantiate(ProvinceMenu.instance.specialBuildings[ind],town);
            t1.transform.localPosition = Vector3.right*1.4f;
                }
        GameObject t0 = GameObject.Instantiate(Fraction.TownPrefab[(int)data.fraction].transform.GetChild(lvl).gameObject, town);
        t0.transform.localPosition = Vector3.zero;
        flag = Town.transform.GetChild(1);
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

    public void UpdateArmy()
    {

    }
    public void RenderArmy()
    {

    }
}

public enum BorderState
{
    HiddenBorder,
    ShowStateBorder,
    ShowBorder
}