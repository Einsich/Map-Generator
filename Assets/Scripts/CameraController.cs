using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public static List<State> state;
    public static List<Region> regions;
    public static bool locked;
    public static float minh = 5;
    public static float maxh = 150;
    public static float minV = 40;
    public static float maxV = 200;
    public static float minangle = 40;
    public static float maxangle = 80;
    public static float Hshowstate = 40;
    public static void SetPosition(Vector3 v)
    {
        t = 0.5f;
        p0 = v;
        Camera.main.transform.position = target = v + Vector3.up*(minh + maxh) * t;
        Camera.main.transform.rotation = Quaternion.Euler(Mathf.Lerp(minangle,maxangle,t), 0, 0);
        showstate = Mathf.Lerp(minh, maxh, t) > Hshowstate;
        ChangeShowState();
    }
    public static void ChangeShowState()
    {

        Main.instance.terrainMat.SetFloat("_BorderMod", showstate?1:0);
        for (int i = 1; i < state.Count; i++)
        {
            state[i].SetNameStatus(!showstate);
            foreach (Army army in state[i].army)
                if(!army.Fogged)
                army.Active = !showstate;
        }
        for (int i = 0; i < regions.Count; i++)
            regions[i].StateBorder = showstate;
        if (Main.mapMode == MapMode.Terrain)
            if (showstate)
                foreach (Chunk ch in Main.Chunks)
                    ch.Trees.gameObject.SetActive(false);
            else
                foreach (Chunk ch in Main.Chunks)
                {
                    ch.Trees.gameObject.SetActive(true);
                    foreach (Transform tree in ch.Trees)
                        MapMetrics.UpdateTree(tree.gameObject);
                }

    }
    public static Vector3 p0;
    public static Vector3 target;
    public static float t;
    public static bool showstate,cheat=false;
    void Update()
    {
        if (locked)
            return;
        float d = Input.GetAxis("Mouse ScrollWheel") * 0.5f;
        t = Mathf.Clamp(t - d, 0, 1);
        if (!cheat)
        {
            Vector3 delta = Vector3.zero;
            if (Input.GetKey(KeyCode.A))
                delta += Vector3.left;
            if (Input.GetKey(KeyCode.D))
                delta += Vector3.right;
            if (Input.GetKey(KeyCode.W))
                delta += Vector3.forward;
            if (Input.GetKey(KeyCode.S))
                delta += Vector3.back;
            p0 += delta.normalized * Time.deltaTime * Mathf.Lerp(minV, maxV, t);
        }
        p0 = ClampV3(p0);
        target = p0 + new Vector3(0, Mathf.Lerp(minh, maxh, t), 0);
        float timeT = Mathf.Clamp01(Time.deltaTime * 10);
        transform.position = Vector3.Lerp(transform.position, target, timeT);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(Mathf.Lerp(minangle, maxangle, t), 0, 0), timeT);
        
        if (transform.position.y <= Hshowstate && showstate || transform.position.y > Hshowstate && !showstate)
        {
            showstate = transform.position.y > Hshowstate;
            ChangeShowState();
            if (Player.curRegion != null)
                Player.curRegion.Selected = true;
        }
    }
    Vector3 ClampV3(Vector3 v)
    {
        v.z = Mathf.Clamp(v.z, -2, MapMetrics.SizeN+1);
        v.x = Mathf.Clamp(v.x, 1, MapMetrics.SizeM-1);
        return v;
    }
}
