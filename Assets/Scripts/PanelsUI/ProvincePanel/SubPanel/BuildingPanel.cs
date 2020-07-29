using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPanel : MonoBehaviour
{
    private BuildingButton[] BuildingButtons;
    private void Awake()
    {
        BuildingButtons = GetComponentsInChildren<BuildingButton>();
        for (int i = 0; i < ProvinceData.buildingsCount; i++)
            BuildingButtons[i].BuildIndex = i;
    }
    public void ShowBuildings(bool show)
    {
        gameObject.SetActive(show);
        foreach (var button in BuildingButtons)
            button.OnEnable();
    }

}
