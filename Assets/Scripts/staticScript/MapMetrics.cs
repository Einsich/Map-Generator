using System;
using System.Collections.Generic;
using UnityEngine;

static class MapMetrics
{
    public const int CustomStateCount = 400;
    public const int Tile = 100;
    public const int TextureStateSize = 32;
    public const float cellPerturbStrength = 0.5f;
    public const float noiseScale = 0.3f;
    public const float MaxHeight = 5;
    public const float OctoScale = 1f / 255;
    public static int SizeN, SizeM;
    static Texture2D HeightTexture;
    static byte[] heights;
    public static float SeaLevel;
    public static int[,] cellStateIndex;
    public static List<Region> regions;
    public static Texture2D map,noise,occupemap,provincemap;
    public static int[] Odx = { 1, 1, 0, -1, -1, -1, 0, 1 },
                        Ody = { 0, -1, -1, -1, 0, 1, 1, 1 },
                        Qdx = { 1, 0, -1, 0 },Qdy = { 0, -1, 0, 1 };
    public static Vector2Int[] Qd = { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
    public static Vector2Int[] OctoDelta = { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1),
    new Vector2Int(1,1),new Vector2Int(1,-1),new Vector2Int(-1,1),new Vector2Int(-1,-1)};
    static Texture2D splatmap;
    static sbyte[,] SplatState;
    public static void SetSplatMap(Texture2D texture)
    {
        splatmap = texture;
        SplatState = new sbyte[splatmap.width, splatmap.height];
    }
    public static Texture2D GetSplatMap() => splatmap;
    public static Texture2D GetHeightMap => HeightTexture;
    public static void SetHeights(byte[] height,int n,int m)
    {
        heights = height;
        HeightTexture = new Texture2D(m, n);
        HeightTexture.wrapMode = TextureWrapMode.Clamp;
        Color[] colors = new Color[n * m];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                colors[i * m + j] = Color.white * (height[i * m + j] / 255f);
        HeightTexture.SetPixels(colors);
        HeightTexture.Apply();
    }
    public static byte[] GetHeightsArray()
    {
        return heights;
    }
    public static int Index(int i, int j)
    {
        return (i * SizeM + j);
    }

    public static bool InsideMap(int i, int j)
    {
        return 0 <= i && i < SizeN && 0 <= j && j < SizeM;
    }

    public static Vector4 SampleNoise(Vector3 position)
    {
        return noise.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
    }

    public static Vector3 Perturb(Vector3 position,Vector2Int dir = default)
    {
        Vector4 sample = SampleNoise(position);
        if (dir == default)
        {
            position.x += (sample.x * 2f - 1f) * cellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * cellPerturbStrength;
        }
        else
        {
            position.x += (sample.x+0.2f) * dir.x * 0.4f * cellPerturbStrength;
            position.z += (sample.z+0.2f) * dir.y * 0.4f * cellPerturbStrength;
        }
        return position;
    }
    public static int[,] dx = { { 1, 1, 0, 0 }, { 1, 1, 0, 0 }, { 0, 0, -1, -1 }, { 0, 0, -1, -1 } };
    public static int[,] dy = { { 1, 0, 0, 1 }, { 0, -1, -1, 0 }, { 0, -1, -1, 0 }, { 1, 0, 0, 1 } };
    
    static Vector3 half = new Vector3(0.5f, 0, 0.5f);
    public static float Height(float x, float y, bool iswater = false) => Height(new Vector2(x, y), iswater);
    public static float Height(Vector2 p, bool iswater = false)
    {
        float h = HeightTexture.GetPixelBilinear(p.x/SizeM, p.y/SizeN).r * MaxHeight;
        return (iswater && h < SeaLevel) ? SeaLevel : h;
    }
    public static Vector3 GetCellPosition(Vector2Int p, bool iswater = false)
    {
        return GetCellPosition(p.y, p.x,iswater);
    }
    public static Vector3 GetCellPosition(int y,int x, bool iswater = false)
    {
        return new Vector3(x, Height(new Vector2(x, y) + Vector2.one * 0.5f, iswater), y) + half;
    }
    public static Region GetRegion(Vector2Int p)
    {
        return GetRegion(p.y, p.x);
    }
    public static Region GetRegion(int y,int x)
    {
        return InsideMap(y, x) ? regions[cellStateIndex[y, x]] : null ;
    }

    public static Vector3 GetPosition(Vector2 p,bool iswater=false)
    {
        return new Vector3(p.x, Height(p,iswater), p.y);
    }
     public static Vector3 GetCornerPosition(Vector2Int p,bool iswater=false)
    {
        return GetCornerPosition(p.y, p.x, iswater);
    }
    public static Vector3 GetCornerPosition(int i,int j, bool iswater = false)
    {
        Vector2Int dir;
        Vector2 p = new Vector2(j, i);
        Vector3 r = !Main.Angle(i, j, out dir)? Perturb(GetPosition(p, iswater)): Perturb(GetPosition(p,  iswater), dir);

        return r;
    }
    public static Color GetColor(int i, int j)
    {
        return regions[cellStateIndex[i, j]].owner!=null? regions[cellStateIndex[i, j]].owner.mainColor : Color.clear;
    }
    public static Color GetOccupedColor(int i, int j)
    {
        if (regions[cellStateIndex[i, j]].ocptby != null)
            return regions[cellStateIndex[i, j]].ocptby.mainColor;
        else
            return new Color(0, 0, 0, 0);
    }
    //public static int GetTextureInd(int i,int j)
    //{
      // return regions[cellStateIndex[i, j]].owner.originalId;
    //}

    public static void UpdateColorMap()
    {
        for (int i = 0; i < SizeN; i++)
            for (int j = 0; j < SizeM; j++)
                map.SetPixel(j, i, GetColor(i, j));
        map.Apply();
    }
    public static void UpdateOccupedMap()
    {
        for (int i = 0; i < SizeN; i++)
            for (int j = 0; j < SizeM; j++)
                occupemap.SetPixel(j, i, GetOccupedColor(i, j));
        occupemap.Apply();
    }
    static Color[] SplatColor = { Color.red, Color.green, Color.blue };
    public static bool Visionable(Vector2Int p) => SplatState[p.x, p.y] > 0;
    public static void SetRegionSplatState(List<Vector2Int> l,LandShowMode ind)
    {
        for (int i = 0; i < l.Count; i++)
        {
            switch (ind)
            {

                case LandShowMode.TerraIncognito: SplatState[l[i].x, l[i].y] = -1;break;
                case LandShowMode.Visible:if (SplatState[l[i].x, l[i].y] <= 0)
                        SplatState[l[i].x, l[i].y] = 1;
                    else
                        SplatState[l[i].x, l[i].y]++;
                    break;
                case LandShowMode.ForOfWar: if (SplatState[l[i].x, l[i].y] <= 0)
                        SplatState[l[i].x, l[i].y] = 0;
                    else
                        SplatState[l[i].x, l[i].y]--;
                    break;
            }
            int k = SplatState[l[i].x, l[i].y];
            k = k < 0 ? 1 : k == 0 ? 0 : 2;
            splatmap.SetPixel(l[i].x, l[i].y, SplatColor[k]);
        }
    }
    public static void UpdateAgentVision(Vector2Int old, Vector2Int cur, float r ,int firstly = 0)
    {
        int xMax = Math.Max(old.x, cur.x) + (int)r;
        int yMax = Math.Max(old.y, cur.y) + (int)r;
        int xMin = Math.Min(old.x, cur.x) - (int)r;
        int yMin = Math.Min(old.y, cur.y) - (int)r;
        xMax = xMax >= SizeM ? SizeM - 1 : xMax;
        yMax = yMax >= SizeN ? SizeN - 1 : yMax;
        xMin = xMin < 0 ? 0 : xMin;
        yMin = yMin < 0 ? 0 : yMin;
        r *= r;
        for(int x = xMin;x<=xMax;x++)
            for(int y = yMin;y<=yMax;y++)
            {
                bool inold = (old.x - x) * (old.x - x) + (old.y - y) * (old.y - y) <= r;
                bool incur = (cur.x - x) * (cur.x - x) + (cur.y - y) * (cur.y - y) <= r;
                if(firstly ==1)
                {
                    if(incur)
                    {
                        if (SplatState[x, y] < 0)
                            SplatState[x, y] = 0;
                        SplatState[x, y]++;
                        splatmap.SetPixel(x, y, SplatColor[2]);
                    }
                } else
                if(firstly ==-1)
                {
                    if (inold)
                    {
                        SplatState[x, y]--;
                        int k = SplatState[x, y] > 0 ? 2 : 0;
                        splatmap.SetPixel(x, y, SplatColor[k]);
                    }
                } else
                {
                    if(inold != incur)
                    {
                        if(incur)
                        {
                            if (SplatState[x, y] < 0)
                                SplatState[x, y] = 0;
                            SplatState[x, y]++;
                        } else
                        {
                            SplatState[x, y]--;
                        }
                        int k = SplatState[x, y] > 0 ? 2 : 0;
                        splatmap.SetPixel(x, y, SplatColor[k]);
                    }
                }
            }
    }
    public static void UpdateSplatMap()
    {
        splatmap.Apply();
    }
    public static Color GetRegionColor(Region reg)
    {
        int c = cellStateIndex[reg.Capital.y, reg.Capital.x];
        int r = c % 256, g = (c / 256);
        return new Color(r * OctoScale, g * OctoScale, 0);
    }
    public static void UpdateProvincesMap()
    {
        Color[] color = new Color[SizeN * SizeM];
        for (int i = 0; i < SizeN; i++)
            for (int j = 0; j < SizeM; j++)
            {
                int c = cellStateIndex[i, j];
                int r = c % 256, g = (c / 256);
                color[i * SizeM + j] = new Color(r * OctoScale, g * OctoScale, 0);
            }
        provincemap.filterMode = FilterMode.Point;
        provincemap.wrapMode = TextureWrapMode.Clamp;
        provincemap.SetPixels(color);
        provincemap.Apply();
    }
    public static void UpdateTree(GameObject tree)
    {
        int x = Mathf.Clamp((int)tree.transform.position.x, 0, SizeM - 1);
        int y = Mathf.Clamp((int)tree.transform.position.z, 0, SizeN - 1);
        tree.SetActive(splatmap.GetPixel(x, y).g < 1);
           
    }
    public static void CalculLCR(List<Vector2Int> ter,out Vector2Int left,out Vector2Int center, out Vector2Int right)
    {
        if (ter.Count == 1)
        {
            center = ter[0];
            left = center + Vector2Int.left * 4;
            right = center + Vector2Int.right * 4;
            return;
        }
        if(ter.Count==2)
        {
            ter[1] = ter[1];
            center = ter[0] + ter[1];
            center.x /= 2;center.y /= 2;
            if(ter[0].x<ter[1].x)
            {
                left = ter[0];
                right = ter[1];
            }
            else
            {
                left = ter[1];
                right = ter[0];
            }
            return;
        }
         float best = 0;
        center = ter[0];

        for (int i = 0; i < ter.Count; i++)
        {
            float d = 0;
            for (int j = 0; j < ter.Count; j++)
                if (i != j)
                {
                    d += 100 / (ter[i] - ter[j]).sqrMagnitude;
                }
            if (best < d)
            {
                best = d;
                center = ter[i];
            }
        }
        float maxd = 0,maxd1=0;
        left =  Vector2Int.left * 3;
        right =  Vector2Int.right * 3;
        Vector2Int L1=left, R1=right;
        for (int i = 0; i < ter.Count; i++)
            for (int j = i + 1; j < ter.Count; j++)
            {
                    float d = (ter[i] - ter[j]).magnitude;
                if (center != ter[i] && center != ter[j])
                {
                    Vector2Int a = ter[i] - center, b = ter[j] - center;
                    if (Vector2.Angle(a, b) > 90)
                    {
                        
                        if (d > maxd)
                        {
                            maxd = d;
                            if (ter[i].x < ter[j].x)
                            {
                                left = a; right = b;
                            }
                            else
                            {
                                left = b; right = a;
                            }
                        }
                    }
                    
                   
                }
                if (d > maxd1)
                {
                    maxd1 = d;
                    if (ter[i].x < ter[j].x)
                    {
                        L1 = ter[i]; R1 = ter[j];
                    }
                    else
                    {
                        R1 = ter[i]; L1 = ter[j];
                    }
                }
            }
        int S = left.x * right.y - left.y * right.x;
            left += center;
            right += center;
        if (maxd<1)
        {
            left = L1;
            right = R1;
            center = (left+right);
            center.x /= 2;center.y /= 2;
            return;
        }
        Vector2Int n = right - left;
        float h = Mathf.Abs(S) / n.magnitude;
        Vector2 norm = new Vector2(n.y, -n.x).normalized * h;
        
        int S0 = (left - center).x * (right - center).y - (left - center).y * (right - center).x;
        center = new Vector2Int((left + right).x / 2 - (int)norm.x, (left + right).y / 2 - (int)norm.y);
        int S1 = (left - center).x * (right - center).y - (left - center).y * (right - center).x;
        if ((S0 > 0) != (S1 > 0))
            center = new Vector2Int(center.x + (int)(norm.x * 2), center.y + (int)(norm.y * 2));

    }
}
public enum LandShowMode
{
    ForOfWar,TerraIncognito,Visible
}
public enum Direction
{
    UR,DR,DL,UL
}
public static class ListPool<T>
{
    static Stack<List<T>> stack = new Stack<List<T>>();
    public static List<T> Get()
    {
        if (stack.Count > 0)
        {
            return stack.Pop();
        }
        return new List<T>();
    }
    public static void Add(List<T> list)
    {
        list.Clear();
        stack.Push(list);
    }
}