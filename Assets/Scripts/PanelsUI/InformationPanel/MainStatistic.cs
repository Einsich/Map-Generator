using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainStatistic : MonoBehaviour {

    public static MainStatistic Instance;
    public Image flagbut;
    public Text gold, manPower,wood,iron,science;
    public GameObject panel;
    public PersonPanel personPanel;
    public TechnologyPanel technologyPanel;
    public EconomicPanel economicPanel;
    [Header("State, diplomacy, army, research, economy, persons")]
    public Button[] ModeButton;
    public ButtonSelector buttonSelector;
    State state;
    private void Awake()
    {
        Instance = this;
        personPanel = Instantiate(personPanel, panel.transform);
        technologyPanel = Instantiate(technologyPanel, panel.transform);
        economicPanel = Instantiate(economicPanel, panel.transform);
        var actions = new ShowSmth[] { Nope, Nope, Nope, ShowTechnology, ShowEconomic, ShowPersons };
        buttonSelector = new ButtonSelector(ModeButton, actions) ;
       
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
    public void ShowTechnology(bool show)
    {
        if (show)
            technologyPanel.ShowTechnology(state.technology);
        technologyPanel.gameObject.SetActive(show);
    }
    public void PanelShowMode()
    {
        panel.SetActive(!panel.activeSelf);
        buttonSelector.Update();
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
