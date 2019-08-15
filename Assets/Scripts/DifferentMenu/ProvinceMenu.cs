using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvinceMenu : MonoBehaviour {

    public Text Name;
    public Text gold, manPower, wood, iron, science;
    public static ProvinceMenu that;
    public static Region current;
    public Texture2D[] build;
    public Sprite[] buildState;
    public Sprite[,] BuildingSprite;
    public GameObject[] specialBuildings;
    public BuildingButtonInterface[] buildings;
    public GameObject buildPanel, diplomPanel;
    public Sprite[] pips;
    public RecruitMenu recruitMenu;
    [Header("diplomacy, buildings, army, navy")]
    public Button[] wievButtons;
    const int ButtonCount = 4;
    public bool[] buttonsSelect ;
    static int height;
    static int[] dx = { 0, 1, 2, 4, 5, 7, 11, 13, 14, 15, 16, 12 };
    private void Awake()
    {
        that = this;
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
        buttonsSelect = new bool[ButtonCount];
        for (int i = 0; i < ButtonCount; i++)
        {
            int k = i;
            wievButtons[i].onClick.AddListener(() => SetMenuMod(k));
        }
        shows = new ShowSmth[] { ShowDiplomacy, ShowBuildings, ShowArmy, ShowNavy };
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
        Name.text = r.name + " (" + r.id + ")";


        Treasury t = r.data.income;
        gold.text = t.Gold.ToString();
        manPower.text = t.Manpower.ToString();
        wood.text = t.Wood.ToString();
        iron.text = t.Iron.ToString();
        science.text = t.Science.ToString();

        wievButtons[0].gameObject.SetActive(r.owner != Player.curPlayer);

        if (r.owner == Player.curPlayer && buttonsSelect[0])
            SetMenuMod(0);
        for (int i = 0; i < ButtonCount; i++)
            if (buttonsSelect[i])
                shows[i](true);
        recruitMenu.UpdateGarnison();
    }

    public void HiddenProvinceMenu()
    {
        gameObject.SetActive(false);
    }
    public void SetMenuMod(int k)
    {
        bool a = buttonsSelect[k];
        for (int i = 0; i < ButtonCount; i++)
            if (buttonsSelect[i])
            {
                SpriteState state = wievButtons[i].spriteState;
                state.highlightedSprite = state.pressedSprite;
                state.pressedSprite = wievButtons[i].image.sprite;
                wievButtons[i].image.sprite = state.highlightedSprite;
                wievButtons[i].spriteState = state;
                buttonsSelect[i] = false;
                shows[i](false);
            }
        if(!a)
        {
            SpriteState state = wievButtons[k].spriteState;
            state.highlightedSprite = state.pressedSprite;
            state.pressedSprite = wievButtons[k].image.sprite;
            wievButtons[k].image.sprite = state.highlightedSprite;
            wievButtons[k].spriteState = state;
            buttonsSelect[k] = true;
            shows[k](true);
        }
    }
    public delegate void ShowSmth(bool show);
    ShowSmth[] shows;
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
        recruitMenu.ShowRecruitMenu(Player.selected);
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