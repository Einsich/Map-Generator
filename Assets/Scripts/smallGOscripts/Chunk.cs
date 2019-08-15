using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

    public int x0, y0, ChildCountT,ChildIT;
    public MapMesh land;
    public Transform Towns, Ports, Trees,Names;

    public void Triangulate()
    {
        land.x0 = x0;
        land.y0 = y0;
        land.TriangulateMap();
    }
}
