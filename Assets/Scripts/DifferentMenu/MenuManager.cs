using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    public MiniMapPanel miniMap;
    public Date datePanel;
    public SaveLoadMenu saveLoadPanel;
    public MainStatistic statisticPanel;
    public GameMenu gameMenu;
    public CodePanel codePanel;
    //public BattleInterface battlePanel;
    public ProvinceMenu provincePanel;
    public ArmyPanel armyPanel;

   // public BattleResult batResPref;
  //  public static BattleResult battleResultPrefab => instance.batResPref;
    public static Transform MenuTransform => instance.transform;
    static MenuManager instance;
    void Awake()
    {
        instance = this;
        GetComponent<CanvasScaler>().referenceResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        miniMap = Instantiate(miniMap,transform);
        datePanel = Instantiate(datePanel, transform);
        saveLoadPanel = Instantiate(saveLoadPanel, transform);
        statisticPanel = Instantiate(statisticPanel, transform);
        gameMenu = Instantiate(gameMenu, transform);
        codePanel = Instantiate(codePanel, transform);
       // battlePanel = Instantiate(battlePanel, transform);
        provincePanel = Instantiate(provincePanel, transform);
        armyPanel = Instantiate(armyPanel, transform);
        saveLoadPanel.prevMenu = gameMenu.gameObject;
    }
    public static void OpenSaveLoadMenu(bool saveMode)
    {
        instance.saveLoadPanel.OpenSaveLoadPanel(saveMode);
    }
    public static void OpenGameMenu()
    {
        instance.gameMenu.ToGameMenu(true);
    }
    public static void SetMiniMap()
    {
        instance.miniMap.SetMiniMap();
    }
    public static void StartTimer()
    {
        instance.datePanel.StartTimer();
    }
    public static void SetState(State state)
    {
        instance.statisticPanel.SetState(state);
    }
    public static void ShowResources()
    {
        instance.statisticPanel.ShowResources();
    }
    public static void ShowTechnology()
    {
        instance.statisticPanel.ShowTechnology(true);
    }
    public static void HiddenProvinceMenu()
    {
        instance.provincePanel.HiddenProvinceMenu();
    }
    public static void ShowProvinceMenu(Region r)
    {
        instance.provincePanel.ShowProvinceMenu(r);
    }

    public static void CheckExchangeRegiment()
    {
        bool exchange = Player.army != null && Player.army.CanExchangeRegimentWith(Player.curRegion) &&
            instance.armyPanel.gameObject.activeSelf && instance.provincePanel.gameObject.activeSelf;


        instance.armyPanel.exchangeImage.gameObject.SetActive(exchange);
        GarnisonIcon.canExchange = exchange;

    }
    //public static void ShowBattle(Battle battle)
    //{
    //    instance.battlePanel.ShowBattle(battle);
    //}
    public static void UpdateArmyList()
    {
        instance.armyPanel.UpdateArmy();
    }
    public static void UpdateGarnisonList()
    {
        instance.provincePanel.recruitMenu.UpdateGarnison();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            CameraController.cheat = Date.cheat = instance.codePanel.activepanel = !instance.codePanel.activepanel;
            instance.codePanel.Active();
        }
    }
}
