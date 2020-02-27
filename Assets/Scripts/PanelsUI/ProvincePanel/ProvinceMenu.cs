using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvinceMenu : MonoBehaviour {

    public Text Name;
    public Text gold, manPower, wood, iron, science, Info;
    static ProvinceMenu instance;
    public static Region current;
  //  public Sprite[] buildState;
  //  public static Sprite GetBuildState(int index) => instance.buildState[index];
    public GameObject[] specialBuildings;
    public static GameObject GetSpecialBuilding(int index) => instance.specialBuildings[index];
    public GameObject diplomPanel;
    public BuildingPanel buildingPanel;
    //public Sprite[] pips;
   // public static Sprite GetPips(int index) => instance.pips[index];
    public RecruitMenu recruitMenu;
    public ShipBuildPanel shipPanel;
    [Header("diplomacy, buildings, army, navy")]
    public Button[] wievButtons;
    public ButtonSelector buttonSelector;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        buildingPanel = Instantiate(buildingPanel, transform);
         buttonSelector = new ButtonSelector(wievButtons, new ShowSmth[] { ShowDiplomacy, (x)=>buildingPanel.ShowBuildings(x), ShowArmy, ShowNavy });
        gameObject.SetActive(false);
    }
    public void ShowProvinceMenu(Region r)
    {

        if (current != null)
            current.data.SomeChanges -= UpdateMenu;
        current = r;
        if (r == null)
        {
            HiddenProvinceMenu();
        } else
        {
            current.data.SomeChanges += UpdateMenu;
        }
        UpdateMenu();
    }
    public void UpdateMenu()
    { 
        gameObject.SetActive(true);
        MenuManager.CheckExchangeRegiment();
        Name.text = current.name + " (" + current.id + ") ";

        Info.text = string.Format("distance = {0:N2}\ndist coef = {1:N2}\norder coef = {2:N2}\n",
            Mathf.Sqrt(current.sqrDistanceToCapital), current.data.IncomeCoefFromDistance(), current.data.IncomeCoefFromOrder());
        Treasury t = current.data.income;
        Treasury tc = current.data.incomeclear;
        gold.text = string.Format("{0:N1}\n({1:N1})", t.Gold, tc.Gold);
        manPower.text = string.Format("{0:N1}\n({1:N1})", t.Manpower, tc.Manpower);
        wood.text = string.Format("{0:N1}\n({1:N1})", t.Wood, tc.Wood);
        iron.text = string.Format("{0:N1}\n({1:N1})", t.Iron, tc.Iron); 
        science.text = string.Format("{0:N1}\n({1:N1})", t.Science, tc.Science); 

        wievButtons[0].gameObject.SetActive(current.owner != Player.curPlayer);
        wievButtons[3].gameObject.SetActive(current.data.haveFervie);
        if (current.owner == Player.curPlayer)
            buttonSelector.Hidden(0);
        if (!current.data.haveFervie)
            buttonSelector.Hidden(3);

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
    
    public void ShowArmy(bool show)
    {
        recruitMenu.gameObject.SetActive(show);
        if (show)
            recruitMenu.ShowRecruitMenu(Player.curRegion);
    }
    public void ShowNavy(bool show)
    {
        shipPanel.gameObject.SetActive(show);
        if (show)
            shipPanel.Open(current.Port);

    }

}
public enum ButtonMode
    {
        Diplomacy,
        Buildings,
        Army,
        Navy
    }