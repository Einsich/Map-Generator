﻿using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapMesh : MonoBehaviour {

    Mesh mapMesh;   
    public static int  tile = MapMetrics.Tile;
    void Awake()
    {
        GetComponent<MeshFilter>().mesh = mapMesh = new Mesh();   
    }

    public void Apply()
    {
       
    }

    public static int[,] regionIndex;
    public static List<Region> regions;
    static int[] dx = MapMetrics.Qdx, dy = MapMetrics.Qdy;
    
    public void TriangulateMap(int x0, int y0)
    {
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>(), uvs1 = new List<Vector2>();
        List<Vector3> vertices = new List<Vector3>();
        for (int i = y0; i < y0 + tile + 1; i++)
            for (int j = x0; j < x0 + tile + 1; j++)
            {
                uvs.Add(new Vector2(1f * j / MapMetrics.SizeM, 1f * i / MapMetrics.SizeN));
                Vector3 v = MapMetrics.PerturbedCorner(new Vector2Int(j, i));
                Navigation.field[i + y0, j + x0] = new Vector2(v.x, v.z);
                vertices.Add(new Vector3(v.x, v.y, v.z));
                uvs1.Add(new Vector2(v.x, v.z));
            }
        for (int i = 0; i < tile; i++)
            for (int j = 0; j < tile; j++)
            {
                int a = i * (tile + 1) + j;
                int b = a + tile + 1;
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(b + 1);

                triangles.Add(b + 1);
                triangles.Add(a + 1);
                triangles.Add(a);
            }
        mapMesh.SetVertices(vertices);
        mapMesh.SetUVs(0, uvs);
        mapMesh.SetUVs(1, uvs1);
        mapMesh.SetTriangles(triangles, 0);
        ListPool<Vector3>.Add(vertices);
        ListPool<int>.Add(triangles);
        ListPool<Vector2>.Add(uvs);
        ListPool<Vector2>.Add(uvs1);
        mapMesh.RecalculateNormals();
        mapMesh.RecalculateBounds();
        GetComponent<MeshCollider>().sharedMesh = mapMesh;
    }
    static Vector2Int rivPoint(Vector2Int p,int d)
    {
        if (d < 0) d = 3;
        if (d > 3) d = 0;
        p.x += dx[d];
        p.y += dy[d];
        return p;
    }
    public static GameObject AddRiver(List<Vector2Int>point)
    {
        List<int> triangles = ListPool<int>.Get();
        List<Vector3> vertices = ListPool<Vector3>.Get();
        List<Vector2> uvs = ListPool<Vector2>.Get();
        
        List<Vector3> bord = ListPool<Vector3>.Get();
        Vector2Int d1, d2;
        d1 = point[1] - point[0];

        bord.Add(MapMetrics.GetCellPosition(point[0]));
        bord.Add(0.5f * (bord[0] + MapMetrics.GetCellPosition(point[1])));
        for(int i=1;i<point.Count-1;i++)
        {
            d2 = point[i + 1] - point[i];
            bord.Add(MapMetrics.GetCellPosition(point[i]));
            if (d1.x*d2.x+d1.y*d2.y==0)
            {
                d1 = d2 - d1;
                bord[bord.Count - 1] += 0.25f * new Vector3(d1.x, 0, d1.y);
            }
            bord.Add(0.5f*(MapMetrics.GetCellPosition(point[i])+ MapMetrics.GetCellPosition(point[i+1])));
            d1 = d2;
        }
        for (int i = 0; i < bord.Count; i++)
        {
            bord[i] += Vector3.up * 0.1f;
        }
        Vector3 dir1, dir2, norm = Vector3.zero;
        float width = 0.1f;

        vertices.Add(bord[0] + norm * width);
        vertices.Add(bord[0] - norm * width);
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        for (int i = 1; i < bord.Count; i++)
        {
            if (i < bord.Count - 1)
            {
                dir1 = bord[i] - bord[i - 1];
                dir2 = bord[i + 1] - bord[i];
                norm = (Vector3.Cross(dir1, Vector3.up) + Vector3.Cross(dir2, Vector3.up)).normalized;
            }
            vertices.Add(bord[i] + norm * width);
            vertices.Add(bord[i] - norm * width);
            uvs.Add(new Vector2(i, 0));
            uvs.Add(new Vector2(i, 1));

            triangles.Add(i * 2);
            triangles.Add(i * 2 - 1);
            triangles.Add(i * 2 - 2);

            triangles.Add(i * 2 + 1);
            triangles.Add(i * 2 - 1);
            triangles.Add(i * 2);
        }

        Mesh mesh;
        GameObject river = new GameObject();
        river.AddComponent<MeshRenderer>();
        river.AddComponent<MeshFilter>().mesh = mesh = new Mesh();

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);

        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        ListPool<Vector3>.Add(vertices);
        ListPool<int>.Add(triangles);
        ListPool<Vector2>.Add(uvs);
        ListPool<Vector2Int>.Add(point);
        ListPool<Vector3>.Add(bord);
        return river;
    }
}
