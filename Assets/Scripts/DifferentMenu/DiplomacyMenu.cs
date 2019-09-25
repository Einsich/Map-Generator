using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacyMenu : MonoBehaviour
{

    public Text stateName, war, alliance, access,descr;
    public Button warbut, alliancebut, accessbut;
    public GameObject actions, choise;
    public Image flag;
    void Start()
    {

    }
    public static Diplomacy dip;
    public static State player, select;
    public void SchowDiplomacy(State pl, State sel)
    {
        if (pl == null) return;
        player = pl;
        if (sel == select)
            return;
        select = sel;
        dip = Diplomacy.GetDiplomacy(pl, sel);
        SchowDiplomacy();
    }
    public void SchowDiplomacy()
    {
        stateName.text = select.name;
        flag.sprite = select.flagSprite;
        actions.SetActive(true);
        choise.SetActive(false);
        warbut.interactable = true;
        alliancebut.interactable = true;
        accessbut.interactable = true;
        if (dip.war)
        {
            war.text = "Предложить мир";
            alliancebut.interactable = false;
            accessbut.interactable = false;
        }
        else
        {
            war.text = "Объявить войну";
        }
        if (dip.alliance)
        {
            alliance.text = "Разорвать союз";
            warbut.interactable = false;
            accessbut.interactable = false;
        }
        else
        {
            alliance.text = "Предложить союз";
        }
        if (dip.forceaccess)
        {
            access.text = "Отменить право прохода";
        }
        else
        {
            access.text = "Предложить право прохода";
        }
    }
    public static int dipstate;
    public void War()
    {
        actions.SetActive(false);
        choise.SetActive(true);
        dipstate = 0;
        if (dip.war)
        {
            descr.text = "Вы действительно хотите предложить мир державе " + stateName.text+" ?";
        }
        else
        {
            descr.text = "Вы действительно хотите объявить войну державе " + stateName.text + " ?";
        }
    }
    public void Alliance()
    {
        actions.SetActive(false);
        choise.SetActive(true);
        dipstate = 1;
        if (dip.alliance)
        {
            descr.text = "Вы действительно хотите разорвать союз с державой " + stateName.text + " ?";
        }
        else
        {
            descr.text = "Вы действительно хотите предложить союз державе " + stateName.text + " ?";
        }
    }
    public void Access()
    {
        actions.SetActive(false);
        choise.SetActive(true);
        dipstate = 2;
        if (dip.forceaccess)
        {
            descr.text = "Вы действительно хотите отменить проход войск державе " + stateName.text + " ?";
        }
        else
        {
            descr.text = "Вы действительно хотите предложить проход войск державе " + stateName.text + " ?";
        }
    }
    public void Yes()
    {
        switch (dipstate)
        {
            case 0: dip.war = !dip.war; break;
            case 1: dip.alliance = !dip.alliance; break;
            case 2: dip.forceaccess = !dip.forceaccess; break;
        }
        dip.s1.RecalculArmyPath();
        dip.s2.RecalculArmyPath();
        SchowDiplomacy();
    }
    public void No()
    {

        SchowDiplomacy();
    }
}
