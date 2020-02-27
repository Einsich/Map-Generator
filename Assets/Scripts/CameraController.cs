using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public static List<State> state;
    public static List<Region> regions;
    public static bool locked;
    public static float minh = 5;
    public static float maxh = 150;
    public static float minV = 20;
    public static float maxV = 100;
    public static float Hshowstate = 40;
    public static void SetPosition(Vector3 v)
    {
        t = 0.1f;
        p0 = v;
        Camera.main.transform.position = target = v + Position(t);
        Camera.main.transform.rotation = Quaternion.Euler(Angle(t), 0, 0);
        showstate = Mathf.Lerp(minh, maxh, t) > Hshowstate;
        locked = false;
        ChangeShowState();
    }
    static Vector3 Position(float t)
    {
        t = 0.5f + t*t * 10f;
        float h = MapMetrics.Height(target.x, target.z,true);
        return new Vector3(0, t * t + h, -t);
    }
    static float Angle(float t)
    {
        t = 0.5f + t*0.7f ;
        return Mathf.Atan(2*t)*180f/Mathf.PI;
    }
    public static void ChangeShowState()
    {

        Main.instance.terrainMat.SetFloat("_BorderMod", showstate?1:0);
        for (int i = 0; i < state.Count; i++)
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
                Main.instance.Trees.gameObject.SetActive(false);
            else
            {
                Main.instance.Trees.gameObject.SetActive(true);
                foreach (Transform tree in Main.instance.Trees)
                    MapMetrics.UpdateTree(tree.gameObject);
            }

    }
    public static Vector3 p0;
    public static Vector3 target;
    public static float t;
    public static bool showstate,cheat=false;
    private void FixedUpdate()
    {
        if (locked)
            return;
        float d = Input.GetAxis("Mouse ScrollWheel")*0.7f;
        t = Mathf.Clamp(t - d, 0, 1);
        if (!cheat)
        {
            Vector3 delta = Input.GetAxis("Vertical") * Vector3.forward + Input.GetAxis("Horizontal") * Vector3.right;
            if (delta.sqrMagnitude > 1)
                delta.Normalize();
            p0 += delta * Time.deltaTime * Mathf.Lerp(minV, maxV, t);
        }
        p0 = ClampV3(p0);
        //target = p0 + new Vector3(0, Mathf.Lerp(minh, maxh, t), 0);
        target = p0 + Position(t) ;
        float timeT = Mathf.Clamp01(Time.deltaTime * 20);
        transform.position = Vector3.Lerp(transform.position, target, timeT);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(Angle(t), 0, 0), timeT);
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
        v.z = Mathf.Clamp(v.z, 0, MapMetrics.SizeN+1);
        v.x = Mathf.Clamp(v.x, 1, MapMetrics.SizeM-1);
        return v;
    }
}
