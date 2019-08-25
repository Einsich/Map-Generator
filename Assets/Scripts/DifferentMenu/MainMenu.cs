using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

using static StaticFunction.staticFunc;
public class MainMenu : MonoBehaviour {

    public GameObject backGround, MainPanel, GeneratorPanel;
    public SaveLoadMenu SaveLoadPanel;
    public Slider Sea, Hei, Wei, See;
    public Image preGenMap;
    static int sealvl, h, w, seed;
    public static MainMenu instance;
    public static bool BackGround
    {
        set => instance.backGround.gameObject.SetActive(value);        
    }
    void Awake()
    {
        instance = this;
        Vector2 resolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        backGround.GetComponent<RectTransform>().sizeDelta = resolution;
       
        GetComponent<CanvasScaler>().referenceResolution = resolution;
        SaveLoadPanel.prevMenu = GeneratorPanel;
    }
    void Start()
    {
        BackGround = true;
    }
    void OnEnable()
    {
        PreGeneratedMap();
    }
    public static void ShowGeneratorMenu()
    {
        instance.GeneratorPanel.SetActive(true);
    }
    public void ExitFromGame()
    {
        Application.Quit();
    }
    public void TransitionMainToGenerator(bool toGen)
    {
        MainPanel.SetActive(!toGen);
        GeneratorPanel.SetActive(toGen);
    }
    public void GenerateMap()
    {
        GetSettings();
        Creator.Create(h, w, seed, (byte)sealvl, hm);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        
    }
    void GetSettings()
    {
        seed = (int)See.value;
        h = MapMetrics.Tile * (int)Hei.value;
        w = MapMetrics.Tile * (int)Wei.value;
        
    }
    NoiseType noiseType;
    public void GenerationMode(Dropdown dropdown)
    {
        NoiseType type = (NoiseType)dropdown.value;
        if(type != noiseType)
        {
            noiseType = type;
            PreGeneratedMap();
        }
    }
    static byte[] hm;
    public void PreGeneratedMap()
    {
        GetSettings();
        hm = MyNoise.GetMap(h, w, seed, Sea.value / Sea.maxValue, noiseType);
        ShowHeightMap();
    }

     
    public void ShowHeightMap()
    {
        if (noiseType == NoiseType.ContinentAlgorithm)
            hm = MyNoise.GetMap(h, w, seed, Sea.value / Sea.maxValue, noiseType);
        switch (noiseType)
        {
            case NoiseType.ContinentAlgorithm: sealvl = 127; break;
            default: sealvl = (byte)(255 * Sea.value / Sea.maxValue); break;
        }
        
        preGenMap.sprite = CreateMiniMap(seaH:(byte)(sealvl),mapH:hm,w:w,h:h);
    }
    public static Sprite MiniMap;
    public static Sprite CreateMiniMap(byte seaH,byte[] mapH,int w, int h)
      {

        Texture2D minimap = new Texture2D(w, h);
        Color[] colors = new Color[h * w];
        byte skyH=(byte)(255-seaH);
      Color[] ColorPoint=new Color[]  {rgb(55, 133, 255),rgb(95, 165, 255),rgb(148, 192, 255), rgb(150, 180, 130),rgb(150, 180, 130), rgb(187, 166, 40)     ,rgb(187, 138, 95) };
      byte[] ColorHightLine=new byte[]{0              ,(byte)(seaH*0.7),seaH            ,(byte)(seaH + 1) ,(byte)(seaH+skyH*0.1),(byte)(seaH+skyH*0.7),255             }; /*not HotLine*/

        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                colors[i * w + j] = Color.red;
                byte nowH = mapH[i * w + j];
                for (byte line = 0, h1 = ColorHightLine[line], h2 = ColorHightLine[line + 1]; line < ColorPoint.Length - 1; line++, h1 = h2, h2 = ColorHightLine[line + 1])
                {
                    if (nowH <= h2)
                    {
                        float deltaH = h2 - h1;
                        if (deltaH == 0) continue;//deltaH+=1;
                        float proc = (nowH - h1) / deltaH;
                        colors[i * w + j] = Color.Lerp(ColorPoint[line], ColorPoint[line + 1], proc);
                        break;
                    }
                }

            }

      //TODO?:
      minimap.SetPixels(colors);
      minimap.Apply();
      return MiniMap = Sprite.Create(minimap,new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
      }

}
