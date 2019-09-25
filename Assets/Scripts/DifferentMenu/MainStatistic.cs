using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainStatistic : MonoBehaviour {

    static MainStatistic instance;
    public Image flagbut;
    public Text gold, manPower,wood,iron,science;
    public GameObject panel, personPanel;
    public ListFiller PersonFiller;
    State state;
    private void Awake()
    {
        instance = this;
        Person.icons = Resources.LoadAll<Sprite>("PersonIcons");
    }
    private void Update()
    {
        if (state != null)
            ShowPersons();
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
        ShowPersons();
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
    public void ShowPersons()
    {
        PersonFiller.UpdateList(state.persons.ConvertAll(x => (object)x));
        personPanel.SetActive(true);
    }
    public void PanelShowMode()
    {
        panel.SetActive(!panel.activeSelf);
        if (panel.gameObject.activeSelf && Player.curPlayer != null)
            ShowPersons();
    }
}
