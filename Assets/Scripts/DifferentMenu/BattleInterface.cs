using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleInterface : MonoBehaviour {
    public BattleResult battleResultPrefab;
    public Texture2D unitCounter, pips;
    public Sprite[] unitSprite, Pips, allRegimentType;
    public Image[] Lider, Random, Moral,Flags;
    public Text[]  Infantry, Cavalry, Artillery,General;
    public Battle battle;
    public static BattleInterface instance;
    static List<Image> units = new List<Image>();
    static int stack=0;
    static Image getUnit()
    {
        if (units.Count == stack)
        {
            Image image = Instantiate(instance.Flags[0], instance.transform);
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(10f, 10f);
            units.Add(image);
        }
        units[stack].gameObject.SetActive(true);
        return units[stack++];
    }
    static void ResetUnits()
    {
        stack = 0;
        foreach (Image unit in units)
            unit.gameObject.SetActive(false);
    }
	void Awake () {
        instance = this;
        gameObject.SetActive(false);
        allRegimentType = unitSprite = new Sprite[3];
        for (int i = 0; i < 3; i++)
            unitSprite[i] = Sprite.Create(unitCounter, new Rect(10f * i, 0, 10f, 10f), new Vector2(0.5f, 0.5f));
        Pips = new Sprite[3];
        for (int i = 0; i < 3; i++)
            Pips[i] = Sprite.Create(pips, new Rect(32f * i, 0, 32, 52), new Vector2(0.5f, 0.5f));
        Random[0].sprite = Random[1].sprite = Pips[0];
    }
    public void Show(Battle battle)
    {
        needUpdate = false;
        ResetUnits();
        for (int k = 0; k < 2; k++)
        {
            Army army = battle.army[k];
            Flags[k].sprite = army.owner.flagSprite;
            Vector3Int count = battle.GetCount(k);
            Infantry[k].text = Format(count.x);
            Cavalry[k].text = Format(count.y);
            Artillery[k].text = Format(count.z);
            General[k].text = army.genegal.name;
            Random[k].GetComponentInChildren<Text>().text = battle.Rand[k].ToString();
            Moral[k].fillAmount = battle.GetMoral(k);
            if(army.genegal.pips[battle.phase]>0)
            {
                Lider[k].sprite = Pips[2 - battle.phase];
                Lider[k].GetComponentInChildren<Text>().text = army.genegal.pips[battle.phase].ToString();
                Lider[k].gameObject.SetActive(true);
            }
            else
            {
                Lider[k].gameObject.SetActive(false);
            }
            for(int i=0;i<80;i++)
                if(battle.F[k,i]!=null)
                {
                    Image im = getUnit();
                    im.GetComponent<RectTransform>().anchoredPosition = new Vector2(((i % 40) - 20) * 10f, (k == 0 ? 1 : -1) * ((i / 40) * 13f + 26f)) +
                        new Vector2(-15, -72) ;
                    Color c = army.owner.mainColor;
                    c.a = battle.F[k, i].count * 0.001f*0.8f+0.2f;
                    im.color = c;
                    im.sprite = unitSprite[(int)battle.F[k, i].baseRegiment.type];
                }
        }
    }
    public static void ShowBattle(Battle battle)
    {
        if(battle!=null)
        {
            instance.gameObject.SetActive(true);
            instance.Show(battle);
            instance.battle = battle;
        }
        else
        {
            instance.gameObject.SetActive(false);
            instance.battle = null;
        }
    }
    public void HiddenMenu()
    {
        ShowBattle(null);
    }
    static string Format(int c)
    {
        string s = c.ToString();
        return s;
    }
    public static bool needUpdate;
	void Update () {
        if (needUpdate && battle != null)
            Show(battle);

	}
}
