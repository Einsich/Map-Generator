using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Wizard : ScriptableWizard
{
    public Texture2D res;
    public Texture2D t;
    void OnWizardCreate()
    {
        for (int i = 0; i < t.height; i++)
            for (int j = 0; j < t.width; j++)
                t.SetPixel(j, i, new Color(0, 0, 0, 1 - t.GetPixel(j, i).a));
        t.Apply();
        return;
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
        int n = 1, l = t.width; 
        Texture2DArray textureNormArray = new Texture2DArray(l, l, n, t.format, t.mipmapCount>1);
        textureNormArray.anisoLevel = t.anisoLevel;
        textureNormArray.filterMode = t.filterMode;
        textureNormArray.wrapMode = t.wrapMode;
        
        int[] q = {0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1 };
        int[] x = {3, 3, 2, 1, 0, 0, 2, 3, 2, 2, 3, 1, 2, 3 };
        int[] y = {3, 3, 3, 0, 0, 3, 0, 0, 2, 1, 2, 1, 1, 1 };

        int[] xn = { 0 };// {3, 0, 0, 1, 1, 0, 2, 3, 3, 2, 1, 1, 1, 1 };
        int[] yn = { 0 };// {3, 3, 3, 0, 0, 0, 0, 0, 0, 1, 2, 2, 2, 2 };
        
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
    public Texture2D colors;
    void OnWizardCreate()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Flag", "Flag", "asset", "Save Flag"
        );
        if (path.Length == 0 || colors == null)
        {
            return;
        }
        List<Color> normcolor = new List<Color>();
        for (int i = 0; i < 25; i++)
            for (int j = 0; j < 32; j++)
                if (!normcolor.Contains(colors.GetPixel(j, i)))
                    normcolor.Add(colors.GetPixel(j, i));
        string s = "";
        foreach (var x in normcolor)
            s += $"{x.r} {x.g} {x.b} ";
        TextAsset asset = new TextAsset(s);
        AssetDatabase.CreateAsset(asset, path);
    }
    [MenuItem("Assets/Create Flag")]
    static void CreateWizard()
    {
        DisplayWizard<WizardFlag>("Create Flag", "Create");
    }

}