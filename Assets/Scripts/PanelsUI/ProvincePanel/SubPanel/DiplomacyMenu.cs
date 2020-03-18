using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacyMenu : MonoBehaviour
{

    public Text stateName, war, trade, access, casusbelli, insult, patron, union, descr, Relation, EnemyIs, selltext,buytext;
    public Button warbut, tradebut, accessbut, casusbut, insultbut, patronbut, unionbut, YesButton;
    public GameObject actions, choise;
    public Slider actionSlider, sellres;
    public Image flag;
    public Dropdown selldrop, buydrop;
    public GameObject tradepanel;
    void Start()
    {
        warbut.onClick.AddListener(War);
        tradebut.onClick.AddListener(Trade);
        accessbut.onClick.AddListener(Access);
        casusbut.onClick.AddListener(FabricateCB);
        insultbut.onClick.AddListener(Insult);
        patronbut.onClick.AddListener(Patronage);
        unionbut.onClick.AddListener(Union);
        selldrop.onValueChanged.AddListener((x) => UpdateTradePanel());
        buydrop.onValueChanged.AddListener((x) => UpdateTradePanel());
        sellres.onValueChanged.AddListener((x) => UpdateTradePanel());
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
    ResourcesType sellT, buyT;
    float sellR, buyR;
    void UpdateTradePanel()
    {
        sellT = (ResourcesType)selldrop.value;
        buyT = (ResourcesType)buydrop.value;
        sellR = sellres.value;
        buyR = GlobalTrade.GetCource(buyT, sellT) * sellR;
        bool want = select.state.WantTrade(buyT, buyR, sellT, sellR);
        selltext.text = $"Продаете\n{sellT.ToString()}: {sellR:N1}";
        buytext.text = $"Покупаете\n{buyT.ToString()}: {buyR:N1}";
        YesButton.interactable = want;
        descr.text = want ? $"{stateName.text} готова торговать с вами." : $"{stateName.text} не интересует ваше предложение.";
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
        tradepanel.SetActive(false);
        UpdateCasusBelliStatistic();

        war.text = player.haveWar(select) ? "Предложить мир" : "Объявить войну";
        trade.text = player.haveDeal(select)?"Расторгнуть сделку": "Предложить сделку";
        access.text = player.haveAccess(select) ? "Отменить право прохода" : "Предложить право прохода";
        casusbelli.text = player.fabricatingCasus(select) ? "Отменить фабрикацию к.б." : "Начать фабрикацию к.б.";
        insult.text = "Испортить отношения";
        patron.text = player.havePatronage(select)?"Отменить покровительство":"Стать покровителем";
        union.text = "Присоединить";

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
            YesButton.interactable = true;
        }
        else
        {
            bool can = player.canDeclareWar(select);
            YesButton.interactable = can;
            descr.text = can ? $"Вы действительно хотите объявить войну державе {stateName.text} ?" :
                $"Вы не можете объявить {stateName.text} войну";
        }
    }
    public void Trade()
    {
        actions.SetActive(false);
        OpenYesNo(true);
        dipstate = 1;
        if (player.haveDeal(select))
        {
            descr.text = $"Вы действительно хотите расторгнуть сделку с державой { stateName.text } ?";
            YesButton.interactable = true;
        }
        else
        {
            bool can = player.canTrade(select);
            YesButton.interactable = can;
            
            if (!can)
            {
                descr.text = $"Вы не можете торговать с {stateName.text}";
                
            } else
            {
                tradepanel.SetActive(true);
                sellres.maxValue = 10;
                UpdateTradePanel();
            }
        }
    }
    public void Access()
    {
        actions.SetActive(false);
        OpenYesNo(true);
        dipstate = 2;
        if (player.haveAccess(select))
        {
            descr.text = $"Вы действительно хотите отменить проход войск державе {stateName.text } ?";
            YesButton.interactable = true;
        }
        else
        {
            bool can = player.canForceAcces(select);
            YesButton.interactable = can;
            descr.text = can ? $"Вы действительно хотите попросить проход войск у державы {stateName.text} ?" :
                $"Вы не можете просить проход войск у {stateName.text}";
        }        
    }
    public void FabricateCB()
    {
        actions.SetActive(false);
        OpenYesNo(true);
        dipstate = 3;
        if (player.fabricatingCasus(select))
        {
            descr.text = $"Вы действительно хотите прекратить подготовку к.б. с { stateName.text } ?";
            YesButton.interactable = true;
        }
        else
        {
            bool can = player.canForceAcces(select);
            actionSlider.gameObject.SetActive(can);
            actionSlider.value = 0;
            YesButton.interactable = can;
            descr.text = can ? $"Вы действительно хотите начать подготовку к.б. с { stateName.text } ?" :
                $"Вы не можете готовить к.б. с {stateName.text}";
        }
    }
    public void Insult()
    {
        actions.SetActive(false);
        OpenYesNo(true);
        dipstate = 4;
        bool can = player.canInsulting(select);
        YesButton.interactable = can;
        descr.text = can ? $"Вы действительно хотите испортить отношения с {stateName.text} ?" :
            $"Вы не можете испортить и так плохие отношения с {stateName.text}";
    }
    public void Patronage()
    {
        actions.SetActive(false);
        OpenYesNo(true);
        dipstate = 5;
        if (player.havePatronage(select))
        {
            descr.text = $"Вы действительно хотите прекратить покровительство державе {stateName.text } ?";
            YesButton.interactable = true;
        }
        else
        {
            bool can = player.canPatronage(select);
            YesButton.interactable = can;
            descr.text = can ? $"Вы действительно хотите стать покровителем для державы {stateName.text} ?" :
                $"Вы не можете стать покровителем {stateName.text}";
        }
    }
    public void Union()
    {
        actions.SetActive(false);
        OpenYesNo(true);
        dipstate = 5;
        bool can = player.canUniate(select);
        YesButton.interactable = can;
        descr.text = can ? $"Вы действительно хотите присоединить {stateName.text} ?" :
            $"Вы не можете присоединить {stateName.text}, необходимы отношения 70";
    }
    public void Yes()
    {
        switch (dipstate)
        {
            case 0: player.DeclareWar(select, !player.haveWar(select));
                break;
            case 1: if (player.haveDeal(select))
                    player.BreakTradeDeal(select);
                else
                    player.SendOfferTradeDeal(select, new TradeDeal(player.state, sellT, sellR, select.state, buyT, buyR));
                break;
            case 2: if (player.haveAccess(select))
                    player.ForceAccess(select, false);
                else
                    player.SendOfferForceAccess(select);
                break;
            case 3:
                player.FabricateCasusBelli(select, !player.fabricatingCasus(select), actionSlider.value);
                break;
            case 4:
                player.Insult(select);
                break;
            case 5:
                player.BeginPatronage(select, !player.havePatronage(select));
                break;
            case 6:
                player.Uniate(select);
                break;
        }
        that.RecalculArmyPath();
        other.RecalculArmyPath();
        SchowDiplomacy();
    }
    public void No()
    {

        SchowDiplomacy();
    }
    public static string ColoredRelation(float relation, float delta)
    {
        Color r, d;
        if (relation < 0)
            r = Color.Lerp(Color.red, Color.white, Mathf.Sqrt((relation + 100) * 0.01f));
        else
            r = Color.Lerp(Color.white, Color.green, Mathf.Sqrt(relation * 0.01f));

        d = delta < 0 ? Color.red : delta > 0 ? Color.green : Color.white;
        return string.Format("Отношения:<color=#{2}> {0} </color>, (<color=#{3}> {1} </color>)", relation.ToString("N1"), delta.ToString("N1"),
            ColorUtility.ToHtmlStringRGB(r), ColorUtility.ToHtmlStringRGB(d));
    }
    void UpdateCasusBelliStatistic()
    {
        Relation.text = ColoredRelation(player.relation[other.ID],player.relationDelta(select));
    }
}
