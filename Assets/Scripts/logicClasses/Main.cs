using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public GameObject Text, worldborder, CoronaPrefab, flagPrefab, WayPoint,SiegePrefab;
    public ArmyBar ArmyBarPrefab;
    public GameObject[] terrainItem;
    public GameObject[] Port,TownPrefab,WallsPrefab,ArmyPrefab;
    public Transform Towns, Ports, Trees, Names;
    public MapMesh mapMeshPrefab;
    public Material terrainMat,riverMat;
    public static List<Region> regions;
    public static List<State> states;
    public static MapMode mapMode;
    public Texture2D noiseSorse;
    public Transform mainCanvas;
    float seaLevelf;
    public static int h,w;
    public static byte seaLevel;
    public static Main instance;
    static Texture2D regionMap,occupationMap;
    static Texture2D[] TerrainMask, TerrainNormalMask;
    static Texture2DArray TerrainMaskArray, TerrainNormalMaskArray;
    public static List<MapMesh> MapMeshes = new List<MapMesh>();
    List<List<Vector2Int>> River;
    public static byte[] terrainIndexes;
    public static int[,] regionIndex;
    public static GameObject WorldBorders;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        Fraction.TownPrefab = TownPrefab;
        Fraction.WallsPrefab = WallsPrefab;
        Creator.StartGame();
    }
    public static void Save(string path)
    {
        SaveLoad.Save(seaLevel, TerrainMask, TerrainNormalMask, terrainIndexes, MapMetrics.GetHeightsArray(), regionIndex, regions, states, path);
    }
    public static void Load(string path)
    {
        byte[] Heights;
        SaveLoad.Load(out seaLevel, out TerrainMask,out TerrainNormalMask, out terrainIndexes, out Heights, out regionIndex, out regions, out states, path);
        instance.StartGame(seaLevel, TerrainMask, TerrainNormalMask, terrainIndexes, Heights, regionIndex, regions, states,null);
    }
    public void StartGame(byte SeaLevel, Texture2D[] landTex,Texture2D[] landnorm,byte[]terrain, byte[] HeightArr, int[,] regIndex, List<Region> region, List<State> st,List<List<Vector2Int>>river)
    {
        MapMetrics.SizeN = h = regIndex.GetLength(0);
        MapMetrics.SizeM = w = regIndex.GetLength(1);
        seaLevel = SeaLevel;
        TerrainMask = landTex;
        TerrainNormalMask = landnorm;
        terrainIndexes = terrain;
        River = river;
        MapMetrics.noise = noiseSorse;
        MapMetrics.splatmap = new Texture2D(w, h, TextureFormat.RGBA32, false);
        MapMetrics.SetHeights(HeightArr,h,w);
        Navigation.Init(h, w);
        regionIndex = Player.regionIndex = MapMetrics.cellStateIndex = regIndex;

        regions = CameraController.regions = Player.regions =
        MapMetrics.regions = region;
        states = CameraController.state = CodePanel.state = Player.states = st;
        MapMetrics.SeaLevel = seaLevelf = MapMetrics.MaxHeight * SeaLevel / 255f;
        distantion = new float[h, w];
        parent = new Vector2Int[h, w]; 
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                distantion[i, j] = -1;//not used
        mapMode = MapMode.None;
        MenuManager.SetMiniMap();
       
        transform.GetChild(0).transform.position = new Vector3(w / 2, seaLevelf - 0.01f, h / 2);
        
        CreateWorldBorder();

        BuildWorld();
        CreateTerrain();
        CreateArmy();
        SetMapMode(MapMode.Politic);
        CameraController.SetPosition(new Vector3(w / 2, seaLevelf, h / 2));
        MenuManager.StartTimer();
        Player.SetState(st[0]);
    }

    void CreateWorldBorder()
    {
        WorldBorders = new GameObject("World Borders");
        for (int i = 0; i < 4; i++)
        {
            GameObject wb = Instantiate(worldborder);
            if (i >= 2)
                wb.transform.rotation = Quaternion.Euler(0, 90, 0);
            wb.transform.SetParent(WorldBorders.transform);
        }

        WorldBorders.transform.GetChild(0).transform.position = new Vector3(-250, MapMetrics.SeaLevel, 1000);
        WorldBorders.transform.GetChild(1).transform.position = new Vector3(w + 250, MapMetrics.SeaLevel, h - 1000 - 1);
        WorldBorders.transform.GetChild(2).transform.position = new Vector3(1000, MapMetrics.SeaLevel, h + 250 - 1);
        WorldBorders.transform.GetChild(3).transform.position = new Vector3(w - 1000, MapMetrics.SeaLevel, -250);
    }
    

  
    void CalculateContinentTerritory()
    {
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                regions[regionIndex[i, j]].territory.Add(new Vector2Int(j, i));
        foreach (var r in regions)
            if (!r.iswater)
                r.CalculateTerreinTypes();
        Queue<Region> q = new Queue<Region>();
        int continent = 1;
        foreach (Region r in regions)
            if (r.Continent != 0)
            {
                q.Enqueue(r);
                r.Continent = continent;
                while (q.Count > 0)
                {
                    q.Dequeue();
                    foreach (Region neib in r.neib)
                        if (neib.iswater == r.iswater)
                        {
                            q.Enqueue(neib);
                            neib.Continent = continent;
                        }
                }
                continent++;
            }
    }
    void BuildPorts()
    {
        foreach(var r in regions)
        if(r.portIdto>=0)
            {
                Vector2Int loc = r.GetPortPosition();
                Vector2 rot = Vector2.zero;
                int n = 0;
                if (MapMetrics.InsideMap(loc.y, loc.x) && regions[regionIndex[loc.y, loc.x]].iswater)
                { n++; rot += new Vector2(1, 1); }
                if (MapMetrics.InsideMap(loc.y, loc.x - 1) && regions[regionIndex[loc.y, loc.x - 1]].iswater)
                { n++; rot += new Vector2(-1, 1); }
                if (MapMetrics.InsideMap(loc.y - 1, loc.x) && regions[regionIndex[loc.y - 1, loc.x]].iswater)
                { n++; rot += new Vector2(1, -1); }
                if (MapMetrics.InsideMap(loc.y - 1, loc.x - 1) && regions[regionIndex[loc.y - 1, loc.x - 1]].iswater)
                { n++; rot += new Vector2(-1, -1); }
                rot /= n;
                GameObject port = Instantiate(Port[(int)r.owner.fraction], Ports);
                port.transform.position = MapMetrics.GetCornerPosition(loc.y, loc.x, true);
                port.transform.rotation = Quaternion.Euler(0, Vector2.SignedAngle(rot, new Vector2(0, -1)), 0);
                r.Port = port;
            }
    }
      
    void BuildWorld()
    {
        CalculateContinentTerritory();
        int tile = MapMetrics.Tile;
        MapMesh.regionIndex = regionIndex;
        MapMesh.regions = regions;
        for (int i = 0; i < h; i += tile)
            for (int j = 0; j < w; j += tile)
            {
                MapMesh mapMesh = Instantiate(mapMeshPrefab);
                MapMeshes.Add(mapMesh);
                mapMesh.TriangulateMap(j, i);
                mapMesh.transform.localPosition = Vector3.zero;
            }
        
        SetProvNames();
        SetTowns();
        BuildPorts();

        foreach (var state in states)
        {
            state.Text = Instantiate(Text);
            state.Text.layer = 8;
            state.SetName();
            state.SetNameStatus(true);
            Region cap = state.Capital;
            cap.Corona = Instantiate(CoronaPrefab, cap.Town.transform);
            cap.Corona.transform.localPosition = Vector3.up * 0.8f;
        }
        

        for (int i = 0; i < regions.Count; i++)
            regions[i].UpdateBorder();

        regionMap = MapMetrics.map = new Texture2D(w, h, TextureFormat.RGBA32, false);
        MapMetrics.provincemap = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
        occupationMap = MapMetrics.occupemap = new Texture2D(w, h, TextureFormat.RGBA32, false);
        
        regionMap.filterMode = FilterMode.Point;
        occupationMap.filterMode = FilterMode.Point;
        MapMetrics.UpdateColorMap();
        MapMetrics.UpdateOccupedMap();
        MapMetrics.UpdateProvincesMap();
        System.IO.File.WriteAllBytes("DasIst.png", regionMap.EncodeToPNG());
        terrainMat.SetTexture("_MainTex", MapMetrics.map);
        terrainMat.SetTexture("_OccupeMap", MapMetrics.occupemap);
        terrainMat.SetTexture("_ProvincesMap", MapMetrics.provincemap);
        terrainMat.SetColor("_Select", Color.white);

        Shader.SetGlobalTexture("_SplatMap", MapMetrics.splatmap);
        Shader.SetGlobalVector("_Size", new Vector4(1f / w, 1f / h, w, h));
    }

    void SetProvNames()
    {

        foreach(var reg in regions)
        {
            reg.Text = Instantiate(Text, Names);
            Vector3 pos = MapMetrics.GetCellPosition(reg.Capital, true);
            reg.Text.transform.position = pos;
            reg.Text.layer = 8;
            
        }
    }

    void SetTowns()
    {

        foreach (var reg in regions)
            if (!reg.iswater)
            {
                GameObject go = new GameObject(reg.id.ToString());
                reg.Town = go;
                go.tag = "Town";
                go.AddComponent<SphereCollider>().radius = 0.71f ;
                go.transform.SetParent(Towns);
                GameObject town = new GameObject();
                town.transform.SetParent(go.transform);
                town.transform.localPosition = Vector3.zero;
                
                town = Instantiate(flagPrefab, go.transform);
                town.transform.localPosition = Vector3.zero;
                reg.flag = town.transform;
                reg.pos = NavAgent.FromV3(go.transform.position = MapMetrics.GetCellPosition(reg.Capital));
                go.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(-90, 90), 0);
                go.transform.GetChild(1).localRotation = Quaternion.Inverse(go.transform.rotation);
                reg.RebuildTown();
            }
    }

    void CreateTerrain()
    {
        TerrainMaskArray = new Texture2DArray(w, h, 4, TerrainMask[0].format, TerrainMask[0].mipmapCount>0);
        TerrainNormalMaskArray = new Texture2DArray(w, h, 4, TerrainNormalMask[0].format, TerrainNormalMask[0].mipmapCount>0);
        
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < TerrainMask[i].mipmapCount; j++)
            {
                Graphics.CopyTexture(TerrainMask[i], 0, j, TerrainMaskArray, i, j);
                Graphics.CopyTexture(TerrainNormalMask[i], 0, j, TerrainNormalMaskArray, i, j);
            }
        }
        TerrainMaskArray.Apply();
        TerrainNormalMaskArray.Apply();
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                int t = terrainIndexes[i * w + j]&31;
                if (t == (int)TerrainType.ForestLeaf || t == (int)TerrainType.ForestSpire)
                {
                    GameObject tree = Instantiate(terrainItem[t - (int)TerrainType.ForestLeaf], Trees);
                    tree.transform.position = MapMetrics.GetCellPosition(i,j);
                }
                
            }
        foreach (var list in River)
        {
            GameObject r = MapMesh.AddRiver(list);
            r.transform.SetParent(WorldBorders.transform);
            r.GetComponent<MeshRenderer>().sharedMaterial = riverMat;
        }
                
            

        terrainMat.SetTexture("_TerrainNormTex", TerrainNormalMaskArray);
        terrainMat.SetTexture("_TerrainTex", TerrainMaskArray);
    }
    void CreateArmy()
    {
        Army.AllArmy.Clear();
        foreach (var state in states)
        {
            state.persons[0].SetToCapital();
        }//  Army.CreateArmy(state.Capital, state.defaultArmy(), state.persons[0]);
        
    }

   public static bool Angle(int y,int x,out Vector2Int dir)
    {
        dir = Vector2Int.zero;
        if (y >= MapMetrics.SizeN || y == 0 || x >= MapMetrics.SizeM || x == 0) 
            return false;
        int a = regionIndex[y, x], b = regionIndex[y - 1, x ], c = regionIndex[y - 1, x - 1 ], d = regionIndex[y, x - 1];
        //d a
        //c b
        if (a == b && b == c && a != d)
        {
            dir = new Vector2Int(-1,1);
            return true;
        }
        if (c==d && b == c && a != d)
        {
            dir = new Vector2Int(1, 1);
            return true;
        }
        if (a == d && d == c && a != b)
        {
            dir = new Vector2Int(1, -1);
            return true;
        }
        if (a == d && b == a && a != c)
        {
            dir = new Vector2Int(-1, -1);
            return true;
        }
        return false;
    }


    public void SetMapMode(MapMode mode)
    {
        if (mode == mapMode) return;
        if (mode == MapMode.Terrain)
        {
            terrainMat.SetFloat("_TerrainMode", 1);
            if (!CameraController.showstate)
            {
                Trees.gameObject.SetActive(true);
                foreach (Transform tree in Trees)
                    MapMetrics.UpdateTree(tree.gameObject);
            }
        }

        if (mode == MapMode.Politic)
        {
            terrainMat.SetTexture("_MainTex", regionMap);
            terrainMat.SetTexture("_OccupeMap", occupationMap);
            terrainMat.SetFloat("_TerrainMode", 0);
            Trees.gameObject.SetActive(false);

        }

        mapMode = mode;
    }
    static float[,] distantion;
    static Vector2Int[,] parent;
    public static int[] dx = MapMetrics.Odx, dy = MapMetrics.Ody;
    public static PriorityQueue<Node> pq = new PriorityQueue<Node>();
    public static Queue<Vector2Int> used = new Queue<Vector2Int>();
    public static List<Vector2Int> FindPath(Vector2Int from, Vector2Int to,State goer)
    {
        if (MapMetrics.GetRegion(from).Continent != MapMetrics.GetRegion(to).Continent)
            return null;
        Node prev = new Node(Heuristic(from, to), from), next;
        distantion[from.y, from.x] = 0;
        Vector2Int pos;
        pq.Enqueue(prev);
        float dnext = 1000,dprev = CellMoveCost(from),dist;
        bool find = false;
        while (pq.Count != 0)
        {
            prev = pq.Dequeue();
            used.Enqueue(prev.pos);
            if (prev.pos == to)
            {
                find = true;
                break;
            }
            dprev = CellMoveCost(prev.pos);
            for (int i = 0; i < 8; i++)
            {
                pos = new Vector2Int(dx[i], dy[i]) + prev.pos;
                next = new Node(Heuristic(prev.pos, pos), pos);

                if (MapMetrics.InsideMap(pos.y, pos.x) && (dnext = CellMoveCost(pos)) < 100 && CanMoveTo(prev.pos,pos,goer,pos == to))
                {
                    dist = distantion[prev.pos.y, prev.pos.x] + ((i & 1)==0 ? 0.5f : 0.7f) * (dnext + dprev);
                    if (distantion[pos.y, pos.x] >= 0 && distantion[pos.y, pos.x] <= dist)
                        continue;
                    if (distantion[pos.y, pos.x] < 0 || distantion[pos.y, pos.x] > dist)
                    {
                        if (distantion[pos.y, pos.x] < 0)
                            pq.Enqueue(new Node(Heuristic(prev.pos, next.pos) + dist, next.pos));
                        distantion[pos.y, pos.x] = dist;
                        parent[pos.y, pos.x] = prev.pos;
                    }
                }
            }
        }
        
        List<Vector2Int> path = null;
        if (find)
        {
            path = ListPool<Vector2Int>.Get();
            while (from!=to)
            {
                path.Add(to);
                to = parent[to.y, to.x];
            }
            //path.Add(to);without fromPoint
            path.Reverse();
        }
        while (used.Count != 0)
        {
            pos = used.Dequeue();
            distantion[pos.y, pos.x] = -1;
        }
        while (pq.Count != 0)
        {
            pos = pq.Dequeue().pos;
            distantion[pos.y, pos.x] = -1;
        }
        return path;
    }
    static float Heuristic(Vector2Int from,Vector2Int to)
    {
        return (from - to).magnitude * 10;
    }
    static bool CanMoveTo(Vector2Int prev,Vector2Int cur, State goer,bool lastCell)
    {
        
        Region reg = MapMetrics.GetRegion(cur);
        State s = reg.ocptby == null ? reg.owner : reg.ocptby;
        if (goer == s) 
            return true;
        if (s == null || !lastCell &&  reg.Capital == cur)
            return false;
        Army army = Army.ArmyInPoint(cur);
        if (army != null && !goer.diplomacy.canAttack(army?.owner.diplomacy))
            return false;

        return goer.diplomacy.canMove(s.diplomacy) || MapMetrics.GetRegion(prev).owner == s;
        
    }
    public static float CellMoveCost(Vector2Int p)
    {
        uint t = terrainIndexes[p.y * w + p.x];
        TerrainType type = (TerrainType)(t & 31);
        float riv = (t & 0x80) != 0 ? 4 : 0;
        if (t == 0) return 10000;
        if (TerrainType.ForestLeaf <= type && type <= TerrainType.ForestSpire)
            return 5 + riv;
        if (TerrainType.MountainDesert <= type && type <= TerrainType.MountainVerySnow)
            return 10 + riv;
        if (TerrainType.Swamp == type)
            return 12 + riv;
        return 3 + riv;
    }
}

public struct Node : IComparable<Node>
{
    public float dist;
    public Vector2Int pos;
    public Node(float d,Vector2Int p)
    {
        dist = d;pos = p;
    }
    public int CompareTo(Node other)
    {
        if (dist < other.dist) return -1;
        if (dist > other.dist) return 1;
        return 0;
    }
    
}
public enum MapMode
{
    None,
    Politic,
    Terrain
}
public class PriorityQueue<T> where T : IComparable<T>
{
    List<T> list;
    public int Count { get { return list.Count; } }

    public PriorityQueue()
    {
        list = new List<T>();
    }

    public void Enqueue(T x)
    {
        list.Add(x);
        int i = Count - 1;

        while (i > 0)
        {
            int p = (i - 1) / 2;
            if (list[p].CompareTo(x) <= 0) break;

            list[i] = list[p];
            i = p;
        }

        if (Count > 0) list[i] = x;
    }

    public T Dequeue()
    {
        T target = Peek();
        T root = list[Count - 1];
        list.RemoveAt(Count - 1);

        int i = 0;

        while (i * 2 + 1 < Count )
        {
            int a = i * 2 + 1;
            int b = i * 2 + 2;
            int c = b < Count && list[b].CompareTo(list[a]) < 0 ? b : a;

            if (list[c].CompareTo(root) >= 0) break;
            list[i] = list[c];
            i = c;
        }

        if (Count > 0) list[i] = root;
        return target;
    }

    public void Remove(T x)
    {
        int j=-1;
        for(int i=0;i<Count;i++)
            if(list[i].CompareTo(x)==0)
            {
                j = i;break;
            }
        if(j<0) throw new InvalidOperationException("Queue not contains this.");
        x = list[0];
        while(j > 0)
        {
            int p = (j - 1) / 2;
            if (list[p].CompareTo(x) <= 0) break;

            list[j] = list[p];
            j = p;
        }

        if (Count > 0) list[j] = x;
        Dequeue();
    }

    public T Peek()
    {
        if (Count == 0) throw new InvalidOperationException("Queue is empty.");
        return list[0];
    }

    public void Clear()
    {
        list.Clear();
    }
}