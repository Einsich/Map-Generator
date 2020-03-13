using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacyMenu : MonoBehaviour
{

    public Text stateName, war, alliance, access,casusbelli,descr,CasusBelliStatistic;
    public Button warbut, alliancebut, accessbut, casusbut;
    public GameObject actions, choise;
    public Slider actionSlider;
    public Image flag;
    void Start()
    {

    }
    public static State that, other;
    public static Diplomacy player, select;
    private void OnEnable()
    {
        GameTimer.OftenUpdate += UpdateCasusBelliStatistic;
    }
    private void OnDisable()
    {
        GameTimer.OftenUpdate -= UpdateCasusBelliStatistic;
    }
    void OpenYesNo(bool open)
    {
        choise.gameObject.SetActive(open);
        if (open)
        {
            InputManager.Instance.EnterAction += Yes;
            InputManager.Instance.EscAction += No;
        } else
        {
            InputManager.Instance.EnterAction -= Yes;
            InputManager.Instance.EscAction -= No;
        }
    }
    public void SchowDiplomacy(State pl, State sel)
    {
        if (pl == null) return;
        player = pl.diplomacy;
        that = pl;
        if (sel.diplomacy == select)
            return;
        select = sel.diplomacy;
        other = sel;
        SchowDiplomacy();
    }
    public void SchowDiplomacy()
    {
        stateName.text = other.name;
        flag.sprite = other.flagSprite;
        actions.SetActive(true);
        OpenYesNo(false);
        actionSlider.gameObject.SetActive(false);
        warbut.interactable = true;
        alliancebut.interactable = true;
        accessbut.interactable = true;
        UpdateCasusBelliStatistic();
        if (player.haveWar(select))
        {
            war.text = "Предложить мир";
            alliancebut.interactable = false;
            accessbut.interactable = false;
        }
        else
        {
            war.text = "Объявить войну";
        }
        if (player.haveAlliance(select))
        {
            alliance.text = "Разорвать союз";
            warbut.interactable = false;
            accessbut.interactable = false;
        }
        else
        {
            alliance.text = "Предложить союз";
        }
        if (player.haveAccess(select))
        {
            access.text = "Отменить право прохода";
        }
        else
        {
            access.text = "Предложить право прохода";
        }
        if (player.fabricatingCasus(select))
        {
            casusbelli.text = "Отменить фабрикацию к.б.";
        }
        else
        {
            casusbelli.text = "Начать фабрикацию к.б.";
        }
    }
    public static int dipstate;
    public void War()
    {
        actions.SetActive(false);
        OpenYesNo(true);
        dipstate = 0;
        if (player.haveWar(select))
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
        OpenYesNo(true);
        dipstate = 1;
        if (player.haveAlliance(select))
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
        OpenYesNo(true);
        dipstate = 2;
        if (player.haveAccess(select))
        {
            descr.text = "Вы действительно хотите отменить проход войск державе " + stateName.text + " ?";
        }
        else
        {
            descr.text = "Вы действительно хотите предложить проход войск державе " + stateName.text + " ?";
        }
    }
    public void FabricateCB()
    {
        actions.SetActive(false);
        OpenYesNo(true);
        dipstate = 3;
        if (player.fabricatingCasus(select))
        {
            descr.text = "Вы действительно хотите прекратить подготовку к.б. с " + stateName.text + " ?";
        }
        else
        {
        actionSlider.gameObject.SetActive(true);
            actionSlider.value = 0;
            descr.text = "Вы действительно хотите начать подготовку к.б. с " + stateName.text + " ?";
        }
    }
    public void Yes()
    {
        switch (dipstate)
        {
            case 0: player.DeclareWar(select, !player.haveWar(select)); break;
            case 1: player.MakeAlliance(select, !player.haveAlliance(select)); break;
            case 2: player.ForceAccess(select, !player.haveAccess(select)); break;
            case 3: player.FabricateCasusBelli(select, !player.fabricatingCasus(select), actionSlider.value);
                UpdateCasusBelliStatistic(); break;
        }
        that.RecalculArmyPath();
        other.RecalculArmyPath();
        SchowDiplomacy();
    }
    public void No()
    {

        SchowDiplomacy();
    }

    void UpdateCasusBelliStatistic()
    {
        float dcb = 0;
        float cb = player.casusbelli[other.ID];
        var i = player.fabricateCB.Find((x) => x.Item1 == select);
        if (i != default)
            dcb = i.Item2;
        CasusBelliStatistic.text = string.Format("Casus Belli:<color=#{2}> {0} </color>, (<color=#{3}> {1} </color>)",cb.ToString("N2"),dcb.ToString("N2"),
            cb>0? "ffa500ff":"ffffffff", dcb>0? "ff0000ff" : "ffffffff");
    }
}
