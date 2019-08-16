using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainStatistic : MonoBehaviour {

    public static MainStatistic instance;
    public SpriteRenderer flagbut;
    public Text gold, manPower,wood,iron,science;
    public GameObject panel;
    private void Awake()
    {
        instance = this;
    }
	public void SetState(State state)
    {
        flagbut.sprite = Sprite.Create(state.flag, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        ShowData(state);
    }
    public void ShowData(State state)
    {
        ShowResources(state);
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
    public void PanelShowMode()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
