using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

using static StaticFunction.staticFunc;
public class MainMenu : MonoBehaviour {

    public GameObject backGround, MainPanel, GeneratorPanel, SaveLoadPanel,GameMenuPanel;
    public InputField SVInputTExt;
    public Text InputHeader,InputAction;
    public RectTransform listContent;
    public SaveLoadItem itemPrefab;
    public Button BackSVmenu;
    public GameObject[] GameMenu;
    public Slider Sea, Hei, Wei, See;
    public Image preGenMap;
     static bool background;
    static int sealvl, h, w, seed;
    public static MainMenu instance;
    public bool saveMode;
    public bool SaveMode
    { get { return saveMode; } set { saveMode = value; } }
    public static bool BackGround
    {
        get { return background; }
        set
        {
            background = value;
            if (value)
            {
                instance.backGround.transform.GetChild(0).gameObject.SetActive(!Player.play);
                instance.backGround.transform.GetChild(1).gameObject.SetActive(Player.play);
            }else
            {
                instance.backGround.transform.GetChild(0).gameObject.SetActive(false);
                instance.backGround.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }
    void Awake()
    {
        instance = this;
        Vector2 resolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        backGround.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = resolution;
        backGround.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = resolution;
       
        GetComponent<CanvasScaler>().referenceResolution = resolution;
    }
    void Start()
    {
        Player.play = false;
        CameraController.locked = true;
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
    public void TransitionToGameMenu(bool tomenu)
    {
        BackGround = tomenu;
        GameMenuPanel.SetActive(tomenu);
        CameraController.locked = tomenu;
    }
    public void QuitFromGame()
    {
        Player.play = false;
        BackGround = true;
        GameMenuPanel.SetActive(false);
        foreach (GameObject go in GameMenu)
            go.SetActive(false);
        Date.instance.Pause();
        GeneratorPanel.SetActive(true);
    }
    public void BackForSV(GameObject prev)
    {
        SaveLoadPanel.SetActive(false);
        prev.SetActive(true);
    }
    public void GenerateMap()
    {
        GeneratorPanel.SetActive(false);
        foreach (GameObject gm in GameMenu)
            gm.SetActive(true);
        CameraController.locked = false;
        Player.play = true;
        BackGround = false;
        GetSettings();
        Creator.Create(h, w, seed, (byte)sealvl, hm);
    }
    void GetSettings()
    {
        seed = (int)See.value;
        h = MapMetrics.Tile * (int)Hei.value;
        w = MapMetrics.Tile * (int)Wei.value;
        
    }

    public void Action()
    {
        string path = GetSelectedPath();
        if (path == null)
            return;
        if (saveMode)
            Save(path);
        else
        {
            SaveLoadPanel.SetActive(false);
            Load(path);
        }
    }

    public void SelectItem(string name)
    {
        SVInputTExt.text = name;
    }

    void FillList()
    {
        for (int i = 0; i < listContent.childCount; i++)
        {
            Destroy(listContent.GetChild(i).gameObject);
        }

        string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.ram");
        Array.Sort(paths);
        float h = itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
        listContent.sizeDelta = new Vector2(0, paths.Length * h);
        for (int i = 0; i < paths.Length; i++)
        {
            SaveLoadItem item = Instantiate(itemPrefab);
            item.menu = this;
            item.MapName = Path.GetFileNameWithoutExtension(paths[i]);
            item.transform.SetParent(listContent, false);
        }
    }
    public void Save(string path)
    {
        Main.Save(path);
        FillList();
    }

    public void Load(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist " + path);
            return;
        }
        CameraController.locked = false;
        Player.play = true;
        BackGround = false;
        foreach (GameObject gm in GameMenu)
            gm.SetActive(true);

        Main.Load(path);
    }

    public void Delete()
    {
        string path = GetSelectedPath();
        if (path == null)
        {
            return;
        }
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        SVInputTExt.text = "";
        FillList();
    }

    public void OpenSaveLoadPanel(bool saveMode)
    {

        this.saveMode = saveMode;
        SaveLoadPanel.SetActive(true);
        GameObject active = GameMenuPanel.activeSelf ? GameMenuPanel : GeneratorPanel;
        active.SetActive(false);
        BackSVmenu.onClick.RemoveAllListeners();
        BackSVmenu.onClick.AddListener((() => BackForSV(active)));
        if (saveMode)
        {
            InputHeader.text = "Сохранение карты";
            InputAction.text = "Сохранить";
            SVInputTExt.interactable = true;
            SVInputTExt.text = "";
        }
        else
        {
            InputHeader.text = "Загрузка карты";
            InputAction.text = "Загрузить";
            SVInputTExt.interactable = false;
            SVInputTExt.text = "";
        }
        FillList();
    }

    string GetSelectedPath()
    {
        string mapName = SVInputTExt.text;
        if (mapName.Length == 0)
        {
            return null;
        }
        return Path.Combine(Application.persistentDataPath, mapName + ".ram");
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

      for(int i=0; i<h; i++)
      for(int j=0; j<w; j++)
        {
        colors[i*w+j]=Color.red;
        byte nowH=mapH[i*w+j];
        for(byte line=0,h1=ColorHightLine[line],h2=ColorHightLine[line+1]; line<ColorPoint.Length-1; line++,h1=h2,h2=ColorHightLine[line+1])
          {
          if(nowH<=h2)
            {
            float deltaH=h2-h1; 
            if(deltaH==0)continue;//deltaH+=1;
            float proc=(nowH-h1)/deltaH;
            colors[i*w+j]=Color.Lerp(ColorPoint[line],ColorPoint[line+1],proc);
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
