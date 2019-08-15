using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class Creator {

    static int[] dx = MapMetrics.Qdx,dy = MapMetrics.Qdy;
    static int[,] prov;
    static bool[,] used;
    static bool[] pused;
    static int h, w, seed,regcount;
    static byte[] ground,ter;
    static byte Height,MaxH;
    static List<Region> reg;
    static List<State> states;
    static List<List<Vector2Int>> river;
    public static void Create(int H,int W,int Seed,byte Hei,byte [] heiarr)
    {
        h = H;w = W;seed = Seed;
        ground = heiarr;
        Random.InitState(seed);
        Height = Hei;
        reg = new List<Region>();
        states = new List<State>();
        river = new List<List<Vector2Int>>();
        ter = new byte[h * w];
        land = new Texture2D[4];
        landNorm = new Texture2D[4];
        for (int i = 0; i < 4; i++)
        {
            land[i] = new Texture2D(w, h);
            landNorm[i] = new Texture2D(w, h);
        }
        CreateMap();
        CreateTerrein();
        CreateProvinces(10,15);
        CreateState(1,20);
        CreateProvincesData();
        CreateDiplomacy();
        Main.that.StartGame(Hei, land, landNorm, ter, ground, prov, reg, states, river);
    }
    static Texture2D[] land,landNorm;
    static byte[] water;
    static Color[] rgba = { new Color(1, 0, 0, 0), new Color(0, 1, 0, 0), new Color(0, 0, 1, 0), new Color(0, 0, 0, 1), new Color(0, 0, 0, 0) };
    static void CreateMap()
    {
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                if (ground[i * w + j] == Height)
                    ground[i * w + j] = (byte)(Height - 1);
                if (ground[i * w + j] > MaxH)
                    MaxH = ground[i * w + j];
            }
        used = new bool[h, w];
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++) 
                if (!used[i,j])
                {
                    CountingH(i, j);
                }
    }
    static Queue<Vector2Int> q = new Queue<Vector2Int>();
    static void CountingH(int y0, int x0)
    {
        bool c = ground[y0 * w + x0] > Height;
        used[y0, x0] = true;
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
                if (Gran(y, x))
                {
                    if (!used[y, x] && (ground[y * w + x] > Height) == c)
                    {
                        q.Enqueue(new Vector2Int(x, y));                        
                        used[y, x] = true;
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
            ground[p.y * w + p.x] = (byte)(c ? Height - 1 : Height + 1);
        }
    }
    static void CreateTerrein()
    {
        water = MyNoise.GetNoise(h, w, seed ^ (seed * 1000));

        float Ev = 1f / (MaxH - Height);
        for (int i = 0; i < h * w; i++)
        {
            if (ground[i] <= Height) continue;
            ter[i] = TerId(water[i] / 255f, (ground[i] - Height) * Ev);
        }
        int t;
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                t = ter[i * w + j];
                land[t / 4].SetPixel(j, i, rgba[t % 4]);
                landNorm[t / 4].SetPixel(j, i, rgba[t % 4]);
                for (int k = 0; k < 4; k++)
                    if (k != t / 4)
                    {
                        land[k].SetPixel(j, i, rgba[4]);
                        landNorm[k].SetPixel(j, i, rgba[4]);
                    }
            }
        for (int i = 0; i < h; i += 20)
            for (int j = 0; j < w; j += 20)
            {
                int x = j + (int)(20 * Random.value), y = i + (int)(20 * Random.value);
                if (ground[y * w + x] > Height)
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
            if (Gran(y + dy[i], x + dx[i]) && ground[(y + dy[i]) * w + x + dx[i]] < Height)
                return i;
        return -1;
    }
    static byte HeiDir(Vector2Int p,int d)
    {
        d = (d + 4) % 4;
        p.x += dx[d];
        p.y += dy[d];
        return ground[p.y * w + p.x];
    }
    static List<Vector2Int> AddRiver(int y,int x)
    {
        if (!Gran(y, x) || SeaNear(y, x) >= 0)
            return null;
        List<Vector2Int> path = ListPool<Vector2Int>.Get();
        int dir = Random.Range(0, 4),d;
        //byte hei = ground[y * w + x];

        float v;
        while(Gran(y, x))
        {
            if((ter[y*w + x]&0x80)!=0)
            {
                foreach (Vector2Int vi in path)
                    ter[vi.y * w + vi.x] &= 0x1F;
                ListPool<Vector2Int>.Add(path);
                return null;
            }
            path.Add(new Vector2Int(x, y));
            ter[y * w + x] |= 0x80;
            if ((d = SeaNear(y, x)) >= 0)
            {
                path.Add(new Vector2Int(x + dx[d], y + dy[d]));
                break;
            }
            v = (Random.value - 0.5f);
            if (v > 0.35f) d = 1;
            else
                if (v < -0.35f) d = -1;
            else d = 0;
            
            dir = (dir + 8 + d) % dy.Length;
            y += dy[dir];
            x += dx[dir];
        }
        foreach (Vector2Int vi in path)
            ter[vi.y * w + vi.x] |= 0x1F;
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
        prov = new int[h, w];
        used = new bool[h, w];

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int cell = y * w + x;
                cell = (cell ^ 0x555) < w * h ? cell ^ 0x555 : cell;
                int i = cell / w, j = cell % w;
                if (prov[i, j] == 0)
                {
                    if (ground[cell] > Height)
                    {
                        Bfs(i, j, n, Random.Range(minsize, maxsize));
                    }
                    else
                    {
                        Bfs(i, j, n, Random.Range(minsize * 5 / 2, maxsize * 5 / 2));
                    }
                    id.Add(n++);
                }
            }

        int[] neuid = new int[n - 1];
        
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                if (!used[i, j])
                {
                    int neib, s = Counting(i, j, out neib);
                    if (s < minsize && neib != -1)
                    {
                        id.Remove(prov[i, j]);
                        RePaint(i, j, neib);
                        n--;
                    }
                }
        regcount = id.Count;
        for (int i = 0; i < n - 1; i++)
            neuid[id[i] - 1] = i;
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                prov[i, j] = neuid[prov[i, j] - 1];
            }

        for (int i = 0; i < h - 1; i++)
            for (int j = 0; j < w - 1; j++)
            {
                if (prov[i, j] == prov[i + 1, j + 1] && prov[i, j] != prov[i + 1, j] && prov[i, j] != prov[i, j + 1])
                    prov[i + 1, j] = prov[i, j];
                else
                    if (prov[i + 1, j] == prov[i, j + 1] && prov[i + 1, j] != prov[i, j] && prov[i + 1, j] != prov[i + 1, j + 1])
                    prov[i, j] = prov[i + 1, j];
            }


        grnames = File.ReadAllLines("Assets/Textes/ProvincesNames.txt");
        wtnames = File.ReadAllLines("Assets/Textes/SeasNames.txt");
        Permutation(grnames);
        Permutation(wtnames);
        gri = wti = 0;

        used = new bool[h, w];
        Region r;
        for (int i = 0; i < regcount; i++)
        {
            reg.Add(new Region());
            reg[i].id = i;
        }

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                reg[prov[i, j]].iswater = ground[i * w + j] <= Height;

       

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                if (!used[i, j])
                {
                    r = reg[prov[i, j]];                
                    FindAllNeib(i, j, r);
                }
        bool[] lakeis = new bool[reg.Count];
        for (int i = 0; i < reg.Count; i++)
            if (reg[i].iswater)
            {
                lakeis[i] = true;
                foreach (Region neib in reg[i].neib)
                    if (neib.iswater)
                    {
                        lakeis[i] = false; break;
                    }
            }
        for (int i = 0; i < reg.Count; i++)
        {
            reg[i].name = Names(reg[i], lakeis[i]);
            if (!reg[i].iswater)
                foreach (Region neib in reg[i].neib)
                    if (neib.iswater && !lakeis[neib.id])
                    {
                        reg[i].portIdto = neib.id;
                        break;
                    }
        }
        ListPool<int>.Add(id);
    }

    static void CreateProvincesData()
    {
        foreach (Region rg in reg)
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
        used[y0, x0] = true;
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
                if (Gran(y, x))
                {
                    int t = prov[y, x], f = prov[y0, x0];
                    if (!used[y, x] && t == f)
                    {
                        used[y, x] = true;
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
            r.neib[i] = reg[neibor[i]];

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
    static void Bfs(int y0, int x0, int c, int size)
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
            prov[y0, x0] = c;
            for (int i = 0; i < 4; i++)
            {
                x = x0 + dx[i];
                y = y0 + dy[i];
                if (Gran(y, x) && prov[y, x] == 0 && Identity(y, x, y0, x0) && !IsItIn(q, y, x))
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
                if (Gran(y, x) && prov[y, x] == c) r++;
            }

        return (Random.Range(0, Mathf.Pow(r, 0.7f)));
    }

    static bool IsItIn(List<Vector2Int> l, int y, int x)
    {
        foreach (Vector2Int p in l)
            if (x == p.x && y == p.y)
                return true;
        return false;
    }
    static int Counting(int y0, int x0, out int neib)
    {
        int c = prov[y0, x0];
        used[y0, x0] = true;
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
                if (Gran(y, x))
                {
                    if (!used[y, x] && prov[y, x] == c)
                    {
                        q.Enqueue(new Vector2Int(x, y));
                        used[y, x] = true;
                    }
                    if (Identity(y0, x0, y, x) && prov[y, x] != c)
                    {
                        neib = prov[y, x];
                    }
                }
            }
        }
        return s;
    }
    static void RePaint(int y0, int x0, int newcolor)
    {
        int c = prov[y0, x0];
        used[y0, x0] = false;
        prov[y0, x0] = newcolor;
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
                if (Gran(y, x) && prov[y, x] == c)
                {
                    q.Enqueue(new Vector2Int(x, y));
                    used[y, x] = false;
                    prov[y, x] = newcolor;
                }
            }
        }
    }

    static void CreateState(int min, int max)
    {
        
        pused = new bool[regcount];
        for (int i = 0; i < regcount; i++)
        {
            if (!pused[i] && !reg[i].iswater && states.Count < MapMetrics.CustomStateCount - 1)
            {
                List<int> l = ListPool<int>.Get();
                BFS(i, l, Random.Range(min, max));
                State stat = new State();
                foreach (int rg in l)
                {
                    stat.reg.Add(reg[rg]);
                    reg[rg].owner = stat;
                }
                states.Add(stat);
                ListPool<int>.Add(l);
            }
        }

        Texture2D colors = new Texture2D(32, 32);
        colors.LoadImage(File.ReadAllBytes("Assets/Texture/Terrain/StateColor.png"));
        string[] names = File.ReadAllLines("Assets/Textes/States.txt");
        List<Color> normcolor = new List<Color>();
        for (int i = 0; i < 20; i++)
            for (int j = 0; j < 32; j++)
                if (!normcolor.Contains(colors.GetPixel(j, i)))
                    normcolor.Add(colors.GetPixel(j, i));
        int n = normcolor.Count;
        int[] idstate = new int[n];
        for (int i = 0; i < n; i++) idstate[i] = i;
        for (int i = n - 1, j; i > 1; i--)
        {
            j = Random.Range(0, i);
            int a = idstate[i];
            idstate[i] = idstate[j];
            idstate[j] = a;
        }
        for (int i = 0; i < states.Count; i++)
        {

            states[i].mainColor = normcolor[idstate[i]];
            states[i].name = names[idstate[i]];
            states[i].flag = new Texture2D(128, 128);
            states[i].flag.LoadImage(File.ReadAllBytes("Assets/Texture/flags/(" + idstate[i] + ").png"));
            if (states[i].reg.Count > 0)
                states[i].Capital = states[i].reg[0];
            else Debug.LogErrorFormat("states[{0}] without regions!", i);

            states[i].fraction = (FractionName)(i % 2);
        }
    }
   static void BFS(int i0, List<int>st, int size)
    {
        Queue<int> q = new Queue<int>();
        q.Enqueue(i0);
        pused[i0] = true;
        st.Add(i0);
        int s = 1;
        while (q.Count > 0)
        {
            int i = q.Dequeue();

            foreach (Region x in reg[i].neib)
                if (!pused[x.id])
                {
                    pused[x.id] = true;
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
        return ground[y0 * w + x0] > Height && ground[y1 * w + x1] > Height ||
            ground[y0 * w + x0] <= Height && ground[y1 * w + x1] <= Height;
    }
    static bool Gran(int y,int x)
    {
        return 0 <= y && y < h && 0 <= x && x < w;
    }
    static void CreateDiplomacy()
    {
        for (int i = 0; i < states.Count; i++)
            for (int j = i + 1; j < states.Count; j++)
            {
                Diplomacy dip = new Diplomacy(states[i], states[j]);
                dip.alliance = Random.value > 0.66f;
                dip.war = dip.alliance ? false : Random.value > 0.5f;
                dip.forceaccess = dip.war ^ dip.alliance ? false : Random.value > 0.5f;
                states[i].diplomacy.Add(dip);
                states[j].diplomacy.Add(dip);
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