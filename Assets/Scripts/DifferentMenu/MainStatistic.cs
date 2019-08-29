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
    private void Awake()
    {
        instance = this;
        Person.icons = Resources.LoadAll<Sprite>("PersonIcons");
    }
	public void SetState(State state)
    {
        flagbut.sprite = state.flagSprite;
        ShowData(state);
    }
    public void ShowData(State state)
    {
        ShowResources(state);
        ShowPersons(state);
    }
    public void ShowResources(State state)
    {
        Treasury t = state.treasury;
        gold.text = t.Gold.ToString();
        manPower.text = t.Manpower.ToString();
        wood.text = t.Wood.ToString();
        iron.text = t.Iron.ToString();
        science.text = t.Science.ToString();
    }
    public void ShowPersons(State state)
    {
        PersonFiller.UpdateList(state.persons.ConvertAll(x => (object)x));
        personPanel.SetActive(true);
    }
    public void PanelShowMode()
    {
        panel.SetActive(!panel.activeSelf);
        if (panel.gameObject.activeSelf && Player.curPlayer != null)
            ShowPersons(Player.curPlayer);
    }
}
