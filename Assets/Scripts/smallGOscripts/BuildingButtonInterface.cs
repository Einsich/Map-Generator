using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButtonInterface : MonoBehaviour {

    //состояния 0-недоступно, 1-можно строить, 2-строится
    //ничего нельзя поделать, можно построить, прекратить постройку
    Image image,state;
    Button button;
    public int ind;
    bool isBuild = false;
    void Awake()
    {
        image = GetComponent<Image>();
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(PressButton);
        state = button.GetComponent<Image>();
    }
    void Update () {
        if (button.interactable)
        {
            float s = Mathf.Cos(Time.time * 3);
            state.color = Color.Lerp(Color.white, dark, s * s);
            if(isBuild && ProvinceMenu.current.data.action != null)
                image.fillAmount = ProvinceMenu.current.data.action.progress;
        }
        
	}
    static Color trans = new Color(0, 0, 0, 0),
        dark = new Color(1, 1, 1, 0.4f);
    public void SetImage(Sprite sprite,BuildState st,int lvl)
    {
        image.sprite = sprite;
        button.GetComponentInChildren<Text>().text = (lvl).ToString();
        if(st != BuildState.isBuilding)
        image.fillAmount = 1f;
        if (st == BuildState.CantBuild)
        {
            button.interactable = false;
            state.sprite = null;
            image.color = Color.gray;
            state.color = trans;
        }
        else
        {
            button.interactable = true;
            image.color = Color.white;
            state.sprite = ProvinceMenu.that.buildState[(int)st - 1];
            isBuild = st == BuildState.isBuilding;
            
        }
    }
    public void PressButton()
    {
        ProvinceMenu.current.data.ClickBuilding(ind);
        ProvinceMenu.that.ShowProvinceMenu(null);
    }

}
