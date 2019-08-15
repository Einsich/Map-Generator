using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Wizard : ScriptableWizard
{
    public Texture2D res;
    public Texture2D t;
    void OnWizardCreate()
    {
        if ( res==null|| res == null || t==null)
        {
            return;
        }
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Texture Array", "Texture Array", "asset", "Save Texture Array"
        );
        if (path.Length == 0)
        {
            return;
        }
        int n = 14, l = t.width; 
        Texture2DArray textureNormArray = new Texture2DArray(l, l, n, t.format, t.mipmapCount>1);
        textureNormArray.anisoLevel = t.anisoLevel;
        textureNormArray.filterMode = t.filterMode;
        textureNormArray.wrapMode = t.wrapMode;

        int[] q = {0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1 };
        int[] x = {3, 3, 2, 1, 0, 0, 2, 3, 2, 2, 3, 1, 2, 3 };
        int[] y = {3, 3, 3, 0, 0, 3, 0, 0, 2, 1, 2, 1, 1, 1 };

        int[] xn = {3, 0, 0, 1, 1, 0, 2, 3, 3, 2, 1, 1, 1, 1 };
        int[] yn = {3, 3, 3, 0, 0, 0, 0, 0, 0, 1, 2, 2, 2, 2 };
        
        for (int i = 0; i < n; i++)
        {
            Color[] c = res.GetPixels(xn[i] * l, yn[i] * l, l, l);
           //f (i == 0) for (int k = 0; k < c.Length; k++) c[k] = new Color(0, 0, 0, 0);
           // else for (int k = 0; k < c.Length; k++) c[k].a=1;
            t.SetPixels(0, 0, l, l, c);            
            t.Apply();
            for (int m = 0; m < t.mipmapCount; m++)
                Graphics.CopyTexture(t,0,m, textureNormArray, i,m);
            
        }

        AssetDatabase.CreateAsset(textureNormArray, path);
    }
    [MenuItem("Assets/Texture Array")]
    static void CreateWizard()
    {
        DisplayWizard<Wizard>("Create Texture Array", "Create");
    }

}
public class WizardFlag : ScriptableWizard
{
    void OnWizardCreate()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Flag", "Flag", "asset", "Save Flag"
        );
        if (path.Length == 0)
        {
            return;
        }
        float h = 1f, w = 1.4f;
        int n = 2;
        Mesh mesh = new Mesh();
        List<Vector3> ver = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> tri = new List<int>();
        for (int i = 0; i <= n; i++)
            for (int j = 0; j <= n; j++)
            {
                ver.Add(new Vector3(j * w / n, i * h / n));
                uv.Add(new Vector2(j * 1f / n, i * 1f / n));
            }
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                int k = i * (n+1) + j;
                tri.Add(k);  tri.Add(k+1+n+1);tri.Add(k+1); 
                tri.Add(k);  tri.Add(k+1+n);tri.Add(k+1+n+1); 
            }
        mesh.SetVertices(ver);
        mesh.SetUVs(0,uv);
        mesh.SetTriangles(tri,0);
        mesh.RecalculateNormals();
        AssetDatabase.CreateAsset(mesh, path);
    }
    [MenuItem("Assets/Create Flag")]
    static void CreateWizard()
    {
        DisplayWizard<WizardFlag>("Create Flag", "Create");
    }

}