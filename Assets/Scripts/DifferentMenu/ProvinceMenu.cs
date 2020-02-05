using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvinceMenu : MonoBehaviour {

    public Text Name;
    public Text gold, manPower, wood, iron, science, Info;
    static ProvinceMenu instance;
    public static Region current;
    public Texture2D[] build;
    public Sprite[] buildState;
    public static Sprite GetBuildState(int index) => instance.buildState[index];
    public Sprite[,] BuildingSprite;
    public GameObject[] specialBuildings;
    public static GameObject GetSpecialBuilding(int index) => instance.specialBuildings[index];
    public BuildingButtonInterface[] buildings;
    public GameObject buildPanel, diplomPanel;
    //public Sprite[] pips;
   // public static Sprite GetPips(int index) => instance.pips[index];
    public RecruitMenu recruitMenu;
    [Header("diplomacy, buildings, army, navy")]
    public Button[] wievButtons;
    public ButtonSelector buttonSelector;
    static int height;
    static int[] dx = { 0, 1, 2, 4, 5, 7, 11, 13, 14, 15, 16, 12 };
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        height = build[0].height;
        for (int i = 0; i < ProvinceData.specialCount; i++)
            buildings[i].ind = i;
        BuildingSprite = new Sprite[2, dx.Length];
        for (int frac = 0; frac < 2; frac++)
            for (int i = 0; i < dx.Length; i++)
                BuildingSprite[frac,i] = Sprite.Create(build[frac], new Rect(height * dx[i], 0, height, height), new Vector2(0.5f, 0.5f));
        
        buttonSelector = new ButtonSelector(wievButtons, new ShowSmth[] { ShowDiplomacy, ShowBuildings, ShowArmy, ShowNavy });
        gameObject.SetActive(false);
    }
    public static bool needUpdate;
     void Update()
    {
        if (needUpdate)
            ShowProvinceMenu(current);
        needUpdate = false;
    }
    public void ShowProvinceMenu(Region r)
    {
        if (r != null)
            current = r;
        else
            r = current;
        if (r == null) return;
        gameObject.SetActive(true);
        MenuManager.CheckExchangeRegiment();
        Name.text = r.name + " (" + r.id + ") ";

        Info.text = string.Format("distance = {0:N2}\ndist coef = {1:N2}\norder coef = {2:N2}\ntrader impact = {3:N2}",
            Mathf.Sqrt(r.sqrDistanceToCapital), r.data.IncomeCoefFromDistance(), r.data.IncomeCoefFromOrder(),r.data.traderImpact);
        Treasury t = r.data.income;
        Treasury tc = r.data.incomeclear;
        gold.text = string.Format("{0:N1}\n({1:N1})", t.Gold, tc.Gold);
        manPower.text = string.Format("{0:N1}\n({1:N1})", t.Manpower, tc.Manpower);
        wood.text = string.Format("{0:N1}\n({1:N1})", t.Wood, tc.Wood);
        iron.text = string.Format("{0:N1}\n({1:N1})", t.Iron, tc.Iron); 
        science.text = string.Format("{0:N1}\n({1:N1})", t.Science, tc.Science); 

        wievButtons[0].gameObject.SetActive(r.owner != Player.curPlayer);

        if (r.owner == Player.curPlayer)
            buttonSelector.Hidden(0);
        buttonSelector.Update();
        recruitMenu.UpdateGarnison();
    }

    public void HiddenProvinceMenu()
    {
        gameObject.SetActive(false);
        MenuManager.CheckExchangeRegiment();
    }
    public void ShowDiplomacy(bool show)
    {

        diplomPanel.SetActive(show);
        if(show)
        diplomPanel.GetComponent<DiplomacyMenu>().SchowDiplomacy(Player.curPlayer, current.owner);
    }
    public void ShowBuildings(bool show)
    {
        buildPanel.SetActive(show);
        if (!show)
            return;
        int frac = (int)current.owner.fraction;

        for (int i = 0; i < ProvinceData.specialCount; i++)
            buildings[i].SetImage(BuildingSprite[frac, i],
                current.data.Stateof((Building)i), current.data.buildings[i]);
    }
    public void ShowArmy(bool show)
    {
        recruitMenu.gameObject.SetActive(show);
        if (!show)
            return;
        recruitMenu.ShowRecruitMenu(Player.curRegion);
    }
    public void ShowNavy(bool show)
    {

    }

}
public enum ButtonMode
    {
        Diplomacy,
        Buildings,
        Army,
        Navy
    }