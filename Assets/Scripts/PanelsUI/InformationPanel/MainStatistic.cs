using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainStatistic : MonoBehaviour {

    public static MainStatistic Instance;
    public Image flagbut;
    public Text gold, manPower,wood,iron,science;
    public GameObject panel, personPanel;
    public ListFiller PersonFiller;
    public TechnologyPanel technologyPanel;
    public EconomicPanel economicPanel;
    [Header("State, diplomacy, army, research, economy, persons")]
    public Button[] ModeButton;
    public ButtonSelector buttonSelector;
    State state;
    private void Awake()
    {
        Instance = this;
        buttonSelector = new ButtonSelector(ModeButton, new ShowSmth[] { Nope, Nope, Nope, ShowTechnology, Nope, ShowPersons }) ;
    }
    public void SetState(State state)
    {
        flagbut.sprite = state.flagSprite;
        this.state = state;
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
            PersonFiller.UpdateList(state.persons.ConvertAll(x => (object)x));
        personPanel.SetActive(show);
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
