using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapPanel : MonoBehaviour
{
    public Image miniMap;
    public Button politic, terrein,menu;
    public void SetMiniMap()
    {
        miniMap.sprite = MainMenu.MiniMap;
        politic.onClick.AddListener(() => Main.instance.SetMapMode(MapMode.Politic));
        terrein.onClick.AddListener(() => Main.instance.SetMapMode(MapMode.Terrain));
        menu.onClick.AddListener(() => MenuManager.OpenGameMenu());
    }
}
