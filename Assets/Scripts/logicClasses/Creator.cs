using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
static class Creator {

    static int[] dx = MapMetrics.Qdx,dy = MapMetrics.Qdy;
    static int[,] regionIndex;
    static bool[,] wasAddToRegion;//добавлена ли клетка в какой-то регион
    static bool[] wasAddToState;//добавлена ли провинция в какое-то гос-во
    static int h, w, seed,RegionCount;
    static byte[] heightArray,waterArray,terrainIndexes;
    static byte seaLevel,maxHeight;
    static List<Region> regions;
    static List<State> states;
    static List<List<Vector2Int>> river;
    static bool wasCreate = false;
    public static void Create(int H, int W, int Seed, byte seaLvl, byte[] heiarr)
    {
        h = H; w = W; seed = Seed;
        heightArray = heiarr;
        Random.InitState(seed);
        seaLevel = seaLvl;
        regions = new List<Region>();
        states = new List<State>();
        river = new List<List<Vector2Int>>();
        terrainIndexes = new byte[h * w];
        terrainMask = new Texture2D[4];
        terrainNormalMask = new Texture2D[4];
        for (int i = 0; i < 4; i++)
        {
            terrainMask[i] = new Texture2D(w, h);
            terrainNormalMask[i] = new Texture2D(w, h);
        }
        CreateMap();
        CreateTerrein();
        CreateProvinces(10, 15);
        CreateState(1, 20);
        CreateProvincesData();
        CreateDiplomacy();
        wasCreate = true;
    }
    public static void StartGame()
    {
        if (!wasCreate)
        {
            int w = 100, h = 100, seed = 1000;
            byte sea = 127;
            byte[] ha = MyNoise.GetMap(h, w, seed, 0.5f, NoiseType.PerlinNoise);
            MainMenu.CreateMiniMap(sea, ha, w, h);
            Create(h, w, seed, sea, ha);
        }
        Main.instance.StartGame(seaLevel, terrainMask, terrainNormalMask, terrainIndexes, heightArray, regionIndex, regions, states, river);
    }
    static Texture2D[] terrainMask,terrainNormalMask;
    static Color[] rgba = { new Color(1, 0, 0, 0), new Color(0, 1, 0, 0), new Color(0, 0, 1, 0), new Color(0, 0, 0, 1), new Color(0, 0, 0, 0) };
    static void CreateMap()
    {
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                if (heightArray[i * w + j] == seaLevel)
                    heightArray[i * w + j] = (byte)(seaLevel - 1);
                if (heightArray[i * w + j] > maxHeight)
                    maxHeight = heightArray[i * w + j];
            }
        wasAddToRegion = new bool[h, w];
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++) 
                if (!wasAddToRegion[i,j])
                {
                    DeleteSmallSpot(i, j);
                }
    }
    static Queue<Vector2Int> q = new Queue<Vector2Int>();
    static void DeleteSmallSpot(int y0, int x0)
    {
        bool c = heightArray[y0 * w + x0] > seaLevel;
        wasAddToRegion[y0, x0] = true;
        q.Clear();
        List<Vector2Int> l = ListPool<Vector2Int>.Get();
        q.Enqueue(new Vector2Int(x0, y0));
        int x, y, s = 1;
        while (q.Count > 0)
        {
            x0 = q.Peek().x;
            y0 = q.Peek().y;
            l.Add(q.Dequeue());
            s++;
            for (int i = 0; i < 4; i++)
            {
                x = x0 + dx[i];
                y = y0 + dy[i];
                if (InsideMap(y, x))
                {
                    if (!wasAddToRegion[y, x] && (heightArray[y * w + x] > seaLevel) == c)
                    {
                        q.Enqueue(new Vector2Int(x, y));                        
                        wasAddToRegion[y, x] = true;
                    }
                }
            }
        }
        if (s > 10)
        {
            ListPool<Vector2Int>.Add(l);
            return;
        }
        foreach(Vector2Int p in l)
        {
            heightArray[p.y * w + p.x] = (byte)(c ? seaLevel - 1 : seaLevel + 1);
        }
    }
    /// <summary>
    /// инициализация типов террейна и создание рек
    /// </summary>
    static void CreateTerrein()
    {
        waterArray = MyNoise.GetNoisePerlin(h, w, seed ^ (seed * 1000));

        float elevation = 1f / (maxHeight - seaLevel);
        for (int i = 0; i < h * w; i++)
        {
            if (heightArray[i] <= seaLevel) continue;
            terrainIndexes[i] = TerId(waterArray[i] / 255f, (heightArray[i] - seaLevel) * elevation);
        }
        for (int i = 0; i < h; i++)
            for (int j = 0, t; j < w; j++)
            {
                t = terrainIndexes[i * w + j];
                terrainMask[t / 4].SetPixel(j, i, rgba[t % 4]);
                terrainNormalMask[t / 4].SetPixel(j, i, rgba[t % 4]);
                for (int k = 0; k < 4; k++)
                    if (k != t / 4)
                    {
                        terrainMask[k].SetPixel(j, i, rgba[4]);
                        terrainNormalMask[k].SetPixel(j, i, rgba[4]);
                    }
            }
        for (int i = 0; i < h; i += 20)
            for (int j = 0; j < w; j += 20)
            {
                int x = j + (int)(20 * Random.value), y = i + (int)(20 * Random.value);
                if (heightArray[y * w + x] > seaLevel)
                {
                    List<Vector2Int> path = AddRiver(y, x);
                    if (path != null && path.Count >= 3)
                        river.Add(path);
                }
            }
    }

    static int SeaNear(int y,int x)
    {
        for (int i = 0; i < dy.Length; i++)
            if (InsideMap(y + dy[i], x + dx[i]) && heightArray[(y + dy[i]) * w + x + dx[i]] < seaLevel)
                return i;
        return -1;
    }
    static List<Vector2Int> AddRiver(int y,int x)
    {
        if (!InsideMap(y, x) || SeaNear(y, x) >= 0)
            return null;
        List<Vector2Int> path = ListPool<Vector2Int>.Get();
        int dir = Random.Range(0, 4),d;
        
        while(InsideMap(y, x))
        {
            if((terrainIndexes[y*w + x]&0x80)!=0)
            {
                foreach (Vector2Int vi in path)
                    terrainIndexes[vi.y * w + vi.x] &= 0x1F;
                ListPool<Vector2Int>.Add(path);
                return null;
            }
            path.Add(new Vector2Int(x, y));
            terrainIndexes[y * w + x] |= 0x80;
            if ((d = SeaNear(y, x)) >= 0)
            {
                path.Add(new Vector2Int(x + dx[d], y + dy[d]));
                break;
            }
            float v = (Random.value - 0.5f);
            if (v > 0.35f) d = 1;
            else
                if (v < -0.35f) d = -1;
            else d = 0;
            
            dir = (dir + 8 + d) % dy.Length;
            y += dy[dir];
            x += dx[dir];
        }
        foreach (Vector2Int vi in path)
            terrainIndexes[vi.y * w + vi.x] |= 0x1F;
        return path;
    }

    static byte TerId(float v, float g)
    {
        if (g < 0.6f)
        {
            if (v <= 0.2f) return 1;//Desert
            if (v <= 0.3f) return 2;//halfsteppe
            if (v <= 0.45f)
            {
                if (g <= 0.5) return 3;//steppe
                return 4;//brownsteppe
            }
            if (v <= 0.6f)
            {
                if (g <= 0.5) return 5;//grass
                return 6;//hills
            }
            if (v <= 0.8f)
            {
                if (g <= 0.5) return 7;//forestLeaf
                return 8;//forestSpire
            }
            return 9;//swampy
        }
        if (v <= 0.2f) return 10;//desertmount
        if (v <= 0.5f) return 11;//mount
        if (v <= 0.8f) return 12;//snowmount
        return 13;//verysnowmount
    }
   
    static void CreateProvinces(int minsize, int maxsize)
    {
        List<int> id = ListPool<int>.Get();
        int n = 1;
        regionIndex = new int[h, w];
        wasAddToRegion = new bool[h, w];

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int cell = y * w + x;
                cell = (cell ^ 0x555) < w * h ? cell ^ 0x555 : cell;
                int i = cell / w, j = cell % w;
                if (regionIndex[i, j] == 0)
                {
                    if (heightArray[cell] > seaLevel)
                    {
                        FillingRegion(i, j, n, Random.Range(minsize, maxsize));
                    }
                    else
                    {
                        FillingRegion(i, j, n, Random.Range(minsize * 5 / 2, maxsize * 5 / 2));
                    }
                    id.Add(n++);
                }
            }

        int[] neuid = new int[n - 1];
        
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                if (!wasAddToRegion[i, j])
                {
                    int neib, s = CountingRegionSize(i, j, out neib);
                    if (s < minsize && neib != -1)
                    {
                        id.Remove(regionIndex[i, j]);
                        ReFillingRegion(i, j, neib);
                        n--;
                    }
                }
        RegionCount = id.Count;
        for (int i = 0; i < n - 1; i++)
            neuid[id[i] - 1] = i;
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                regionIndex[i, j] = neuid[regionIndex[i, j] - 1];
            }

        for (int i = 0; i < h - 1; i++)
            for (int j = 0; j < w - 1; j++)
            {
                if (regionIndex[i, j] == regionIndex[i + 1, j + 1] && regionIndex[i, j] != regionIndex[i + 1, j] && regionIndex[i, j] != regionIndex[i, j + 1])
                    regionIndex[i + 1, j] = regionIndex[i, j];
                else
                    if (regionIndex[i + 1, j] == regionIndex[i, j + 1] && regionIndex[i + 1, j] != regionIndex[i, j] && regionIndex[i + 1, j] != regionIndex[i + 1, j + 1])
                    regionIndex[i, j] = regionIndex[i + 1, j];
            }

        grnames = Resources.Load<TextAsset>("Textes/ProvincesNames").text.Split('\n');
        wtnames = Resources.Load<TextAsset>("Textes/SeasNames").text.Split('\n');
        Permutation(grnames);
        Permutation(wtnames);
        gri = wti = 0;

        wasAddToRegion = new bool[h, w];
        Region r;
        for (int i = 0; i < RegionCount; i++)
        {
            regions.Add(new Region());
            regions[i].id = i;
        }

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                regions[regionIndex[i, j]].iswater = heightArray[i * w + j] <= seaLevel;

       

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                if (!wasAddToRegion[i, j])
                {
                    r = regions[regionIndex[i, j]];                
                    FindAllNeib(i, j, r);
                }
        bool[] lakeis = new bool[regions.Count];
        for (int i = 0; i < regions.Count; i++)
            if (regions[i].iswater)
            {
                lakeis[i] = true;
                foreach (Region neib in regions[i].neib)
                    if (neib.iswater)
                    {
                        lakeis[i] = false; break;
                    }
            }
        for (int i = 0; i < regions.Count; i++)
        {
            regions[i].name = Names(regions[i], lakeis[i]);
            if (!regions[i].iswater)
                foreach (Region neib in regions[i].neib)
                    if (neib.iswater && !lakeis[neib.id])
                    {
                        regions[i].portIdto = neib.id;
                        break;
                    }
        }
        ListPool<int>.Add(id);
    }

    static void CreateProvincesData()
    {
        foreach (Region rg in regions)
            if(!rg.iswater)
        {
            rg.data = new ProvinceData(rg.owner.fraction, rg);
        }
    }
    static  string[] grnames,wtnames;
    static int gri, wti;
    static string Names(Region r,bool lake)
    {   
        if(r.iswater)
        {
            return wtnames[(wti++)%wtnames.Length] + (lake ? ".озеро" : ".море");         
        }
        return grnames[(gri++)%grnames.Length];
    }
    static void Permutation(string[] s)
    {
        for (int i = s.Length - 1, j; i > 0; i--)
        {
            j = Random.Range(0, i);
            string a = s[i];
            s[i] = s[j];
            s[j] = a;
        }
    }
     static void FindAllNeib(int y0, int x0, Region r)
    {
        List<int> neibor = new List<int>();
        List<Vector2Int> list = ListPool<Vector2Int>.Get();
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        wasAddToRegion[y0, x0] = true;
        q.Enqueue(new Vector2Int(x0, y0));
        list.Add(new Vector2Int(x0, y0));
        int x, y;//, X = x0, Y = y0;
        r.portIdto = -1;
        while (q.Count > 0)
        {
            x0 = q.Peek().x;
            y0 = q.Peek().y;
            q.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                x = x0 + dx[i];
                y = y0 + dy[i];
                if (InsideMap(y, x))
                {
                    int t = regionIndex[y, x], f = regionIndex[y0, x0];
                    if (!wasAddToRegion[y, x] && t == f)
                    {
                        wasAddToRegion[y, x] = true;
                        q.Enqueue(new Vector2Int(x, y));
                        list.Add(new Vector2Int(x, y));
                       // X += x; Y += y;
                    }
                    if (f != t)
                    {

                        bool find = false;
                        foreach (int d in neibor)
                            if (d == t)
                            {
                                find = true;
                                break;
                            }
                        if (!find)
                        {
                            neibor.Add(t);
                           
                        }
                    }
                }
            }
        }
        r.neib = new Region[neibor.Count];
        for (int i = 0; i < neibor.Count; i++)
            r.neib[i] = regions[neibor[i]];

        float best = 0;
        for(int i=0;i<list.Count;i++)
        {
            float d = 0;
            for (int j = 0; j < list.Count; j++)
                if (i != j)
                {
                    d += 100/(list[i] - list[j]).sqrMagnitude;
                }
            if (d > best)
            {
                best = d;
                r.Capital = list[i];
            }
        }
        ListPool<Vector2Int>.Add(list);
    }
    static void FillingRegion(int y0, int x0, int c, int size)
    {
        List<Vector2Int> q = ListPool<Vector2Int>.Get();
        int k = 0, x, y;
        q.Add(new Vector2Int(x0, y0));
        while (q.Count > 0)
        {
            float pr = 0, t;
            int j = 0;
            for (int i = 0; i < q.Count; i++)
            {
                t = GetPriority(q[i].y, q[i].x, c);
                if (pr < t)
                {
                    pr = t;
                    j = i;
                }
            }
            x0 = q[j].x; y0 = q[j].y;
            q.RemoveAt(j);
            k++;
            regionIndex[y0, x0] = c;
            for (int i = 0; i < 4; i++)
            {
                x = x0 + dx[i];
                y = y0 + dy[i];
                if (InsideMap(y, x) && regionIndex[y, x] == 0 && Identity(y, x, y0, x0) && !q.Contains(new Vector2Int(x,y)))
                    q.Add(new Vector2Int(x, y));
            }
            if (k > size) break;
        }
        ListPool<Vector2Int>.Add(q);
    }

    static float GetPriority(int y0, int x0, int c)
    {
        int r = 0, x, y;
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                x = x0 + j;
                y = y0 + i;
                if (InsideMap(y, x) && regionIndex[y, x] == c) r++;
            }

        return (Random.Range(0, Mathf.Pow(r, 0.7f)));
    }
    
    static int CountingRegionSize(int y0, int x0, out int neib)
    {
        int c = regionIndex[y0, x0];
        wasAddToRegion[y0, x0] = true;
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(x0, y0));
        int x, y, s = 1; neib = -1;
        while (q.Count > 0)
        {
            x0 = q.Peek().x;
            y0 = q.Peek().y;
            q.Dequeue();
            s++;
            for (int i = 0; i < 4; i++)
            {
                x = x0 + dx[i];
                y = y0 + dy[i];
                if (InsideMap(y, x))
                {
                    if (!wasAddToRegion[y, x] && regionIndex[y, x] == c)
                    {
                        q.Enqueue(new Vector2Int(x, y));
                        wasAddToRegion[y, x] = true;
                    }
                    if (Identity(y0, x0, y, x) && regionIndex[y, x] != c)
                    {
                        neib = regionIndex[y, x];
                    }
                }
            }
        }
        return s;
    }
    static void ReFillingRegion(int y0, int x0, int newcolor)
    {
        int c = regionIndex[y0, x0];
        wasAddToRegion[y0, x0] = false;
        regionIndex[y0, x0] = newcolor;
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(x0, y0));
        int x, y;

        while (q.Count > 0)
        {
            x0 = q.Peek().x;
            y0 = q.Peek().y;
            q.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                x = x0 + dx[i];
                y = y0 + dy[i];
                if (InsideMap(y, x) && regionIndex[y, x] == c)
                {
                    q.Enqueue(new Vector2Int(x, y));
                    wasAddToRegion[y, x] = false;
                    regionIndex[y, x] = newcolor;
                }
            }
        }
    }

    static void CreateState(int min, int max)
    {
        
        wasAddToState = new bool[RegionCount];
        for (int i = 0; i < RegionCount; i++)
        {
            if (!wasAddToState[i] && !regions[i].iswater && states.Count < MapMetrics.CustomStateCount - 1)
            {
                List<int> l = ListPool<int>.Get();
                AddRegions(i, l, Random.Range(min, max));
                State stat = new State();
                foreach (int rg in l)
                {
                    stat.regions.Add(regions[rg]);
                    regions[rg].owner = stat;
                }
                states.Add(stat);
                ListPool<int>.Add(l);
            }
        }
        string[] names = Resources.Load<TextAsset>("Textes/States").text.Split('\n');
        string[] colors = Resources.Load<TextAsset>("Textes/StateColor").text.Split(' ');
        List<Color> normcolor = new List<Color>();
        for (int i = 0; i+2 < colors.Length; i += 3)
            normcolor.Add(new Color(float.Parse(colors[i]), float.Parse(colors[i+1]), float.Parse(colors[i+2])));

        int n = normcolor.Count;
        int[] idstate = new int[n];
        for (int i = 0; i < n; i++) idstate[i] = i;
        for (int i = n - 1; i > 1; i--)
        {
            int j = Random.Range(0, i);
            int a = idstate[i];
            idstate[i] = idstate[j];
            idstate[j] = a;
        }
        int k = 0;
        foreach(var state in states)
        {

            state.mainColor = normcolor[idstate[k]];
            state.name = names[idstate[k]].TrimEnd('\r');
            state.flag = Resources.Load<Texture2D>("FlagTexture/(" + idstate[k] + ")");
            if (state.regions.Count > 0)
                state.Capital = state.regions[0];
            else Debug.LogErrorFormat("states[{0}] without regions!", k);

            state.fraction = (FractionName)(k % 2);
            state.defaultLeader();
            state.defaultLeader();
            state.defaultLeader();
            k++;
        }
    }
   static void AddRegions(int i0, List<int>st, int size)
    {
        Queue<int> q = new Queue<int>();
        q.Enqueue(i0);
        wasAddToState[i0] = true;
        st.Add(i0);
        int s = 1;
        while (q.Count > 0)
        {
            int i = q.Dequeue();

            foreach (Region x in regions[i].neib)
                if (!wasAddToState[x.id])
                {
                    wasAddToState[x.id] = true;
                    q.Enqueue(x.id);
                    if (!x.iswater)
                    {
                        st.Add(x.id);
                        s++;
                    }
                    if (s > size) return;
                }
        }
    }
  static  bool Identity(int y0, int x0, int y1, int x1)
    {
        return heightArray[y0 * w + x0] > seaLevel && heightArray[y1 * w + x1] > seaLevel ||
            heightArray[y0 * w + x0] <= seaLevel && heightArray[y1 * w + x1] <= seaLevel;
    }
    static bool InsideMap(int y,int x)
    {
        return 0 <= y && y < h && 0 <= x && x < w;
    }
    static void CreateDiplomacy()
    {
        Diplomacy.InitDiplomacy(states.Count);
        for (int i = 0; i < states.Count; i++)
        {
            states[i].ID = i;
            states[i].diplomacy = new Diplomacy(states[i]);
        }
        for (int i = 0; i < states.Count; i++)
            for (int j = i; j < states.Count; j++)
            {
                bool alliance = Random.value > 0.66f;
                states[i].diplomacy.MakeAlliance(states[j].diplomacy, alliance);
                states[i].diplomacy.DeclareWar(states[j].diplomacy, alliance ? false : Random.value > 0.5f);                
            }
    }
}
public enum TerrainType
{
    Water,
    Desert,
    HalfSteppe,
    Steppe,
    BrownSteppe,
    Grass,
    Hills,
    ForestLeaf,
    ForestSpire,
    Swamp,
    MountainDesert,
    Mountain,
    MountainSnow,
    MountainVerySnow
}