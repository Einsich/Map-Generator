using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainStatistic : MonoBehaviour {

    public static MainStatistic Instance;
    public Image flagbut;
    public Text gold, manPower,wood,iron,science;
    public GameObject panel;
    public GameObject tabsPanel;
    public PersonPanel personPanel;
    public TechnologyPanel technologyPanel;
    public EconomicPanel economicPanel;
    public DiplomacyPanel diplomacyPanel;
    public TradePanel tradePanel;
    [Header("Trade, diplomacy, army, research, economy, persons")]
    public Button[] ModeButton;
    public ButtonSelector buttonSelector;
    private PanelMover PanelMover;
    State state;
    private void Awake()
    {
        Instance = this;
        personPanel = Instantiate(personPanel, tabsPanel.transform);
        technologyPanel = Instantiate(technologyPanel, tabsPanel.transform);
        economicPanel = Instantiate(economicPanel, tabsPanel.transform);
        diplomacyPanel = Instantiate(diplomacyPanel, tabsPanel.transform);
        tradePanel = Instantiate(tradePanel, tabsPanel.transform);
        var actions = new ShowSmth[] { ShowTrade, ShowDiplomacy, Nope, ShowTechnology, ShowEconomic, ShowPersons };
        buttonSelector = new ButtonSelector(ModeButton, actions) ;
        PanelMover = panel.GetComponent<PanelMover>();


    }
    public void SetState(State state)
    {
        flagbut.sprite = state.flagSprite;
        if (this.state != null)
            this.state.TreasureChange -= ShowResources;

        this.state = state;

        state.TreasureChange += ShowResources;
        ShowData();
    }
    public void ShowData()
    {
        ShowResources();
    }
    public void ShowResources()
    {
        Treasury t = state.treasury;
        gold.text = t.Gold.ToString("N1");
        manPower.text = t.Manpower.ToString("N1");
        wood.text = t.Wood.ToString("N1");
        iron.text = t.Iron.ToString("N1");
        science.text = t.Science.ToString("N1");
    }
    public void ShowEconomic(bool show)
    {
        if (show)
            economicPanel.ShowEconomic(state);
        economicPanel.gameObject.SetActive(show);
    }
    public void ShowPersons(bool show)
    {
        if (show)
            personPanel.Show(state.persons);
        personPanel.gameObject.SetActive(show);
    }
    public void ShowDiplomacy(bool show)
    {
        if (show)
        {
            diplomacyPanel.Show(state.diplomacy);

            state.diplomacy.DiplomacyAction += diplomacyPanel.UpdateInfo;
        } else
        {

            if(state != null)
            state.diplomacy.DiplomacyAction -= diplomacyPanel.UpdateInfo;
        }
        diplomacyPanel.gameObject.SetActive(show);
    }
    public void ShowTechnology(bool show)
    {
        if (show)
            technologyPanel.ShowTechnology(state.technologyTree);
        technologyPanel.gameObject.SetActive(show);
    }
    public void ShowTrade(bool show)
    {
        if (show)
            tradePanel.Show(state);
        tradePanel.gameObject.SetActive(show);
    }
    public void PanelShowMode()
    {
        bool show = !PanelMover.isOn;

        if (show)
            buttonSelector.Update();
        if (show)
            PanelMover.Show();
        else
            PanelMover.Hide();
    }
    public void Nope(bool show) { }
    private void OnEnable()
    {
        GameTimer.OftenUpdate += () => buttonSelector.Update();   
    }
    private void OnDisable()
    {
        GameTimer.OftenUpdate -= () => buttonSelector.Update();

    }
}
