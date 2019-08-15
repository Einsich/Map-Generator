using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    //static int[] dx = { 0, 0, 1, -1 };
    //static int[] dy = { 1, -1, 0, 0 };
    public GameObject Text, worldborder, borderPrefab, CoronaPrefab, flagPrefab, WayPoint;
    public ArmyBar ArmyBarPrefab;
    public GameObject[] terrainItem;
    public GameObject[] Port,TownPrefab,ArmyPrefab;
    public Chunk ChunkPref;
    public Material forland,Rivermat;
    public Image MiniMap;
    public static List<Region> reg;
    public static List<State> states;
    public static MapMode currentmapmod;
    public Texture2D noiseSorse;
    public Transform mainCanvas;
    //public Texture2DArray TerrainSource,TerrainNormSource;
    float fHeight;
    public static int h,w;
    public static byte BSeaLevel;
    public static Main that;
    static Texture2D colorMap,occupeMap;
    static Texture2D[] land, landNorm;
    static Texture2DArray landAr, landNormAr;
    public static List<Chunk> Chunks=new List<Chunk>();
    List<List<Vector2Int>> River;
    public static byte[] terrainInd;
    public static GameObject WorldBorders;
    private void Awake()
    {
        that = this;
    }
    private void Start()
    {
        Fraction.TownPrefab = TownPrefab;
    }
    public static void Save(string path)
    {
        SaveLoad.Save(BSeaLevel, land, landNorm, terrainInd, MapMetrics.GetHeightsArray(), prov, reg, states, path);
    }
    public static void Load(string path)
    {
        byte[] Heights;
        SaveLoad.Load(out BSeaLevel, out land,out landNorm, out terrainInd, out Heights, out prov, out reg, out states, path);
        that.StartGame(BSeaLevel, land, landNorm, terrainInd, Heights, prov, reg, states,null);
    }
    bool firstMap = true;
    public void StartGame(byte SeaLevel, Texture2D[] landTex,Texture2D[] landnorm,byte[]terrain, byte[] HeightArr, int[,] provArr, List<Region> region, List<State> st,List<List<Vector2Int>>river)
    {
        MapMetrics.SizeN = h = provArr.GetLength(0);
        MapMetrics.SizeM = w = provArr.GetLength(1);
        BSeaLevel = SeaLevel;
        land = landTex;
        landNorm = landnorm;
        terrainInd = terrain;
        River = river;
        if (firstMap)
        {

            MapMetrics.noise = noiseSorse;
        }
        else
        {
        }
        MapMetrics.splatmap = new Texture2D(w, h, TextureFormat.RGBA32, false);
        MapMetrics.SetHeights(HeightArr,h,w);
        prov = Player.prov = MapMetrics.cellStateIndex = provArr;
        Player.selected = null;Player.curPlayer = null;
        reg = CameraController.reg = Player.reg =
        MapMetrics.regions = region;
        states = CameraController.state = CodePanel.state = Player.states = st;
        MapMetrics.SeaLevel = fHeight = MapMetrics.MaxHeight * SeaLevel / 255f;
        distantion = new float[h, w];
        parent = new Vector2Int[h, w]; 
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                distantion[i, j] = -1;//not used
        currentmapmod = MapMode.None;

       
        transform.GetChild(0).transform.position = new Vector3(w / 2, fHeight - 0.01f, h / 2);
        

        CreateMiniMap();
        CreateWorldBorder();

        BuildWorld();
        CreateTerrain();
        CreateArmy();
        SetMapMode(MapMode.Politic.ToString());
        CameraController.SetPosition(new Vector3(w / 2, fHeight, h / 2));
        Date.StartTimer(1, 1, 1387);
        Player.SetState(st[0]);
        if (firstMap) firstMap = false;
    }
    public static int[,] prov;
    //bool[,] used;
    //bool[] pused;
    
    void CreateWorldBorder()
    {
        if(firstMap)
        {
            WorldBorders = new GameObject("World Borders");
            for (int i = 0; i < 4; i++) 
            {
                GameObject wb = Instantiate(worldborder);
                if (i >= 2)
                    wb.transform.rotation = Quaternion.Euler(0, 90, 0);
                wb.transform.SetParent(WorldBorders.transform);
            }
        }
        WorldBorders.transform.GetChild(0).transform.position =  new Vector3(-250, MapMetrics.SeaLevel, 1000);
        WorldBorders.transform.GetChild(1).transform.position = new Vector3(w + 250, MapMetrics.SeaLevel, h - 1000-1 );
        WorldBorders.transform.GetChild(2).transform.position = new Vector3(1000, MapMetrics.SeaLevel, h + 250 -1);
        WorldBorders.transform.GetChild(3).transform.position = new Vector3(w - 1000, MapMetrics.SeaLevel, -250);
    }

    void CreateMiniMap()
    {
        int a = 100, b = 265;
        float k;
        if (a * w > b * h) k = 1f * b / w;
        else k = 1f * a / h;
        Texture2D t = new Texture2D(b, a);
        int xp, yp, i, j;
        for (i = 0; i < a; i++)
            for (j = 0; j < b; j++)
                t.SetPixel(j, i, new Color(0, 0, 0, 0));
        int x0 = (int)(w * k / 2);
        int y0 = (int)(h * k / 2);

        Color wt = Color.blue, gr = Color.green;
        for (int x = -x0; x <= x0; x++)
            for (int y = -y0; y <= y0; y++)
            {
                xp = Mathf.Clamp((int)(x / k + w * 0.5f), 0, w - 1);
                yp = Mathf.Clamp((int)(y / k + h * 0.5f), 0, h - 1);
                j = Mathf.Clamp((int)(x + b * 0.5f), 0, b - 1);
                i = Mathf.Clamp((int)(y + a * 0.5f), 0, a - 1);
                t.SetPixel(j, i, (MapMetrics.Height(new Vector2(xp,yp)) > MapMetrics.SeaLevel ? gr : wt));
            }
        t.Apply();
        MiniMap.sprite = Sprite.Create(t, new Rect(0, 0, b, a), new Vector2(0.5f, 0.5f));
    }

  
    void CalculateContinentTerritory()
    {
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                reg[prov[i, j]].territory.Add(new Vector2Int(j, i));

        Queue<Region> q = new Queue<Region>();
        int continent = 1;
        foreach (Region r in reg)
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
        foreach(var r in reg)
        if(r.portIdto>0)
            {
                Vector2Int loc = r.GetPortPosition();
                Vector2 rot = Vector2.zero;
                int n = 0;
                if (MapMetrics.InsideMap(loc.y, loc.x) && reg[prov[loc.y, loc.x]].iswater)
                { n++; rot += new Vector2(1, 1); }
                if (MapMetrics.InsideMap(loc.y, loc.x - 1) && reg[prov[loc.y, loc.x - 1]].iswater)
                { n++; rot += new Vector2(-1, 1); }
                if (MapMetrics.InsideMap(loc.y - 1, loc.x) && reg[prov[loc.y - 1, loc.x]].iswater)
                { n++; rot += new Vector2(1, -1); }
                if (MapMetrics.InsideMap(loc.y - 1, loc.x - 1) && reg[prov[loc.y - 1, loc.x - 1]].iswater)
                { n++; rot += new Vector2(-1, -1); }
                rot /= n;
                GameObject port;
                Chunk chunk = ChunkWithPosition(loc);
               /* if (PortI < PortCount)
                {
                    port = portsPater.GetChild(PortI).gameObject;
                    Destroy(port);
                }
                */
                port = Instantiate(Main.that.Port[(int)r.owner.fraction], chunk.Ports);
                port.transform.position = MapMetrics.GetCornerPosition(loc.y, loc.x, true);
                port.transform.rotation = Quaternion.Euler(0, Vector2.SignedAngle(rot, new Vector2(0, -1)), 0);
               // PortI++;
                r.Port = port;
            }
    }
      
    void BuildWorld()
    {
        CalculateContinentTerritory();
        int loc = 0;
        int tile = MapMetrics.Tile;
        Chunk cur;
        MapMesh.prov = prov;
        MapMesh.reg = reg;
        for (int i = 0; i < h; i += tile)
            for (int j = 0; j < w; j += tile)
            {

                if (Chunks.Count == loc)
                {
                    cur = Instantiate(ChunkPref);
                    Chunks.Add(cur);
                }
                else cur = Chunks[loc];
                cur.y0 = i;
                cur.x0 = j;
                cur.transform.localPosition = Vector3.zero;
                cur.Triangulate();
                loc++;
            }
        for (int i = loc; i < Chunks.Count; i++)
        {
            Chunks[i].gameObject.SetActive(false);
        }
        
        SetProvNames(loc);
        SetTowns(loc);
        BuildPorts();

        for (int i = 1; i < states.Count; i++)
        {
            GameObject text = Instantiate(Text);
            text.layer = 8;
            states[i].Text = text;
            states[i].SetName();
            states[i].SetNameStatus(true);
            Region cap = states[i].Capital;
            if (cap.Corona)
                cap.Corona.SetActive(true);
            else
                cap.Corona = Instantiate(CoronaPrefab, cap.Town.transform);
            
            cap.Corona.transform.localPosition = Vector3.up*0.8f;
        }
        

        for (int i = 0; i < reg.Count; i++)
            reg[i].UpdateBorder();

        colorMap = MapMetrics.map = new Texture2D(w, h, TextureFormat.RGBA32, false);
        MapMetrics.provincemap = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
        occupeMap = MapMetrics.occupemap = new Texture2D(w, h, TextureFormat.RGBA32, false);
        
        colorMap.filterMode = FilterMode.Point;
        occupeMap.filterMode = FilterMode.Point;
        MapMetrics.UpdateColorMap();
        MapMetrics.UpdateOccupedMap();
        MapMetrics.UpdateProvincesMap();
        System.IO.File.WriteAllBytes("DasIst.png", colorMap.EncodeToPNG());
        forland.SetTexture("_MainTex", MapMetrics.map);
        forland.SetTexture("_OccupeMap", MapMetrics.occupemap);
        forland.SetTexture("_ProvincesMap", MapMetrics.provincemap);
        Shader.SetGlobalTexture("_SplatMap", MapMetrics.splatmap);
        Shader.SetGlobalVector("_Size", new Vector4(1f / w, 1f / h, w, h));
    }

    void SetProvNames(int loc)
    {
        for (int i = 0; i < loc; i++)
        {
            Chunks[i].ChildIT = 0;
            Chunks[i].ChildCountT = Chunks[i].Names.transform.childCount;
        }
        for (int i = 0; i < reg.Count; i++)
        {
            Chunk chunk = ChunkWithPosition(reg[i].Capital);
            if (chunk.ChildIT < chunk.ChildCountT)
            {
                reg[i].Text = chunk.Names.GetChild(chunk.ChildIT).gameObject;
                reg[i].Text.SetActive(true);
            }
            else
            {
                reg[i].Text = Instantiate(Text, chunk.Names);                
            }
            chunk.ChildIT++;

            Vector3 pos = MapMetrics.GetCellPosition(reg[i].Capital,  true);
            reg[i].Text.transform.position = pos;
            reg[i].Text.layer = 8;
            
            // reg[i].SetName();
        }
        for (int i = 0; i < loc; i++)
            for (int j = Chunks[i].ChildIT; j < Chunks[i].ChildCountT; j++)
                Chunks[i].Names.GetChild(j).gameObject.SetActive(false);
    }

    void SetTowns(int loc)
    {

        for (int i = 0; i < loc; i++)
        {
            Chunks[i].ChildIT = 0;
            Chunks[i].ChildCountT = Chunks[i].Towns.transform.childCount;
        }
        for (int i = 0; i < reg.Count; i++)
            if (!reg[i].iswater)
            {
                GameObject go;
                Chunk chunk = ChunkWithPosition(reg[i].Capital);
                if (chunk.ChildIT < chunk.ChildCountT)
                {
                    go = chunk.Towns.GetChild(chunk.ChildIT++).gameObject;
                    go.SetActive(true);
                    Transform corona = go.transform.childCount > 2 ? go.transform.GetChild(2) : null ;
                    if (corona)
                        corona.gameObject.SetActive(false);
                }
                else
                {
                    go = new GameObject();
                    go.transform.SetParent(chunk.Towns);
                    GameObject town = new GameObject();
                    town.transform.SetParent(go.transform);
                    town.transform.localPosition = Vector3.zero;
                    town = Instantiate(flagPrefab, go.transform);
                    town.transform.localPosition = Vector3.zero;
                }
                go.transform.position = MapMetrics.GetCellPosition(reg[i].Capital);
                go.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(-90, 90), 0);
                go.transform.GetChild(1).localRotation = Quaternion.Inverse(go.transform.rotation);
                reg[i].Town = go;
                reg[i].RebuildTown();
            }
        for (int i = 0; i < loc; i++)
            for (int j = Chunks[i].ChildIT; j < Chunks[i].ChildCountT; j++)
                Chunks[i].Towns.GetChild(j).gameObject.SetActive(false);
    }

    void CreateTerrain()
    {
        landAr = new Texture2DArray(w, h, 4, land[0].format, land[0].mipmapCount>0);
        landNormAr = new Texture2DArray(w, h, 4, landNorm[0].format, landNorm[0].mipmapCount>0);
        
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < land[i].mipmapCount; j++)
            {
                Graphics.CopyTexture(land[i], 0, j, landAr, i, j);
                Graphics.CopyTexture(landNorm[i], 0, j, landNormAr, i, j);
            }
        }
        landAr.Apply();
        landNormAr.Apply();
        int tile = MapMetrics.Tile;
        for (int i = 0, l = 0; i < h; i += tile)
            for (int j = 0; j < w; j += tile, l++)
                foreach (Transform tr in Chunks[l].Trees)
                    Destroy(tr.gameObject);
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                int t = terrainInd[i * w + j]&31;
                if (t == (int)TerrainType.ForestLeaf || t == (int)TerrainType.ForestSpire)
                {
                    GameObject tree = Instantiate(terrainItem[t - (int)TerrainType.ForestLeaf], ChunkWithPosition(i, j).Trees);
                    tree.transform.position = MapMetrics.GetCellPosition(i,j);
                }
                
            }
        foreach (var list in River)
        {
            GameObject r = MapMesh.AddRiver(list);
            r.transform.SetParent(WorldBorders.transform);
            r.GetComponent<MeshRenderer>().sharedMaterial = Rivermat;
        }
                
            

        forland.SetTexture("_TerrainNormTex", landNormAr);
        forland.SetTexture("_TerrainTex", landAr);
    }
    void CreateArmy()
    {
        for (int i = 0; i < states.Count; i++)
        {
            Army.CreateArmy(states[i].Capital, states[i].defaultArmy());
        }
    }

   public static bool Angle(int y,int x,out Vector2Int dir)
    {
        dir = Vector2Int.zero;
        if (y >= MapMetrics.SizeN || y == 0 || x >= MapMetrics.SizeM || x == 0) 
            return false;
        int a = prov[y, x], b = prov[y - 1, x ], c = prov[y - 1, x - 1 ], d = prov[y, x - 1];
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


    public void SetMapMode(string smode)
    {
        MapMode mode = MapMode.Politic;
        for (; mode <= MapMode.Terrain; mode++)
            if (mode.ToString() == smode)
                break;
        if (mode == currentmapmod) return;
        if (mode == MapMode.Terrain)
        {
            forland.SetFloat("_TerrainMode", 1);
            if (!CameraController.showstate)
                foreach (Chunk ch in Chunks)
                    {
                        ch.Trees.gameObject.SetActive(true);
                        foreach (Transform tree in ch.Trees)
                            MapMetrics.UpdateTree(tree.gameObject);
                    }
        }

        if (mode == MapMode.Politic)
        {
            forland.SetTexture("_MainTex", colorMap);
            forland.SetTexture("_OccupeMap", occupeMap);
            forland.SetFloat("_TerrainMode", 0);
            foreach (Chunk ch in Chunks)
                ch.Trees.gameObject.SetActive(false);

        }
        
        currentmapmod = mode;
    }
    static float[,] distantion;
    static Vector2Int[,] parent;
    public static int[] dx = MapMetrics.Odx, dy = MapMetrics.Ody;
    public static PriorityQueue<Node> pq = new PriorityQueue<Node>();
    public static Queue<Vector2Int> used = new Queue<Vector2Int>();
    public static List<Vector2Int> FindPath(Vector2Int from, Vector2Int to,State goer)
    {
        // Debug.Log(from); Debug.Log(to);
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

                if (MapMetrics.InsideMap(pos.y, pos.x) && (dnext = CellMoveCost(pos)) < 100 && CanMoveTo(prev.pos,pos,goer))
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
    static bool CanMoveTo(Vector2Int prev,Vector2Int cur, State goer)
    {
        State s = MapMetrics.GetRegion(cur).owner;
        if (goer == null||goer==s)
            return true;
        
        Diplomacy dip = goer.GetDiplomacyWith(s);
        return dip.alliance || dip.forceaccess || dip.war || MapMetrics.GetRegion(prev).owner == s;
    }
    public static float CellMoveCost(Vector2Int p)
    {
        uint t = terrainInd[p.y * w + p.x];
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
    public static Chunk ChunkWithPosition(int i,int j)
    {
        return ChunkWithPosition(new Vector2Int(j, i));
    }

   public static Chunk ChunkWithPosition(Vector2Int v)
    {
        int a = 0;
        int tile = MapMetrics.Tile;
        for (int i = 0; i < h; i += tile)
            for (int j = 0; j < w; j += tile)
                if (v.y <= i + tile && v.x <= j + tile)
                    return Chunks[a];
                else a++;
        return null;
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