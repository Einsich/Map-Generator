using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public enum NoiseType
{
    PerlinNoise,
    ContinentAlgorithm,
    SquareAndDiamond
}
public class MyNoise :MonoBehaviour {

    public static byte[] GetMap(int height,int widht,int seed, float watering,NoiseType type)
    {
        byte[] r;
        water = watering;
        MyNoise.seed = seed;
        switch (type)
        {
            case NoiseType.ContinentAlgorithm: r = GetHeightArray(height, widht); break;
            case NoiseType.SquareAndDiamond: r = FractalNoise(height, widht); break;
            default: r = GetNoise(height, widht,seed);break;
        }
        return r;
    }
   static int seed;
   static Vector2 GetNorm(float x,float y)
    {
        int v = (int)((((int)x * (1836311903 + seed)) ^ ((int)y * (2971215073 + seed)) + 4807526976))%113;
        v = v & 15;

        switch (v)
        {
            case 0: return new Vector2 ( 1, 0 );
            case 1: return new Vector2(0, 1);
            case 2: return new Vector2(-1, 0);
            case 3: return new Vector2(0, -1);
            case 4: return new Vector2(0.7f, 0.7f);
            case 5: return new Vector2(0.7f, -0.7f);
            case 6: return new Vector2(-0.7f, 0.7f);
            case 7: return new Vector2(-0.7f, -0.7f);
            case 8: return new Vector2(0.86f, 0.5f);
            case 9: return new Vector2(0.86f,-0.5f);
            case 10: return new Vector2(-0.86f, 0.5f);
            case 11: return new Vector2(-0.86f, -0.5f);
            case 12: return new Vector2(0.5f, 0.86f);
            case 13: return new Vector2(0.5f, -0.86f);
            case 14: return new Vector2(-0.5f, 0.86f);
            default: return new Vector2(-0.5f, -0.86f);
        }
    }
    static float Curve(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
    static float GetNoiseIn(float x0,float y0)
    {
        float x = Mathf.Floor(x0), y = Mathf.Floor(y0);
        Vector2 a = GetNorm(x, y);
        Vector2 b = GetNorm(x+1, y);
        Vector2 c = GetNorm(x, y+1);
        Vector2 d = GetNorm(x + 1, y + 1);
        x = x0 - x;
        y = y0 - y;
        float tx1 = Vector2.Dot(new Vector2(x, y), a);
        float tx2 = Vector2.Dot(new Vector2(x - 1, y), b);
        float bx1 = Vector2.Dot(new Vector2(x, y - 1), c);
        float bx2 = Vector2.Dot(new Vector2(x - 1, y - 1), d);

        x = Curve(x); y = Curve(y);
        
        float tx = Lerp(tx1, tx2, x);
        float bx = Lerp(bx1, bx2, x);
        float tb = Lerp(tx, bx, y);

        return tb;
    }
    public static byte[] GetNoise(int n, int m,int seed)
    {
        MyNoise.seed = seed;
        float x, y;
       int  oct = 6;
        byte[] r = new byte[n * m];
        float[] k = new float[oct];
        k[0] = 1;
        for (int i = 1; i < oct; i++)
            k[i] = k[i - 1] * 2;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
            {
                int d = i * m + j;
                float am = 1, max = 0;
                float a = 0;
                for (int l = 0; l < oct; l++)
                {
                    max += am/k[l];
                    y = 1f * i / 250;
                    x = 1f * j / 250;
                    a += GetNoiseIn(x*k[l], y*k[l])*am;

                }
                a = (a/ max + 1) / 2;
                r[d] = (byte)(Mathf.Clamp(a * 255, 0, 255));

               
            }
        
        return r;
    }
    static void SmoothWater(byte[] r,int n,int m,byte sea)
    {
        Vector2Int p = Vector2Int.zero,pd;
        for (p.y = 0; p.y < n; p.y++)
            for (p.x = 0; p.x < m; p.x++)
                if(r[p.y * m+ p.x] <=sea)
                    foreach (var d in D)
                    {
                        pd = p + d;
                        if (0 <= pd.x && pd.x < m && 0 <= pd.y && pd.y < n && r[pd.y * m + pd.x] > sea+10 )
                        {
                            r[p.y * m + p.x] = (byte)(sea + 1);
                            break;
                        }
                    }
    }
    static Cell[] cells;
    static List<Plate> plates;
    static float water;
    public static byte[] GetHeightArray(int n, int m)
    {
        Random.InitState(seed);
        int k = n * m;
        cells = new Cell[k];
        for (int i = 0; i < k; i++)
            cells[i] = new Cell();
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                for (int d = 0; d < 4; d++)
                    if (0 <= i + dy[d] && i + dy[d] < n && 0 <= j + dx[d] && j + dx[d] < m)
                    {
                        Cell cell = cells[(i + dy[d]) * m + j + dx[d]];
                        if (cell.neibor.Contains(cells[i * m + j]))
                            continue;
                        cells[i * m + j].neibor.Add(cell);
                        cell.neibor.Add(cells[i * m + j]);
                    }
        plates = new List<Plate>();
        Plate curPlate = new Plate();
        int offset = (int)(k * Random.value);
        for(int i=0;i<k;i++)
        {
            int j = (i ^ 0b101010101010) < k ? i ^ 0b101010101010 : i;
            j = j + offset < k ? j + offset : j + offset - k;
            if (cells[j].plate == null)
            {
                if (AddCells(cells[j], curPlate))
                {
                    plates.Add(curPlate);
                    curPlate = new Plate();
                }
            }
        }
        int watermater = (int)(plates.Count * water);
        if (watermater == plates.Count) watermater--;
        Debug.Log(watermater + " / " + plates.Count);
        for (int i = 0; i < plates.Count; i++)
            plates[i].SetWaterLevel(i+1 < watermater);
        CalculateHeight();



        byte[] heights = new byte[k];
        for (int i = 0; i < k; i++)
            heights[i] = (byte)(Mathf.Clamp01(cells[i].targethei)*255);
        SmoothWater(heights, n, m, 127);
        return heights;
    }
    static bool AddCells(Cell start,Plate plate)
    {
        List<Cell> list = new List<Cell>();
        Queue<Cell> queue = new Queue<Cell>();
        start.plate = plate;
        list.Add(start);
        queue.Enqueue(start);
        int count = 1;
        Plate randomNeib = null;
        while (queue.Count > 0)
        {
            Cell cur = queue.Dequeue();
            if (Random.value > 0.3f)
            {
                queue.Enqueue(cur);
                continue;
            }
            list.Add(cur);
            count++;
            if (count > 1000)
                break;
            foreach (var neib in cur.neibor)
                if (neib.plate == null)
                {
                    neib.plate = plate;
                    queue.Enqueue(neib);
                }
                else
                    if (neib.plate != plate)
                    randomNeib = neib.plate;
        }
        foreach (var x in queue)
            x.plate = null;
        bool result = list.Count > 50 || randomNeib == null;
        if (result)
            plate.cells = list;
        else
        {
            foreach (var x in list)
                x.plate = randomNeib;
            randomNeib.cells.AddRange(list);
        }
        return result;
    }

    static void CalculateHeight()
    {
        List<Cell> allBorder = new List<Cell>();
        foreach (var x in plates)
            allBorder.AddRange(x.CalculateBorder());

        Queue<Cell> pq = new Queue<Cell>();
        foreach (var x in allBorder)
            pq.Enqueue(x);
        while(pq.Count>0)
        {
            Cell cur = pq.Dequeue();
            foreach(var x in cur.neibor)
                if(!x.useinmount)
                {
                    x.useinmount = true;
                    x.targethei = cur.targethei;
                    x.t = cur.t - 0.2f;
                    if (x.t >= 0.2f)
                        pq.Enqueue(x);
                }
        }
        foreach (var x in cells)
            x.targethei = Mathf.Lerp(x.plate.height, x.targethei, x.t);
    }
    static float f(float x) => ((-5.33333f * x + 13.3333f) * x - 9.66667f) * x + 2.66667f * x;
    static int[] dx = { 0, 1, 0, -1 };
    static int[] dy = { 1, 0, -1, 0 };
    static Vector2Int[] D = {new Vector2Int(0,1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1,0),
        new Vector2Int(1, 1),new Vector2Int(1,-1),new Vector2Int(-1,1),new Vector2Int(-1,-1) };
    class Cell
    {
        public Plate plate;
        public List<Cell> neibor = new List<Cell>(4);
        public float t = 1,targethei;
        public bool useinmount;
        public bool BorderCell()
        {
            foreach (var x in neibor)
                if (x.plate != plate)
                    return true;
            return false;
        }
    }
    class Plate
    {
        public List<Cell> cells;
        public byte hei { get; private set; }
        public bool ocean;
        public float height => hei / 255f;
        public float addhei(Plate other)
        {
            int hash = cells.Count + other.cells.Count;
            hash &= 7;
            if (other.ocean && ocean && hash >= 3)
                hash -= 3;
            return (hash - 3) * 0.1f;
        }
        public void SetWaterLevel(bool ocean)
        {
            this.ocean = ocean;
            float d = ocean ? -0.01f : 0.01f;
            hei = (byte)((Random.value * d + 0.5f) * 255f);
            foreach (var x in cells)
                x.targethei = height;
        }
        public List<Cell> CalculateBorder()
        {
            List<Cell> ans = new List<Cell>();
            foreach(var cell in cells)
                if(cell.BorderCell())
                {
                    float st = 0;
                    bool hit = false;
                    foreach (Cell neib in cell.neibor)
                        if (neib.plate != cell.plate)
                        {
                            cell.useinmount = true;
                            hit = true;
                            st += cell.plate.addhei(neib.plate);
                        }
                    if (hit)
                    {
                        cell.targethei = Mathf.Clamp01(cell.targethei +  st);
                        cell.t = 1;
                        ans.Add(cell);
                    }
                }
            return ans;
        }
    }
    static float R,maxH=1f;
    static void For(float[,]hm,int L,int l,int d,int y0,int x0,int[]dx,int[]dy)
    {
        for (int i = 0, y; (y = y0 + i * d) < L; i++)
            for (int j = 0, x; (x = x0 + j * d) < L; j++)
            {
                float h = 0;
                for (int k = 0,X,Y; k < 4; k++)
                {
                    X = x + dx[k] * l;
                    Y = y + dy[k] * l;
                    h += 0 <= X && X < L && 0 <= Y && Y < L ? hm[Y, X] : 0.5f;
                }
                h = 0.25f * h + l * R * (Random.value-0.5f);
                hm[y, x] = h ;
                maxH = Mathf.Max(hm[y, x], maxH);
            }
    }
    static byte[]FractalNoise(int n,int m)
    {
        int L = 2;
        while (L < Mathf.Max(n, m))
            L = L * 2 - 1;
        R = 32f / L;
        float[,] hm = new float[L, L];
        hm[0, 0] = hm[0, L - 1] = hm[L - 1, 0] = hm[L - 1, L - 1] = 0.5f;
        int l = L / 2, d = L - 1 ;
        int[] dx1 = { 1, 1, -1, -1 }, dx2 = { 1, 0, -1, 0 };
        int[] dy1 = { 1, -1, 1, -1 }, dy2 = { 0, 1, 0, -1 };
        while (l>0)
        {
            For(hm, L, l, d, l, l, dx1, dy1);
            For(hm, L, l, d, 0, l, dx2, dy2);
            For(hm, L, l, d, l, 0, dx2, dy2);
            d = l;
            l /= 2;
        }
        float q = 1f * Mathf.Max(n, m) / L;
        byte[] ans = new byte[n * m];
        maxH = 1f / (maxH*maxH);
        for (int i = 0, y; (y = (int)(i * q)) < n; i++)
            for (int j = 0, x; (x = (int)(j * q)) < m; j++)
                ans[y * m + x] = (byte)(Mathf.Clamp01(hm[i, j]* hm[i, j] * maxH)*255);
        
        return ans;
    }
}

